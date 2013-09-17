using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;
using Google.Apis.Datastore.v1beta1.Data;

namespace CodeProse.Pogo
{
    public interface IPogoQueryable<T> //: IEnumerable<T>, IEnumerable
    {
        IPogoQueryable<T> Where(Expression<Func<T, bool>> predicate);

        IPogoQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IPogoQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        IPogoQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IPogoQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        IPogoQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector) where TResult : new();

        IPogoQueryable<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IPogoQueryable<T> Skip(int count);

        IPogoQueryable<T> Take(int count);

        IPogoQueryable<T> Customize(Action<IPogoQueryCustomization> action);

        Query ToQuery();

        RunQueryRequest GetRequest();

        List<T> ToList();
    }
}
