namespace PokemonGoScanner.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Google.Protobuf;

    using Models;
    public static class ProtoBufHelper
    {
        public static async Task<Response> PostProtoAsync<TRequest>(this HttpClient client, string url, TRequest request)
            where TRequest : IMessage<TRequest>
        {
            var data = request.ToByteString();
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new Response();
            decodedResponse.MergeFrom(codedStream);
            return decodedResponse;
        }

        public static async Task<TResponsePayload> PostProtoPayloadAsync<TRequest, TResponsePayload>(this HttpClient client,
            string url, TRequest request) where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            var response = await PostProtoAsync(client, url, request);
            if (response.Payload.Count == 0)
            {
                Console.WriteLine("Invalid response");
                throw new Exception();
            }
            var payload = response.Payload[0];
            var parsedPayload = new TResponsePayload();
            parsedPayload.MergeFrom(payload);
            return parsedPayload;
        }

        public static byte[] EncodeUlongList(List<ulong> integers)
        {
            var output = new List<byte>();
            foreach (var integer in integers.OrderBy(c => c))
            {
                output.AddRange(VarintBitConverter.GetVarintBytes(integer));
            }

            return output.ToArray();
        }
    }
}
