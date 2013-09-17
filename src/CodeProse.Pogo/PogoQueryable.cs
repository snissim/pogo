using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Google.Apis.Datastore.v1beta1.Data;
using CodeProse.Pogo.Mapping;

namespace CodeProse.Pogo
{
    internal class PogoQueryable<T> : IPogoQueryable<T>, IPogoQueryCustomization where T : new()
    {
        DatastoreSession _session;
        Query _query;
        Filter _filter;
        string _readConsistency;

        public PogoQueryable(DatastoreSession session) : this(session, new Query(), null, "default")
        {
        }

        public PogoQueryable(DatastoreSession session, Query query, Filter filter, string readConsistency)
        {
            _session = session;
            _query = query;
            _filter = filter;
            _readConsistency = readConsistency;
        }

        private string InvertOperator(string operatorType)
        {
            switch (operatorType)
            {
                case "lessThan":
                    return "greaterThan";

                case "lessThanOrEqual":
                    return "greaterThanOrEqual";

                case "greaterThan":
                    return "lessThan";

                case "greaterThanOrEqual":
                    return "lessThanOrEqual";
            }

            return operatorType;
        }

        public IPogoQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            //VisitExpression(predicate);

            if (predicate is LambdaExpression)
            {
                _filter = ConvertToFilterRecursive(predicate.Body);

                while (CanReduce(_filter))
                {
                    Reduce(_filter);
                }
            }

            return this;
        }

        private Filter ConvertToFilterRecursive(Expression expression)
        {
            var filter = new Filter();

            if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;

                switch (methodExpression.Method.Name)
                {
                    case "Equals":
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "equal");
                        break;

                    case "Contains":
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "equal");
                        break;

                    // case "Any":
                    // should be recursive, but need to intercept property name from first argument

