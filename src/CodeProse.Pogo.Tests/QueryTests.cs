using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Apis.Datastore.v1beta1.Data;
using System.Collections;
using System.Xml;

namespace CodeProse.Pogo.Tests
{
    [TestClass]
    public class QueryTests
    {
        IDatastore _datastore;

        [TestInitialize]
        public void Initialize()
        {
            _datastore = new GoogleCloudDatastore("Pogo");
        }

        [TestMethod]
        public void Equals()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.HourlyRate == 200.0)
                    .ToQuery();

                Assert.AreEqual("equal", query.Filter.PropertyFilter.Operator);
                Assert.AreEqual("HourlyRate", query.Filter.PropertyFilter.Property.Name);
                Assert.AreEqual(200.0, query.Filter.PropertyFilter.Value.DoubleValue);
            }
        }

        [TestMethod]
        public void ReverseEquals()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => 200.0 == t.HourlyRate)
                    .ToQuery();

                Assert.AreEqual("equal", query.Filter.PropertyFilter.Operator);
                Assert.AreEqual("HourlyRate", query.Filter.PropertyFilter.Property.Name);
                Assert.AreEqual(200.0, query.Filter.PropertyFilter.Value.DoubleValue);
            }
        }

        [TestMethod]
        public void LessThanReversed()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => 50.0 < t.HourlyRate)
                    .ToQuery();

                Assert.AreEqual("greaterThan", query.Filter.PropertyFilter.Operator);
                Assert.AreEqual("HourlyRate", query.Filter.PropertyFilter.Property.Name);
                Assert.AreEqual(50.0, query.Filter.PropertyFilter.Value.DoubleValue);
            }
        }

        [TestMethod]
        public void IntegerCompare()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.DepartmentCode == 123)
                    .ToQuery();

                Assert.AreEqual("equal", query.Filter.PropertyFilter.Operator);
                Assert.AreEqual("DepartmentCode", query.Filter.PropertyFilter.Property.Name);
                Assert.AreEqual("123", query.Filter.PropertyFilter.Value.IntegerValue);
            }
        }

        [TestMethod]
        public void DateTimeCompare()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.HireDate > new DateTime(2010, 1, 1))
                    .ToQuery();

                var dateString = XmlConvert.ToString(new DateTime(2010, 1, 1), XmlDateTimeSerializationMode.Unspecified) + "Z";

                Assert.AreEqual("greaterThan", query.Filter.PropertyFilter.Operator);
                Assert.AreEqual("HireDate", query.Filter.PropertyFilter.Property.Name);
                Assert.AreEqual(dateString, query.Filter.PropertyFilter.Value.DateTimeValue);
            }
        }

        [TestMethod]
        public void StrongReadConsistency()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Customize(t => t.WaitForNonStaleResults());

                Assert.AreEqual("strong", query.GetRequest().ReadOptions.ReadConsistency);
            }
        }

        [TestMethod]
        public void Composite()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.HourlyRate > 50.0 && t.DepartmentCode == 123)
                    .ToQuery();

                Assert.AreEqual("and", query.Filter.CompositeFilter.Operator);

                var filter1 = query.Filter.CompositeFilter.Filters[0];
                Assert.AreEqual("greaterThan", filter1.PropertyFilter.Operator);
                Assert.AreEqual("HourlyRate", filter1.PropertyFilter.Property.Name);
                Assert.AreEqual(50.0, filter1.PropertyFilter.Value.DoubleValue);

                var filter2 = query.Filter.CompositeFilter.Filters[1];
                Assert.AreEqual("equal", filter2.PropertyFilter.Operator);
                Assert.AreEqual("DepartmentCode", filter2.PropertyFilter.Property.Name);
                Assert.AreEqual("123", filter2.PropertyFilter.Value.IntegerValue);
            }
        }

        [TestMethod]
        public void CompositeWithThree()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.HourlyRate > 50.0 && t.DepartmentCode == 123 && t.HourlyRate < 100.0)
                    .ToQuery();

                Assert.AreEqual("and", query.Filter.CompositeFilter.Operator);
                
                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "lessThan" &&
                    t.PropertyFilter.Property.Name == "HourlyRate" &&
                    t.PropertyFilter.Value.DoubleValue == 100.0));

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "greaterThan" &&
                    t.PropertyFilter.Property.Name == "HourlyRate" &&
                    t.PropertyFilter.Value.DoubleValue == 50.0));

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "equal" &&
                    t.PropertyFilter.Property.Name == "DepartmentCode" &&
                    t.PropertyFilter.Value.IntegerValue == "123"));
            }
        }

        [TestMethod]
        public void CompositeWithFour()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.HourlyRate > 50.0 && t.DepartmentCode == 123 && t.HourlyRate < 100.0 && t.FirstName == "Ryan")
                    .ToQuery();

                Assert.AreEqual("and", query.Filter.CompositeFilter.Operator);

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "lessThan" &&
                    t.PropertyFilter.Property.Name == "HourlyRate" &&
                    t.PropertyFilter.Value.DoubleValue == 100.0));

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "greaterThan" &&
                    t.PropertyFilter.Property.Name == "HourlyRate" &&
                    t.PropertyFilter.Value.DoubleValue == 50.0));

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "equal" &&
                    t.PropertyFilter.Property.Name == "DepartmentCode" &&
                    t.PropertyFilter.Value.IntegerValue == "123"));

                Assert.IsTrue(query.Filter.CompositeFilter.Filters.Any(t =>
                    t.PropertyFilter.Operator == "equal" &&
                    t.PropertyFilter.Property.Name == "FirstName" &&
                    t.PropertyFilter.Value.StringValue == "Ryan"));
            }
        }

        [TestMethod]
        public void EqualsFunctionInsteadOfOperand()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.FirstName.Equals("Ryan"))
                    .ToQuery();

                Assert.IsTrue(
                    query.Filter.PropertyFilter.Operator == "equal" &&
                    query.Filter.PropertyFilter.Property.Name == "FirstName" &&
                    query.Filter.PropertyFilter.Value.StringValue == "Ryan");
            }
        }

        [TestMethod]
        public void CollectionContains()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.Skills.Contains("Pogo"))
                    .ToQuery();

                Assert.IsTrue(
                    query.Filter.PropertyFilter.Operator == "equal" &&
                    query.Filter.PropertyFilter.Property.Name == "Skills" &&
                    query.Filter.PropertyFilter.Value.StringValue == "Pogo");
            }
        }

        [TestMethod]
        public void CollectionHasAny()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Where(t => t.Skills.Any(s => s == "Pogo"))
                    .ToQuery();

                Assert.IsTrue(
                    query.Filter.PropertyFilter.Operator == "equal" &&
                    query.Filter.PropertyFilter.Property.Name == "Skills" &&
                    query.Filter.PropertyFilter.Value.StringValue == "Pogo");
            }
        }

        [TestMethod]
        public void OrderBy()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .OrderBy(t => t.HireDate)
                    .ToQuery();

                Assert.AreEqual("HireDate", query.Order[0].Property.Name);
            }
        }

        [TestMethod]
        public void OrderByDescending()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .OrderByDescending(t => t.HireDate)
                    .ToQuery();

                Assert.AreEqual("HireDate", query.Order[0].Property.Name);
                Assert.AreEqual("descending", query.Order[0].Direction);
            }
        }

        [TestMethod]
        public void OrderByTwoKeys()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .OrderBy(t => t.HireDate)
                    .ThenBy(t => t.FirstName)
                    .ToQuery();

                Assert.AreEqual("HireDate", query.Order[0].Property.Name);
                Assert.AreEqual("FirstName", query.Order[1].Property.Name);
            }
        }

        [TestMethod]
        public void GroupBy()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .GroupBy(t => t.HireDate)
                    .ToQuery();

                Assert.AreEqual("HireDate", query.GroupBy[0].Name);
            }
        }

        [TestMethod]
        public void GroupByTwoKeys()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .GroupBy(t => new { t.HireDate, t.FirstName })
                    .ToQuery();

                Assert.AreEqual("HireDate", query.GroupBy[0].Name);
                Assert.AreEqual("FirstName", query.GroupBy[1].Name);
            }
        }

        [TestMethod]
        public void Projection()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Select(t => t.HireDate)
                    .ToQuery();

                Assert.AreEqual("HireDate", query.Projection[0].Property.Name);
            }
        }

        [TestMethod]
        public void ProjectionOfTwoKeys()
        {
            using (var session = _datastore.OpenSession())
            {
                var query = session.Query<TestPerson>()
                    .Select(t => new TestProjection { HireDate = t.HireDate, FirstName = t.FirstName })
                    .ToQuery();

                Assert.AreEqual("HireDate", query.Projection[0].Property.Name);
                Assert.AreEqual("FirstName", query.Projection[1].Property.Name);
            }
        }

        class TestProjection
        {
            public DateTime? HireDate { get; set; }
            public string FirstName { get; set; }
        }
    }
}