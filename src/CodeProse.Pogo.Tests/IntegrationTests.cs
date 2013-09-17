using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Apis.Datastore.v1beta1;
using Google.Apis.Services;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using System.Security.Cryptography.X509Certificates;
using CodeProse.Pogo.Mapping;
using Google.Apis.Datastore.v1beta1.Data;

namespace CodeProse.Pogo.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        IDatastore _datastore;

        [TestInitialize]
        public void Initialize()
        {
            _datastore = new GoogleCloudDatastore("Pogo");
        }

        [TestMethod]
        public void SaveObjectWithAllPrimitiveFields()
        {
            var person = new TestPerson
            {
                Id = "TestPersons/99",
                DepartmentCode = 123,
                Face = 1234567890L,
                FirstName = "Rey",
                HireDate = new DateTime(2012, 5, 7, 9, 30, 0),
                HourlyRate = 25,
                IsActive = true
            };

            using (var session = _datastore.OpenSession())
            {
                session.Store(person);
                session.SaveChanges();
            }
        }

        [TestMethod]
        public void StoreAndLookupObjectWithChildEntity()
        {
            var unique = Guid.NewGuid().ToString();

            var poco = new TestDepartment
            {
                Id = unique,
                Code = 123,
                Director = new TestPerson { FirstName = "Boss" }
            };

            using (var session = _datastore.OpenSession())
            {
                session.Store(poco);
                session.SaveChanges();

                var lookupPoco = session.Load<TestDepartment>(unique);

                Assert.AreEqual("Boss", lookupPoco.Director.FirstName);
            }
        }

        [TestMethod]
        public void BasicLookup()
        {
            var person = new TestPerson
            {
                Id = "TestPersons/33",
                DepartmentCode = 123,
                Face = 1234567890L,
                FirstName = "Ryan",
                HireDate = new DateTime(2010, 5, 7, 9, 30, 0),
                HourlyRate = 200,
                IsActive = true
            };

            using (var session = _datastore.OpenSession())
            {
                session.Store(person);
                session.SaveChanges();

                var lookupPerson = session.Load<TestPerson>("TestPersons/33");

                Assert.AreEqual(person.DepartmentCode, lookupPerson.DepartmentCode);
                Assert.AreEqual(person.Face, lookupPerson.Face);
                Assert.AreEqual(person.FirstName, lookupPerson.FirstName);
                Assert.AreEqual(person.HireDate, lookupPerson.HireDate);
                Assert.AreEqual(person.HourlyRate, lookupPerson.HourlyRate);
                Assert.AreEqual(person.Id, lookupPerson.Id);
                Assert.AreEqual(person.IsActive, lookupPerson.IsActive);
            }
        }

        [TestMethod]
        public void BasicQuery()
        {
            using (var session = _datastore.OpenSession())
            {
                var results = session.Query<TestPerson>()
                    .Where(t => t.HourlyRate > 50.0)
                    .ToList();

                Assert.IsTrue(results.Count > 0);
            }
        }

        //[TestMethod]
        //public void AddIndex()
        //{
        //    var appEngine = new AppEngineRpcClient("Pogo");
        //    appEngine.AddIndex();
        //}

        [TestMethod]
        public void CompositeQueryWithMultipleFilters()
        {
            using (var session = _datastore.OpenSession())
            {
                //var person = new TestPerson
                //{
                //    Id = "TestPersons/33",
                //    DepartmentCode = 123,
                //    Face = 1234567890L,
                //    FirstName = "Ryan",
                //    HireDate = new DateTime(2010, 5, 7, 9, 30, 0),
                //    HourlyRate = 75,
                //    IsActive = true
                //};

                //session.Store(person);
                //session.SaveChanges();

                var results = session.Query<TestPerson>()
                    //.Customize(t => t.WaitForNonStaleResults())
                    .Where(t => t.HourlyRate == 75.0 && t.DepartmentCode >= 123)
                    .ToList();

                Assert.IsTrue(results.Count > 0);
            }
        }
    }
}
