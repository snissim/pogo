using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProse.Pogo
{
    public class AddIndexRequest : Google.Apis.Requests.ClientServiceRequest<AddIndexResponse>
    {
        private System.Nullable<bool> _prettyPrint;

        private string _fields;

        private string _quotaUser;

        private string _oauth_token;

        private string _userIp;

        private string _alt;

        private string _datasetId;

        private AddIndexRequest _Body;

        public AddIndexRequest(Google.Apis.Services.IClientService service, AddIndexRequest body, string datasetId) :
            base(service)
        {
            this.Body = body;
            this._datasetId = datasetId;
            this.InitParameters();
        }

        /// <summary>Returns response with indentations and line breaks.</summary>
        [Google.Apis.Util.RequestParameterAttribute("prettyPrint", Google.Apis.Util.RequestParameterType.Query)]
        public virtual System.Nullable<bool> PrettyPrint
        {
            get
            {
                return this._prettyPrint;
            }
            set
            {
                this._prettyPrint = value;
            }
        }

        /// <summary>Selector specifying which fields to include in a partial response.</summary>
        [Google.Apis.Util.RequestParameterAttribute("fields", Google.Apis.Util.RequestParameterType.Query)]
        public virtual string Fields
        {
            get
            {
                return this._fields;
            }
            set
            {
                this._fields = value;
            }
        }

        /// <summary>Available to use for quota purposes for server-side applications. Can be any arbitrary string assigned to a user, but should not exceed 40 characters. Overrides userIp if both are provided.</summary>
        [Google.Apis.Util.RequestParameterAttribute("quotaUser", Google.Apis.Util.RequestParameterType.Query)]
        public virtual string QuotaUser
        {
            get
            {
                return this._quotaUser;
            }
            set
            {
                this._quotaUser = value;
            }
        }

        /// <summary>OAuth 2.0 token for the current user.</summary>
        [Google.Apis.Util.RequestParameterAttribute("oauth_token", Google.Apis.Util.RequestParameterType.Query)]
        public virtual string Oauth_token
        {
            get
            {
                return this._oauth_token;
            }
            set
            {
                this._oauth_token = value;
            }
        }

        /// <summary>IP address of the site where the request originates. Use this if you want to enforce per-user limits.</summary>
        [Google.Apis.Util.RequestParameterAttribute("userIp", Google.Apis.Util.RequestParameterType.Query)]
        public virtual string UserIp
        {
            get
            {
                return this._userIp;
            }
            set
            {
                this._userIp = value;
            }
        }

        /// <summary>Data format for the response.</summary>
        [Google.Apis.Util.RequestParameterAttribute("alt", Google.Apis.Util.RequestParameterType.Query)]
        public virtual string Alt
        {
            get
            {
                return this._alt;
            }
            set
            {
                this._alt = value;
            }
        }

        /// <summary>Identifies the dataset.</summary>
        [Google.Apis.Util.RequestParameterAttribute("datasetId", Google.Apis.Util.RequestParameterType.Path)]
        public virtual string DatasetId
        {
            get
            {
                return this._datasetId;
            }
        }

        /// <summary>Gets/Sets the Body of this Request.</summary>
        public virtual AddIndexRequest Body
        {
            get
            {
                return this._Body;
            }
            set
            {
                this._Body = value;
            }
        }

        public override string ResourcePath
        {
            get
            {
                return "datasets";
            }
        }

        public override string MethodName
        {
            get
            {
                return "runQuery";
            }
        }

        public override string HttpMethod
        {
            get
            {
                return "POST";
            }
        }

        public override string RestPath
        {
            get
            {
                return "{datasetId}/runQuery";
            }
        }

        protected override object GetBody()
        {
            return this.Body;
        }

        private void InitParameters()
        {
            System.Collections.Generic.Dictionary<string, Google.Apis.Discovery.IParameter> parameters = new System.Collections.Generic.Dictionary<string, Google.Apis.Discovery.IParameter>();
            parameters.Add("datasetId", Google.Apis.Util.Utilities.CreateRuntimeParameter("datasetId", true, "path", null, null, new string[0]));
            this._requestParameters = new Google.Apis.Util.ReadOnlyDictionary<string, Google.Apis.Discovery.IParameter>(parameters);
        }
    }
}