                    default:
                        throw new Exception("Method not supported: " + methodExpression.Method.Name);
                }
            }
            else
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.LessThan:
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "lessThan");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "lessThanOrEqual");
                        break;

                    case ExpressionType.GreaterThan:
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "greaterThan");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "greaterThanOrEqual");
                        break;

                    case ExpressionType.Equal:
                        filter.PropertyFilter = ConvertToPropertyFilter(expression, "equal");
                        break;

                    case ExpressionType.AndAlso:
                        var binaryExpression = (BinaryExpression)expression;
                        filter.CompositeFilter = new CompositeFilter { Operator = "and", Filters = new List<Filter>() };
                        filter.CompositeFilter.Filters.Add(ConvertToFilterRecursive(binaryExpression.Left));
                        filter.CompositeFilter.Filters.Add(ConvertToFilterRecursive(binaryExpression.Right));
                        break;

                    // hasAncestor

                    default:
                        throw new Exception("Operator not supported: " + expression.NodeType.ToString());
                }
            }

            return filter;
        }

        private PropertyFilter ConvertToPropertyFilter(Expression expression, string operatorType)
        {
            var filter = new PropertyFilter();
            filter.Operator = operatorType;

            if (expression is BinaryExpression)
            {
                var binaryExpression = (BinaryExpression)expression;
                if (binaryExpression.Left is UnaryExpression)
                {
                    filter.Operator = InvertOperator(filter.Operator);
                    filter.Property = ConvertExpressionToProperty((MemberExpression)binaryExpression.Right);
                    filter.Value = ConvertExpressionToValue(binaryExpression.Left);
                }
                else
                {
                    filter.Property = ConvertExpressionToProperty((MemberExpression)binaryExpression.Left);
                    filter.Value = ConvertExpressionToValue(binaryExpression.Right);
                }
            }
            else if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;

                switch (methodExpression.Method.Name)
                {
                    case "Equals":
                        filter.Property = ConvertExpressionToProperty((MemberExpression)methodExpression.Object);
                        filter.Value = ConvertExpressionToValue(methodExpression.Arguments[0]);
                        break;

                    case "Contains":
                        filter.Property = ConvertExpressionToProperty((MemberExpression)methodExpression.Arguments[0]);
                        filter.Value = ConvertExpressionToValue(methodExpression.Arguments[1]);
                        break;
                }
            }

            return filter;
        }

        private static Value ConvertExpressionToValue(Expression expression)
        {
            var obj = ConvertExpressionToObject(expression);

            var value = ObjectEntityMapper.ConvertNonCollectionObjectToEntityPropertyValue(obj);

            return value;
        }

        private static object ConvertExpressionToObject(Expression expression)
        {
            object value = null;

            if (expression is UnaryExpression)
            {
                return ConvertExpressionToObject(((UnaryExpression)expression).Operand);
            }
            else if (expression is MemberInitExpression)
            {
                value = Expression.Lambda(expression).Compile().DynamicInvoke();
            }
            else if (expression is NewExpression)
            {
                value = GetNewExpressionValue(expression);
            }
            else
            {
                value = ((ConstantExpression)expression).Value;
            }

            return value;
        }

        private static object GetNewExpressionValue(Expression expression)
        {
            var newExpression = ((NewExpression)expression);
            var instance = Activator.CreateInstance(newExpression.Type, newExpression
                .Arguments
                .Select(e => ConvertExpressionToObject(e))
                .ToArray());
            return instance;
        }

        private PropertyReference ConvertExpressionToProperty(MemberExpression expression)
        {
            var propertyReference = new PropertyReference();
            propertyReference.Name = expression.Member.Name;

            return propertyReference;
        }

        private void Reduce(Filter filter)
        {
            var filterToEliminate = filter.CompositeFilter.Filters.Single(t => t.CompositeFilter != null);
            var filters = filterToEliminate.CompositeFilter.Filters;
            ((List<Filter>)filter.CompositeFilter.Filters).AddRange(filters);

            filter.CompositeFilter.Filters.Remove(filterToEliminate);
        }

        private bool CanReduce(Filter filter)
        {
            return filter.CompositeFilter != null
                && filter.CompositeFilter.Filters.Any(t => t.CompositeFilter != null)
                && filter.CompositeFilter.Filters.Any(t => t.PropertyFilter != null);
        }

        public IPogoQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector) where TResult : new()
        {
            _query.Projection = new List<PropertyExpression>();

            var lambdaExpression = (LambdaExpression)selector;

            if (lambdaExpression.Body is MemberExpression)
            {
                var memberExpression = (MemberExpression)lambdaExpression.Body;

                _query.Projection.Add(new PropertyExpression { Property = new PropertyReference { Name = memberExpression.Member.Name } });
            }
            else if (lambdaExpression.Body is MemberInitExpression)
            {
                var memberInitExpression = (MemberInitExpression)lambdaExpression.Body;

                foreach (var args in memberInitExpression.Bindings)
                {
                    _query.Projection.Add(new PropertyExpression { Property = new PropertyReference { Name = ((MemberBinding)args).Member.Name } });
                }
            }

            return new PogoQueryable<TResult>(_session, _query, _filter, _readConsistency);
        }

        public IPogoQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query.Order = new List<PropertyOrder>();

            var lambdaExpression = (LambdaExpression)keySelector;

            if (lambdaExpression.Body is MemberExpression)
            {
                var memberExpression = (MemberExpression)lambdaExpression.Body;

                _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = memberExpression.Member.Name } });
            }
            else if (lambdaExpression.Body is NewExpression)
            {
                var newExpression = (NewExpression)lambdaExpression.Body;

                foreach (var args in newExpression.Arguments)
                {
                    _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = ((MemberExpression)args).Member.Name } });
                }
            }

            return this;
        }

        public IPogoQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query.Order = new List<PropertyOrder>();

            var lambdaExpression = (LambdaExpression)keySelector;

            if (lambdaExpression.Body is MemberExpression)
            {
                var memberExpression = (MemberExpression)lambdaExpression.Body;

                _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = memberExpression.Member.Name }, Direction = "descending" });
            }
            else if (lambdaExpression.Body is NewExpression)
            {
                var newExpression = (NewExpression)lambdaExpression.Body;

                foreach (var args in newExpression.Arguments)
                {
                    _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = ((MemberExpression)args).Member.Name }, Direction = "descending" });
                }
            }

            return this;
        }

        public IPogoQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var memberExpression = (MemberExpression)((LambdaExpression)keySelector).Body;

            _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = memberExpression.Member.Name } });

            return this;
        }

        public IPogoQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var memberExpression = (MemberExpression)((LambdaExpression)keySelector).Body;

            _query.Order.Add(new PropertyOrder { Property = new PropertyReference { Name = memberExpression.Member.Name }, Direction = "descending" });

            return this;
        }

        public IPogoQueryable<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query.GroupBy = new List<PropertyReference>();

            var lambdaExpression = (LambdaExpression)keySelector;

            if (lambdaExpression.Body is MemberExpression)
            {
                var memberExpression = (MemberExpression)lambdaExpression.Body;

                _query.GroupBy.Add(new PropertyReference { Name = memberExpression.Member.Name });
            }
            else if (lambdaExpression.Body is NewExpression)
            {
                var newExpression = (NewExpression)lambdaExpression.Body;

                foreach (var args in newExpression.Arguments)
                {
                    _query.GroupBy.Add(new PropertyReference { Name = ((MemberExpression)args).Member.Name });
                }
            }

            return this;
        }

        public IPogoQueryable<T> Skip(int count)
        {
            _query.Offset = count;

            return this;
        }

        public IPogoQueryable<T> Take(int count)
        {
            _query.Limit = count;

            return this;
        }

        public Query ToQuery()
        {
            _query.Kinds = new List<KindExpression>();
            _query.Kinds.Add(new KindExpression { Name = typeof(T).FullName });
            _query.Filter = _filter;

            return _query;
        }

        public RunQueryRequest GetRequest()
        {
            var request = new RunQueryRequest();
            request.Query = ToQuery();

            if (!_readConsistency.Equals("default"))
            {
                request.ReadOptions = new ReadOptions();
                request.ReadOptions.ReadConsistency = _readConsistency;
            }

            return request;
        }

        public List<T> ToList()
        {
            var request = GetRequest();

            var response = _session.Service.Datasets.RunQuery(request, _session.DatasetId).Fetch();

            if (response.Batch != null && response.Batch.EntityResults != null)
            {
                // unit of work
                _session.Store(response.Batch.EntityResults.Select(t => ObjectEntityMapper.ConvertEntityToObject<T>(t.Entity)));

                var objectList = response.Batch.EntityResults.Select(t => ObjectEntityMapper.ConvertEntityToObject<T>(t.Entity)).ToList();

                // TODO cursor through more results

                return objectList;
            }

            return new List<T>();
        }

        public IPogoQueryable<T> Customize(Action<IPogoQueryCustomization> action)
        {
            action(this);

            return this;
        }

        public IPogoQueryCustomization WaitForNonStaleResults()
        {
            _readConsistency = "strong";

            return this;
        }
    }
}
