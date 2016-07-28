namespace PokemonGoScanner.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using AppModels;
    using Models;
    using Newtonsoft.Json;
    public class GoogleLogin
    {
        public string accessToken{get;set;}

        public async Task LoginAsync(Scanner scanner)
        {
            if (Constant.UseEmailPasswordToLogin)
            {
                var gpsoAuthClient = new GPSOAuthClient(scanner.Email, scanner.Password);
                var response = gpsoAuthClient.PerformMasterLogin();
                var json = JsonConvert.SerializeObject(response, Formatting.Indented);
                if (response.ContainsKey("Token"))
                {
                    string token = response["Token"];
                    var oauthResponse = gpsoAuthClient.PerformOAuth(token, Constant.AndroidService, Constant.AndroidAppName, Constant.AndroidClientSignature);
                    this.accessToken = oauthResponse["Auth"];
                }
                else
                {
                    Trace.TraceInformation("MasterLogin failed (check credentials)");
                    throw new Exception();
                }
            }
            else
            {
                var refreshToken = string.Empty;
                if (File.Exists(Constant.GoogleRefreshTokenPath))
                {
                    refreshToken = File.ReadAllText(Constant.GoogleRefreshTokenPath);
                }

                GoogleTokenResponse response;
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    response = await this.GetGoogleAccessTokenAsync(refreshToken);
                    this.accessToken = response?.id_token;
                }

                if (string.IsNullOrWhiteSpace(this.accessToken))
                {
                    var deviceCode = await this.GetGoogleDeviceCodeAsync();
                    response = await this.GetGoogleAccessTokenAsync(deviceCode);
                    refreshToken = response?.refresh_token;
                    File.WriteAllText(Constant.GoogleRefreshTokenPath, refreshToken);
                    this.accessToken = response?.id_token;
                }
            }
        }

        private async Task<GoogleTokenResponse> GetGoogleAccessTokenAsync(GoogleDeviceCode deviceCode)
        {
            GoogleTokenResponse response;
            do
            {
                await Task.Delay(Constant.GoogleGetDeviceCodeDelayInMs);
                response = await this.PostEncodedAsync<GoogleTokenResponse>(
                Constant.GoogleOauthTokenEndpoint,
                new KeyValuePair<string, string>("client_secret", Constant.GoogleClientSecret),
                new KeyValuePair<string, string>("code", deviceCode.device_code),
                new KeyValuePair<string, string>("grant_type", Constant.GoogleDeviceAuthUri));
            } while (response.access_token == null || response.refresh_token == null);

            return response;
        }

        private async Task<GoogleTokenResponse> GetGoogleAccessTokenAsync(string refreshToken)
        {
            return await this.PostEncodedAsync<GoogleTokenResponse>(
                Constant.GoogleOauthTokenEndpoint,
                new KeyValuePair<string, string>("access_type", "offline"),
                new KeyValuePair<string, string>("client_secret", Constant.GoogleClientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"));
        }

        private async Task<GoogleDeviceCode> GetGoogleDeviceCodeAsync()
        {
            var deviceCode = await this.PostEncodedAsync<GoogleDeviceCode>(Constant.GoogleOauthEndpoint);

            try
            {
                Trace.TraceInformation("Do not use your main Google account!!!");
                Trace.TraceInformation("Device code copied to clipboard");
                Thread.Sleep(Constant.GoogleGetDeviceCodeDelayInMs);
                Process.Start(Constant.GoogleDeviceUri);
                var thread = new Thread(() => Clipboard.SetText(deviceCode.user_code)); //Copy device code
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join();
            }
            catch (Exception)
            {
                Trace.TraceInformation($"Goto: {Constant.GoogleDeviceUri} & enter {deviceCode.user_code}");
            }

            return deviceCode;
        }

        private async Task<T> PostEncodedAsync<T>(string url, params KeyValuePair<string, string>[] keyValuePairs)
        {
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
            if (keyValuePairs != null)
            {
                content = keyValuePairs.ToList();
            }
            content.Add(new KeyValuePair<string, string>("client_id", Constant.GoogleClientId));
            content.Add(new KeyValuePair<string, string>("scope", Constant.GoogleEmailUri));

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var httpClient = new HttpClient(handler))
            {
                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(content));
                return await response.Content.ReadAsAsync<T>();
            }
        }
    }
}
