using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Services;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using RestSharp;

namespace CodeProse.Pogo
{
    internal class AppEngineRpcClient
    {
        Dictionary<string, string> _connStringParts;
        RestClient _client;

        public AppEngineRpcClient(string connectionStringName)
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            _connStringParts = connString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Split(new char[] { '=' }, 2))
                .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            _client = new RestClient("http://appengine.google.com/api/datastore");
        }

        public void AddIndex()
        {
            var auth = CreateAuthenticator();
            auth.LoadAccessToken();
            string token = auth.State.AccessToken;

            var request = new RestRequest("index/add?app_id=" + _connStringParts["DatasetId"] + "&version=1", Method.POST);
            
            _client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token);
            var response = _client.Execute(request);
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
