namespace PokemonGoScanner.Common
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    public class GPSOAuthClient
    {
        static RSAParameters androidKey = Utility.KeyFromB64(Constant.b64Key);

        private string email;

        private string password;

        public GPSOAuthClient(string email, string password)
        {
            this.email = email;
            this.password = password;
        }

        // _perform_auth_request
        private Dictionary<string, string> PerformAuthRequest(Dictionary<string, string> data)
        {
            NameValueCollection nvc = new NameValueCollection();
            foreach (var kvp in data)
            {
                nvc.Add(kvp.Key.ToString(), kvp.Value.ToString());
            }
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, Constant.userAgent);
                string result;
                try
                {
                    byte[] response = client.UploadValues(Constant.authUrl, nvc);
                    result = Encoding.UTF8.GetString(response);
                }
                catch (WebException e)
                {
                    result = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                }
                return Utility.ParseAuthResponse(result);
            }
        }

        // perform_master_login
        public Dictionary<string, string> PerformMasterLogin(string service = "ac2dm",
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            string signature = Utility.CreateSignature(email, password, androidKey);
            var dict = new Dictionary<string, string> {
                { "accountType", "HOSTED_OR_GOOGLE" },
                { "Email", email },
                { "has_permission", 1.ToString() },
                { "add_account", 1.ToString() },
                { "EncryptedPasswd",  signature},
                { "service", service },
                { "source", "android" },
                { "device_country", deviceCountry },
                { "operatorCountry", operatorCountry },
                { "lang", lang },
                { "sdk_version", sdkVersion.ToString() }
            };
            return PerformAuthRequest(dict);
        }

        // perform_oauth
        public Dictionary<string, string> PerformOAuth(string masterToken, string service, string app, string clientSig,
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            var dict = new Dictionary<string, string> {
                { "accountType", "HOSTED_OR_GOOGLE" },
                { "Email", email },
                { "has_permission", 1.ToString() },
                { "EncryptedPasswd",  masterToken},
                { "service", service },
                { "source", "android" },
                { "app", app },
                { "client_sig", clientSig },
                { "device_country", deviceCountry },
                { "operatorCountry", operatorCountry },
                { "lang", lang },
                { "sdk_version", sdkVersion.ToString() }
            };
            return PerformAuthRequest(dict);
        }
    }
}
