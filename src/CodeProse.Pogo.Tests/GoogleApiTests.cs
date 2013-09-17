using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Authentication.OAuth2;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Datastore.v1beta1;
using Google.Apis.Services;
using GoogleData = Google.Apis.Datastore.v1beta1.Data;
using Google.Apis.Datastore.v1beta1.Data;
using System.Xml;
using System.Configuration;

namespace CodeProse.Pogo.Tests
{
    [TestClass]
    public class GoogleApiTests
    {
        string _datasetId;
        DatastoreService _service;
        Dictionary<string, string> _connStringParts;

        [TestInitialize]
        public void Initialize()
        {
            var connString = ConfigurationManager.ConnectionStrings["Pogo"].ConnectionString;
            _connStringParts = connString.Split(';')
                .Select(t => t.Split(new char[] { '=' }, 2))
                .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            _datasetId = _connStringParts["DatasetId"];

            _service = new DatastoreService(new BaseClientService.Initializer() { Authenticator = CreateAuthenticator() });
        }

        [TestMethod]
        public void BasicBlindWrite()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var firstName = new GoogleData.Property();
            firstName.Values = new List<GoogleData.Value>();
            firstName.Values.Add(new GoogleData.Value { StringValue = "Ryan" });
            entity.Properties.Add("FirstName", firstName);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void MultipleProperties()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var firstName = new GoogleData.Property();
            firstName.Values = new List<GoogleData.Value>();
            firstName.Values.Add(new GoogleData.Value { StringValue = "Ryan" });
            entity.Properties.Add("FirstName", firstName);

            var lastName = new GoogleData.Property();
            lastName.Values = new List<GoogleData.Value>();
            lastName.Values.Add(new GoogleData.Value { StringValue = "Gray" });
            entity.Properties.Add("LastName", lastName);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void SaveDateTime()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var birthDateValue = new DateTime(1980, 12, 11);
            var birthDate = new GoogleData.Property();
            birthDate.Values = new List<GoogleData.Value>();
            birthDate.Values.Add(new GoogleData.Value { DateTimeValue = XmlConvert.ToString(birthDateValue, XmlDateTimeSerializationMode.Unspecified) + "Z" });
            entity.Properties.Add("BirthDate", birthDate);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void SaveInteger()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var birthDateValue = new DateTime(1980, 12, 11);
            var birthDate = new GoogleData.Property();
            birthDate.Values = new List<GoogleData.Value>();
            birthDate.Values.Add(new GoogleData.Value { StringValue = XmlConvert.ToString(birthDateValue, XmlDateTimeSerializationMode.Unspecified) });
            entity.Properties.Add("BirthDate", birthDate);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void SaveDouble()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var doubleTest = new GoogleData.Property();
            doubleTest.Values = new List<GoogleData.Value>();
            doubleTest.Values.Add(new GoogleData.Value { DoubleValue = 27.5 });
            entity.Properties.Add("DoubleTest", doubleTest);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void ChildEntity()
        {
            var request = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var firstName = new GoogleData.Property();
            firstName.Values = new List<GoogleData.Value>();
            firstName.Values.Add(new GoogleData.Value { StringValue = "Ryan" });
            entity.Properties.Add("FirstName", firstName);

            var lastName = new GoogleData.Property();
            lastName.Values = new List<GoogleData.Value>();
            lastName.Values.Add(new GoogleData.Value { StringValue = "Gray" });
            entity.Properties.Add("LastName", lastName);

            // child entity
            var childEntity = new GoogleData.Entity();
            //childEntity.Key = new GoogleData.Key();
            //childEntity.Key.Path = new List<KeyPathElement>();
            //childEntity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "Consumer", Name = "Consumer-1" });
            childEntity.Properties = new GoogleData.Entity.PropertiesData();

            var companyName = new GoogleData.Property();
            companyName.Values = new List<GoogleData.Value>();
            companyName.Values.Add(new GoogleData.Value { StringValue = "CodeProse" });
            childEntity.Properties.Add("CompanyName", companyName);

            var companyTitle = new GoogleData.Property();
            companyTitle.Values = new List<GoogleData.Value>();
            companyTitle.Values.Add(new GoogleData.Value { StringValue = "Partner" });
            childEntity.Properties.Add("Title", companyTitle);

            // property on parent for child entity
            var company = new GoogleData.Property();
            company.Values = new List<GoogleData.Value>();
            company.Values.Add(new GoogleData.Value { EntityValue = childEntity, Indexed = false });
            entity.Properties.Add("Company", company);

            request.Mutation = new GoogleData.Mutation();
            request.Mutation.Upsert = new List<GoogleData.Entity>();
            request.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(request, _datasetId).Fetch();
        }

        [TestMethod]
        public void QueryMultiValueProperty()
        {
            var writeRequest = new GoogleData.BlindWriteRequest();

            var entity = new GoogleData.Entity();
            entity.Key = new GoogleData.Key();
            entity.Key.Path = new List<KeyPathElement>();
            entity.Key.Path.Add(new GoogleData.KeyPathElement { Kind = "CodeProse.Pogo.Tests.TestPerson", Name = "TestMultiValueQuery" });
            entity.Properties = new GoogleData.Entity.PropertiesData();

            var skills = new GoogleData.Property { Multi = true };
            skills.Values = new List<GoogleData.Value>();
            skills.Values.Add(new GoogleData.Value { StringValue = ".NET" });
            skills.Values.Add(new GoogleData.Value { StringValue = "Pogo" });
            skills.Values.Add(new GoogleData.Value { StringValue = "Google Cloud Datastore" });
            entity.Properties.Add("Skills", skills);

            writeRequest.Mutation = new GoogleData.Mutation();
            writeRequest.Mutation.Upsert = new List<GoogleData.Entity>();
            writeRequest.Mutation.Upsert.Add(entity);

            var response = _service.Datasets.BlindWrite(writeRequest, _datasetId).Fetch();


            var query = new Query();
            query.Kinds = new List<KindExpression> { new KindExpression { Name = "CodeProse.Pogo.Tests.TestPerson" } };

            query.Filter = new Filter
            {
                PropertyFilter = new PropertyFilter
                    {
                        Operator = "equal",
                        Value = new Value { StringValue = "Pogo" },
                        Property = new PropertyReference { Name = "Skills" }
                    }
            };

            var queryRequest = new RunQueryRequest { Query = query };
            var queryResponse = _service.Datasets.RunQuery(queryRequest, _datasetId).Fetch();

            Assert.IsTrue(queryResponse.Batch.EntityResults[0].Entity.Properties["Skills"].Values.Count > 1);
        }

        private OAuth2Authenticator<AssertionFlowClient> CreateAuthenticator()
        {
            var certificate = new X509Certificate2(_connStringParts["CertificateFilePath"], _connStringParts["CertificatePassword"],
                X509KeyStorageFlags.Exportable);

            var provider = new AssertionFlowClient(GoogleAuthenticationServer.Description, certificate)
            {
                ServiceAccountId = _connStringParts["ServiceAccountId"],
                Scope = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/datastore"
            };

            var authenticator = new OAuth2Authenticator<AssertionFlowClient>(provider, AssertionFlowClient.GetState);

            return authenticator;
        }
    }
}
