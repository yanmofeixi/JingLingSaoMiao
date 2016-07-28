namespace PokemonGoScanner.Common
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using POGOProtos.Networking.Requests;
    using POGOProtos.Networking.Requests.Messages;
    using POGOProtos.Networking.Responses;
    using POGOProtos.Networking.Envelopes;
    using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;
    using Google.Protobuf;
    using AppModels;
    public class NianticRequestSender
    {
        private string authToken;

        private AuthTicket authTicket;

        private string apiUri;

        private HttpClient httpClient;

        public NianticRequestSender(string authToken, AuthTicket authTicket = null)
        {
            this.authToken = authToken;
            this.authTicket = authTicket;
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            httpClient = new HttpClient(new RetryHelper(handler));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        }

        public async Task Initialize(Location location)
        {
            var getPlayerMessage = new GetPlayerMessage();
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            var serverRequest = this.GetRequestEnvelope(
                location,
                new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = getPlayerMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });
            var response = await this.PostProtoBuf<Request>(Constant.NianticRpcUrl, serverRequest);
            if (response.AuthTicket == null)
                throw new Exception("Token has expired");

            this.authTicket = response.AuthTicket;
            this.apiUri = response.ApiUrl;
        }

        public async Task<GetMapObjectsResponse> SendMapRequest(Location location)
        {
            var getMapObjectsMessage = new GetMapObjectsMessage
            {
                CellId = { GoogleMapHelper.GetNearbyCellIds(location) },
                SinceTimestampMs = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            var request = this.GetRequestEnvelope(
                location,
                new Request
                {
                    RequestType = RequestType.GetMapObjects,
                    RequestMessage = getMapObjectsMessage.ToByteString()
                },
                new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });

            var response = await PostProtoBuf<Request>($"https://{this.apiUri}/rpc", request);

            if (response.Returns.Count == 0)
                throw new Exception("Invalid Response");

            var payload = response.Returns[0];
            var parsedPayload = new GetMapObjectsResponse();
            parsedPayload.MergeFrom(payload);
            return parsedPayload;
        }

        public async Task<ResponseEnvelope> PostProtoBuf<TRequest>(string url, RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
        {
            var data = requestEnvelope.ToByteString();
            var result = await this.httpClient.PostAsync(url, new ByteArrayContent(data.ToByteArray()));
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new ResponseEnvelope();
            decodedResponse.MergeFrom(codedStream);
            return decodedResponse;
        }


        private RequestEnvelope GetRequestEnvelope(Location location, params Request[] customRequests)
        {
            return new RequestEnvelope
            {
                StatusCode = 2,
                RequestId = 1469378659230941192,
                Requests = { customRequests },
                //Unknown6 = ,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = Constant.DefaultAltitude,
                AuthInfo = new AuthInfo
                {
                    Provider = "google",
                    Token = new AuthInfo.Types.JWT
                    {
                        Contents = this.authToken,
                        Unknown2 = 14
                    }
                },
                AuthTicket = this.authTicket,
                Unknown12 = 989
            };
        }
    }
}
