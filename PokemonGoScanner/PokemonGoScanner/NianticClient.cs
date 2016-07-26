namespace PokemonGoScanner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Common;
    using Enum;
    using Google.Protobuf;
    using Models;

    public class NianticClient
    {
        private readonly HttpClient httpClient;
        private string apiUrl;
        private Request.Types.UnknownAuth unknownAuth;

        private List<MapPokemon> pokemonsLessThanOneStep;
        private List<WildPokemon> pokemonsLessThanTwoStep;
        private List<NearbyPokemon> pokemonsMoreThanTwoStep;

        public NianticClient()
        {
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

        public async Task InitializeAsync(string google_token)
        {
            var initialRequest = NianticRequestSender.GetInitialRequest(google_token, RequestType.GET_PLAYER, RequestType.GET_HATCHED_OBJECTS, RequestType.GET_INVENTORY, RequestType.CHECK_AWARDED_BADGES, RequestType.DOWNLOAD_SETTINGS);
            var initialResepone = await this.httpClient.PostProtoAsync(Constant.NianticRpcUrl, initialRequest);
            if (initialResepone.Auth == null)
            {
                Console.WriteLine("Token expired");
                throw new Exception();
            }
            this.unknownAuth = new Request.Types.UnknownAuth
            {
                Unknown71 = initialResepone.Auth.Unknown71,
                Timestamp = initialResepone.Auth.Timestamp,
                Unknown73 = initialResepone.Auth.Unknown73
            };

            this.apiUrl = initialResepone.ApiUrl;
        }

        public async Task ScanAsync()
        {
            while (true)
            {
                await this.GetPokemonsAsync();
                this.Print();
                Console.Write($"Found {this.pokemonsMoreThanTwoStep.Count} pokemons. Rescan in {Constant.ScanDelayInSeconds} seconds");
                for(int i = 0; i < Constant.ScanDelayInSeconds; i++)
                {
                    await Task.Delay(1000);
                    Console.Write(".");
                }
                Console.WriteLine();
            }
        }

        private async Task GetPokemonsAsync()
        {
            var cellRequest = new Request.Types.MapObjectsRequest
            {
                CellIds = ByteString.CopyFrom(ProtoBufHelper.EncodeUlongList(GoogleMapHelper.GetNearbyCellIds())),
                Unknown14 = ByteString.CopyFromUtf8("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
            };

            var request = NianticRequestSender.GetRequest(this.unknownAuth, 
                new Request.Types.Requests
                {
                    Type = (int)RequestType.GET_MAP_OBJECTS,
                    Message = cellRequest.ToByteString()
                },
                new Request.Types.Requests { Type = (int)RequestType.GET_HATCHED_OBJECTS },
                new Request.Types.Requests
                {
                    Type = (int)RequestType.GET_INVENTORY,
                    Message = new Request.Types.Time { Time_ = DateTimeHelper.ToUnixTime(DateTime.UtcNow) }.ToByteString()
                },
                new Request.Types.Requests { Type = (int)RequestType.CHECK_AWARDED_BADGES },
                new Request.Types.Requests
                {
                    Type = (int)RequestType.DOWNLOAD_SETTINGS,
                    Message =
                        new Request.Types.SettingsGuid
                        {
                            Guid = ByteString.CopyFromUtf8("4a2e9bc330dae60e7b74fc85b98868ab4700802e")
                        }.ToByteString()
                });

            var response = await httpClient.PostProtoPayloadAsync<Request, GetMapObjectsResponse>($"https://{this.apiUrl}/rpc", request);

            this.pokemonsLessThanOneStep = response.MapCells.SelectMany(x => x.CatchablePokemons).ToList();
            this.pokemonsLessThanTwoStep = response.MapCells.SelectMany(x => x.WildPokemons).ToList();
            this.pokemonsMoreThanTwoStep = response.MapCells.SelectMany(x => x.NearbyPokemons).ToList();
        }

        private void Print()
        {
            List<ulong> printedIds = new List<ulong>();

            Console.WriteLine("Pokemon within 1 step:");
            foreach (var pokemon in pokemonsLessThanOneStep)
            {
                var despawnSeconds = (pokemon.ExpirationTimestampMs - DateTimeHelper.ToUnixTime(DateTime.UtcNow)) / 1000;
                var despawnMinutes = despawnSeconds / 60;
                despawnSeconds = despawnSeconds % 60;
                Console.ForegroundColor = Constant.PokemonsDisplayInWhite.Contains(pokemon.PokemonId) ? ConsoleColor.White : ConsoleColor.Red;
                Console.WriteLine($"{pokemon.PokemonId} at {pokemon.Latitude},{pokemon.Longitude}, despawn in {despawnMinutes} minutes { despawnSeconds} seconds");
                printedIds.Add(pokemon.EncounterId);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Pokemon within 2 steps:");
            foreach (var pokemon in pokemonsLessThanTwoStep)
            {
                if (!printedIds.Contains(pokemon.EncounterId))
                {
                    var despawnSeconds = pokemon.TimeTillHiddenMs;
                    var despawnMinutes = despawnSeconds / 60;
                    despawnSeconds = despawnSeconds % 60;
                    Console.ForegroundColor = Constant.PokemonsDisplayInWhite.Contains(pokemon.PokemonData.PokemonId) ? ConsoleColor.White : ConsoleColor.Green ;
                    Console.WriteLine($"{pokemon.PokemonData.PokemonId} at {pokemon.Latitude},{pokemon.Longitude}, despawn in {despawnMinutes} minutes { despawnSeconds} seconds");
                    printedIds.Add(pokemon.EncounterId);
                }
            }
            Console.WriteLine();
            Console.ResetColor();

            Console.WriteLine("Pokemon > 200 meter away");
            foreach (var pokemon in pokemonsMoreThanTwoStep)
            {
                if (!printedIds.Contains(pokemon.EncounterId))
                {
                    Console.ForegroundColor = Constant.PokemonsDisplayInWhite.Contains(pokemon.PokemonId) ? ConsoleColor.White : ConsoleColor.Magenta;
                    Console.WriteLine($"{pokemon.PokemonId}");
                    printedIds.Add(pokemon.EncounterId);
                }
            }
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
