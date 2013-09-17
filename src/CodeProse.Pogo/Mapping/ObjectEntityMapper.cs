using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Google.Apis.Datastore.v1beta1.Data;
using System.Xml;
using System.Collections;
using Newtonsoft.Json;

namespace CodeProse.Pogo.Mapping
{
    public class ObjectEntityMapper
    {
        public static Entity ConvertObjectToEntity(object poco)
        {
            if (poco == null) throw new ArgumentNullException("poco");

            var entity = new Entity();

            string id = GetIdValue(poco);
            if (!string.IsNullOrEmpty(id))
            {
                entity.Key = new Key();
                entity.Key.Path = new List<KeyPathElement>();
                entity.Key.Path.Add(new KeyPathElement { Name = id, Kind = poco.GetType().FullName });
            }

            entity.Properties = new Entity.PropertiesData();
            var properties = poco.GetType().GetProperties().Where(t => t != GetIdProperty(poco) && !t.IsDefined(typeof(JsonIgnoreAttribute), true)); // should inherit really be true?
            foreach (var propertyInfo in properties)
            {
                object propertyValue = propertyInfo.GetValue(poco, null);
                if (propertyValue != null)
                {
                    var entityProperty = new Property();
                    entityProperty.Values = ConvertObjectPropertyToEntityPropertyValues(propertyValue, propertyInfo.PropertyType);
                    if (IsMulti(propertyInfo.PropertyType)) // fix this duplicate logic
                    {
                        entityProperty.Multi = true;
                    }

                    string propertyName = propertyInfo.Name;
                    if (propertyInfo.IsDefined(typeof(JsonPropertyAttribute), true))
                    {
                        var attributes = propertyInfo.GetCustomAttributes(true)
                            .Where(t => t.GetType() == typeof(JsonPropertyAttribute))
                            .Select(t => (JsonPropertyAttribute)t)
                            .ToList();

                        if (attributes.Count() > 1)
                        {
                            throw new Exception("Property contains more than one JsonProperty attribute: " + propertyInfo.Name);
                        }

                        propertyName = attributes[0].PropertyName;
                    }

                    entity.Properties.Add(propertyName, entityProperty);
                }
            }

            return entity;
        }

        public static object ConvertEntityToObject(Entity entity, Type entityType)
        {
            var obj = Activator.CreateInstance(entityType);

            // TODO set ancestor path based on inheritance (so entities can be queried using polymorphism)

            var idProperty = GetIdProperty(entityType);
            if (idProperty != null && entity.Key != null)
            {
                var keyPath = entity.Key.Path[0];
                if (keyPath.Name != null)
                {
                    idProperty.SetValue(obj, keyPath.Name, null);
                }
                else
                {
                    idProperty.SetValue(obj, keyPath.Id, null); // TODO verify?
                }
            }

            var properties = entityType.GetProperties().Where(t => t != idProperty && !t.IsDefined(typeof(JsonIgnoreAttribute), true)); // duplicate code (see above method)
            foreach (var propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                if (propertyInfo.IsDefined(typeof(JsonPropertyAttribute), true))
                {
                    var attributes = propertyInfo.GetCustomAttributes(true)
                        .Where(t => t.GetType() == typeof(JsonPropertyAttribute))
                        .Select(t => (JsonPropertyAttribute)t)
                        .ToList();

                    if (attributes.Count() > 1)
                    {
                        throw new Exception("Property contains more than one JsonProperty attribute: " + propertyInfo.Name);
                    }

                    propertyName = attributes[0].PropertyName;
                }

                if (entity.Properties.ContainsKey(propertyName))
                {
                    var entityValue = entity.Properties[propertyName];

                    var value = ConvertEntityPropertyToObjectProperty(entityValue, propertyInfo.PropertyType);

                    propertyInfo.SetValue(obj, value, null);
                }
            }

            return obj;
        }

        public static T ConvertEntityToObject<T>(Entity entity) where T : new()
        {
            return (T)ConvertEntityToObject(entity, typeof(T));
        }

        public static object ConvertEntityPropertyToObjectProperty(Property entityValue, Type propertyType)
        {
            if (entityValue.Multi.HasValue && entityValue.Multi.Value)
            {
                var elementType = GetElementType(propertyType);
                var array = Array.CreateInstance(elementType, entityValue.Values.Count);

                for (int i = 0; i < entityValue.Values.Count; i++)
                {
                    var value = entityValue.Values[i];

                    var obj = ConvertEntityPropertyValueToObject(value, elementType);

                    array.SetValue(obj, i);
                }

                return array;
            }

            if (entityValue.Values.Count > 0)
            {
                var value = entityValue.Values[0];

                return ConvertEntityPropertyValueToObject(value, propertyType);
            }

