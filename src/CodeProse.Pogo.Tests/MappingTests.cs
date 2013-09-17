using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProse.Pogo.Mapping;
using System.Xml;
using Google.Apis.Datastore.v1beta1.Data;

namespace CodeProse.Pogo.Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void BasicObjectToEntityMap()
        {
            var poco = new TestPerson { FirstName = "Ryan" };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            Assert.IsTrue(entity.Properties.Count > 0);
        }

        [TestMethod]
        public void ObjectToEntityWithId()
        {
            var poco = new TestPerson { Id = "Consumers/33" };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var key = entity.Key.Path[0].Name;

            Assert.AreEqual("Consumers/33", key);
        }

        [TestMethod]
        public void ObjectToEntityString()
        {
            var poco = new TestPerson { FirstName = "Ryan" };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = entity.Properties["FirstName"].Values[0].StringValue;

            Assert.AreEqual("Ryan", value);
        }

        [TestMethod]
        public void ObjectToEntityBoolean()
        {
            var poco = new TestPerson { IsActive = true };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = entity.Properties["IsActive"].Values[0].BooleanValue;

            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void ObjectToEntityInteger()
        {
            var poco = new TestPerson { DepartmentCode = 123 };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = Convert.ToInt32(entity.Properties["DepartmentCode"].Values[0].IntegerValue);

            Assert.AreEqual(123, value);
        }

        [TestMethod]
        public void ObjectToEntityLong()
        {
            var poco = new TestPerson { Face = 123L };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = Convert.ToInt64(entity.Properties["Face"].Values[0].IntegerValue);

            Assert.AreEqual(123L, value);
        }

        [TestMethod]
        public void ObjectToEntityDouble()
        {
            var poco = new TestPerson { HourlyRate = 12.5 };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = entity.Properties["HourlyRate"].Values[0].DoubleValue;

            Assert.AreEqual(12.5, value);
        }

        [TestMethod]
        public void ObjectToEntityDateTime()
        {
            var now = DateTime.Now;

            var poco = new TestPerson { HireDate = now };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            string dateString = entity.Properties["HireDate"].Values[0].DateTimeValue;
            var value = XmlConvert.ToDateTime(dateString, XmlDateTimeSerializationMode.Unspecified);

            Assert.AreEqual(now, value);
        }

        [TestMethod]
        public void ByteArray()
        {
            byte[] bytes = BitConverter.GetBytes(201805978);

            var poco = new TestPerson { ProfileImage = bytes };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var deserializedPoco = ObjectEntityMapper.ConvertEntityToObject<TestPerson>(entity);

            CollectionAssert.AreEqual(bytes, deserializedPoco.ProfileImage);
        }

        [TestMethod]
        public void ObjectToEntityWithMultiValuedProperty()
        {
            var poco = new TestPerson { Skills = new string[] { ".NET", "Pogo", "Google Cloud Datastore" } };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var property = entity.Properties["Skills"];

            Assert.AreEqual(true, property.Multi);
            Assert.AreEqual("Pogo", property.Values[1].StringValue);
        }

        [TestMethod]
        public void EntityToObjectWithMultiValuedArrayProperty()
        {
            var property = new Property()
            {
                Multi = true,
                Values = new List<Value>
                {
                    new Value { StringValue = ".NET" },
                    new Value { StringValue = "Pogo" },
                    new Value { StringValue = "Google Cloud Datastore" }
                }
            };
            var entity = new Entity();
            entity.Properties = new Entity.PropertiesData();
            entity.Properties.Add("Skills", property);

            var poco = ObjectEntityMapper.ConvertEntityToObject<TestPerson>(entity);

            Assert.AreEqual("Pogo", poco.Skills[1]);
        }

        [TestMethod]
        public void EntityToObjectWithMultiValuedCollectionProperty()
        {
            var property = new Property()
            {
                Multi = true,
                Values = new List<Value>
                {
                    new Value { StringValue = "111" },
                    new Value { StringValue = "555" },
                    new Value { StringValue = "999" }
                }
            };
            var entity = new Entity();
            entity.Properties = new Entity.PropertiesData();
            entity.Properties.Add("PhoneNumbers", property);

            var poco = ObjectEntityMapper.ConvertEntityToObject<TestPerson>(entity);

            Assert.AreEqual("555", poco.PhoneNumbers[1]);
        }

        [TestMethod]
        public void IgnoreProperty()
        {
            var poco = new TestDepartment { PhoneNumber = "555", Code = 123 };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            Assert.IsFalse(entity.Properties.ContainsKey("Code"));
            Assert.IsTrue(entity.Properties.ContainsKey("PhoneNumber"));
        }

        [TestMethod]
        public void CustomSerializePropertyName()
        {
            var poco = new TestDepartment { Name = "test" };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            Assert.IsFalse(entity.Properties.ContainsKey("Name"));
            Assert.IsTrue(entity.Properties.ContainsKey("departmentName"));
        }

        [TestMethod]
        public void CustomDeserializePropertyName()
        {
            var entity = new Entity();
            entity.Properties = new Entity.PropertiesData();
            entity.Properties.Add("departmentName", new Property { Values = new List<Value> { new Value { StringValue = "test" } } });

            var poco = ObjectEntityMapper.ConvertEntityToObject<TestDepartment>(entity);

            Assert.AreEqual("test", poco.Name);
        }

        [TestMethod]
        public void ObjectToEntityWithEntityProperty()
        {
            var poco = new TestDepartment
            {
                Code = 123,
                Director = new TestPerson { FirstName = "Boss" }
            };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = entity.Properties["Director"].Values[0].EntityValue;
            string firstName = value.Properties["FirstName"].Values[0].StringValue;

            Assert.AreEqual("Boss", firstName);
        }

        [TestMethod]
        public void EntityToObjectWithEntityProperty()
        {
            var directorEntity = new Entity();
            directorEntity.Properties = new Entity.PropertiesData();
            directorEntity.Properties.Add("FirstName", new Property { Values = new List<Value> { new Value { StringValue = "Boss" } } });

            var directorProperty = new Property();
            directorProperty.Values = new List<Value>();
            directorProperty.Values.Add(new Value { EntityValue = directorEntity });

            var entity = new Entity();
            entity.Properties = new Entity.PropertiesData();
            entity.Properties.Add("Director", directorProperty);

            var poco = ObjectEntityMapper.ConvertEntityToObject<TestDepartment>(entity);

            Assert.AreEqual("Boss", poco.Director.FirstName);
        }

        [TestMethod]
        public void ObjectToEntityExcludeIdFromProperties()
        {
            var poco = new TestPerson { Id = "testId", FirstName = "Ryan" };

            var entity = ObjectEntityMapper.ConvertObjectToEntity(poco);

            var value = entity.Properties["FirstName"].Values[0].StringValue;

            Assert.AreEqual("Ryan", value);

            try
            {
                var idProperty = entity.Properties["Id"];

                Assert.Fail("Id should not exist in Properties.");
            }
            catch (Exception)
            {
            }
        }
    }
}
