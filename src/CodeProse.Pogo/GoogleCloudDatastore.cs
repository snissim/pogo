using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Datastore.v1beta1;
using Google.Apis.Services;
using System.Configuration;

namespace CodeProse.Pogo
{
    public class GoogleCloudDatastore : IDatastore
    {
        DatastoreService _service;
        Dictionary<string, string> _connStringParts;

        public GoogleCloudDatastore(string connectionStringName)
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            _connStringParts = connString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Split(new char[] { '=' }, 2))
                .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            _service = new DatastoreService(new BaseClientService.Initializer() { Authenticator = CreateAuthenticator() });
        }

        public IDatastoreSession OpenSession()
        {
            return new DatastoreSession(_service, _connStringParts["DatasetId"]);
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