            return null;
        }

        public static object ConvertEntityPropertyValueToObject(Value value, Type propertyType)
        {
            if (value.StringValue != null)
            {
                return value.StringValue;
            }

            if (value.BooleanValue != null)
            {
                return value.BooleanValue;
            }

            if (value.BlobValue != null)
            {
                byte[] data = value.BlobValue.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();

                return data;
            }

            if (value.DateTimeValue != null)
            {
                return XmlConvert.ToDateTime(value.DateTimeValue, XmlDateTimeSerializationMode.Unspecified);
            }

            if (value.DoubleValue != null)
            {
                return value.DoubleValue;
            }

            if (value.IntegerValue != null)
            {
                // if nullable, get underlying type
                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    var underlyingPropertyType = Nullable.GetUnderlyingType(propertyType);

                    var underlyingValue = Convert.ChangeType(value.IntegerValue, underlyingPropertyType);

                    MethodInfo castMethod = typeof(ObjectEntityMapper).GetMethod("Cast").MakeGenericMethod(propertyType);
                    return castMethod.Invoke(null, new object[] { underlyingValue });
                }

                return Convert.ChangeType(value.IntegerValue, propertyType);
            }
            if (value.EntityValue != null)
            {
                return ConvertEntityToObject(value.EntityValue, propertyType);
            }

            return null;
        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }

        public static IList<Value> ConvertObjectPropertyToEntityPropertyValues(object propertyValue)
        {
            return ConvertObjectPropertyToEntityPropertyValues(propertyValue, propertyValue.GetType());
        }

        public static IList<Value> ConvertObjectPropertyToEntityPropertyValues(object propertyValue, Type propertyType)
        {
            IList<Value> values = new List<Value>();

            if (IsMulti(propertyType))
            {
                var elementType = GetElementType(propertyType);

                foreach (var item in (IEnumerable)propertyValue)
                {
                    values.Add(ConvertNonCollectionObjectToEntityPropertyValue(item, elementType));
                }
            }
            else
            {
                values.Add(ConvertNonCollectionObjectToEntityPropertyValue(propertyValue, propertyType));
            }

            return values;
        }

        public static Value ConvertNonCollectionObjectToEntityPropertyValue(object propertyValue)
        {
            return ConvertNonCollectionObjectToEntityPropertyValue(propertyValue, propertyValue.GetType());
        }

        public static Value ConvertNonCollectionObjectToEntityPropertyValue(object propertyValue, Type propertyType)
        {
            var value = new Value();

            // if nullable, get underlying type
            if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            if (propertyType == typeof(String))
            {
                value.StringValue = (string)propertyValue;
            }
            else if (propertyType == typeof(bool))
            {
                value.BooleanValue = (bool)propertyValue;
            }
            else if (propertyType == typeof(byte[]))
            {
                value.BlobValue = BitConverter.ToString((byte[])propertyValue);
            }
            else if (IsIntegralType(propertyType))
            {
                value.IntegerValue = propertyValue.ToString();
            }
            else if (IsFloatingPointType(propertyType))
            {
                value.DoubleValue = (double?)propertyValue;
            }
            else if (propertyType == typeof(DateTime))
            {
                value.DateTimeValue = XmlConvert.ToString((DateTime)propertyValue, XmlDateTimeSerializationMode.Unspecified) + "Z";
            }
            // should anything be saved as BlobValue? image? other large values? byte array?
            else
            {
                value.EntityValue = ConvertObjectToEntity(propertyValue); // recursive ... TODO is Key a problem here?
                value.Indexed = false; // entity values cannot be indexed
            }

            return value;
        }

        public static PropertyInfo GetIdProperty(object poco)
        {
            return GetIdProperty(poco.GetType());
        }

        public static PropertyInfo GetIdProperty(Type pocoType)
        {
            return pocoType.GetProperty("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public static string GetIdValue(object poco)
        {
            var id = GetIdProperty(poco);

            if (id != null)
            {
                var idValue = id.GetValue(poco, null);

                if (idValue != null)
                {
                    return idValue.ToString();
                }
            }

            return null;
        }

        private static bool IsIntegralType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsFloatingPointType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsMulti(Type type)
        {
            return type != typeof(string) && type.GetInterface(typeof(IEnumerable<>).FullName) != null;

            // TODO add support for non-generic collections?
        }

        private static Type GetElementType(Type collectionType)
        {
            var arrayType = collectionType.GetElementType();

            if (arrayType != null)
            {
                return arrayType;
            }

            var genericArguments = collectionType.GetGenericArguments();

            if (genericArguments != null && genericArguments.Length > 0)
            {
                return genericArguments[0];
            }

            return null; // or throw exception that element type not supported?
        }
    }
}
