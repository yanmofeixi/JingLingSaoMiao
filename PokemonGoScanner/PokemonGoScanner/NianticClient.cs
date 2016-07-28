﻿namespace PokemonGoScanner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Common;
    using Enum;
    using Google.Protobuf;
    using Models;
    using POGOProtos.Networking.Requests;
    public class NianticClient
    {
        private readonly HttpClient httpClient;
        private string apiUrl;
        private Request.Types.UnknownAuth unknownAuth;
        private List<ulong> lastScannedPokemons = new List<ulong>(); 
        private List<MapPokemon> pokemonsLessThanOneStep;
        private List<WildPokemon> pokemonsLessThanTwoStep;
        private List<NearbyPokemon> pokemonsMoreThanTwoStep;
        private List<FortData> nearByPokeStops;

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

        public async Task InitializeAsync(string google_token, UserSetting user)
        {
            var initialRequest = NianticRequestSender.GetInitialRequest(user, google_token, RequestType.GET_PLAYER, RequestType.GET_HATCHED_OBJECTS, RequestType.GET_INVENTORY, RequestType.CHECK_AWARDED_BADGES, RequestType.DOWNLOAD_SETTINGS);
            var initialResepone = await this.httpClient.PostProtoAsync(Constant.NianticRpcUrl, initialRequest);
            if (initialResepone.Auth == null)
            {
                Trace.TraceInformation("Token expired");
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

        public async Task ScanAsync(UserSetting user, EmailAlerter alerter)
        {
            while (true)
            {
                await this.GetPokemonsAsync(user);
                this.Print(user);
                if (Constant.EnableEmailAlert && (this.HasNewRarePokemonSpawned(user) || this.HasNewRarePokemonBeenLured(user)))
                {
                    var html = PrintToHtml(user);
                    alerter.Send($"{user.UserName}", html);
                    this.lastScannedPokemons = this.pokemonsMoreThanTwoStep.Select(p => p.EncounterId).ToList();
                    this.lastScannedPokemons.AddRange(this.nearByPokeStops.Select(p => p.LureInfo.EncounterId).ToList());
                }
                Trace.TraceInformation($"Found {this.pokemonsMoreThanTwoStep.Count} pokemons. Rescan in {Constant.ScanDelayInSeconds} seconds");
                await Task.Delay(Constant.ScanDelayInSeconds * 1000);
                Trace.TraceInformation("");
            }
        }

        private async Task GetPokemonsAsync(UserSetting user)
        {
            var cellRequest = new Request.Types.MapObjectsRequest
            {
                CellIds = ByteString.CopyFrom(ProtoBufHelper.EncodeUlongList(GoogleMapHelper.GetNearbyCellIds(user))),
                Unknown14 = ByteString.CopyFromUtf8("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
            };

            var request = NianticRequestSender.GetRequest(user, this.unknownAuth, 
                new Request.Types.Requests
                {
                    Type = (int)RequestType.GET_MAP_OBJECTS,
                    Message = cellRequest.ToByteString()
                },
                new Request.Types.Requests { Type = (int)RequestType.GET_HATCHED_OBJECTS },
                new Request.Types.Requests
                {
                    Type = (int)RequestType.GET_INVENTORY,
                    Message = new Request.Types.Time { Time_ = DateTime.UtcNow.ToUnixTime() }.ToByteString()
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
            this.nearByPokeStops = response.MapCells.SelectMany(x => x.Forts).Where(y => y.Type == FortType.Checkpoint && y.LureInfo != null).ToList();
            this.pokemonsLessThanOneStep = response.MapCells.SelectMany(x => x.CatchablePokemons).ToList();
            this.pokemonsLessThanTwoStep = response.MapCells.SelectMany(x => x.WildPokemons).ToList();
            this.pokemonsMoreThanTwoStep = response.MapCells.SelectMany(x => x.NearbyPokemons).ToList();
        }

        private void Print(UserSetting user)
        {
            List<ulong> printedIds = new List<ulong>();

            Trace.TraceInformation("Pokemon within 1 step:");
            foreach (var pokemon in this.pokemonsLessThanOneStep)
            {
                Trace.TraceInformation($"{pokemon.PokemonId} at {pokemon.Latitude},{pokemon.Longitude}" + Utility.GetDespawnString(pokemon));
                printedIds.Add(pokemon.EncounterId);
            }
            Trace.TraceInformation("");

            Trace.TraceInformation("Pokemon lured by pokestops");
            foreach (var pokestop in this.nearByPokeStops)
            {
                if (!printedIds.Contains(pokestop.LureInfo.EncounterId))
                {
                    Trace.TraceInformation($"{pokestop.LureInfo.ActivePokemonId} lured at {pokestop.Latitude},{pokestop.Longitude}, pokestopId {pokestop.Id}" + Utility.GetDespawnString(pokestop.LureInfo));
                    printedIds.Add(pokestop.LureInfo.EncounterId);
                }
            }
            Trace.TraceInformation("");

            Trace.TraceInformation("Pokemon within 2 steps:");
            foreach (var pokemon in this.pokemonsLessThanTwoStep)
            {
                if (!printedIds.Contains(pokemon.EncounterId))
                {
                    Trace.TraceInformation($"{pokemon.PokemonData.PokemonId} at {pokemon.Latitude},{pokemon.Longitude}" + Utility.GetDespawnString(pokemon));
                    printedIds.Add(pokemon.EncounterId);
                }
            }
            Trace.TraceInformation("");

            Trace.TraceInformation("Pokemon > 200 meter away");
            foreach (var pokemon in this.pokemonsMoreThanTwoStep)
            {
                if (!printedIds.Contains(pokemon.EncounterId))
                {
                    Trace.TraceInformation($"{pokemon.PokemonId}");
                    printedIds.Add(pokemon.EncounterId);
                }
            }
            Trace.TraceInformation("");
        }

        public string PrintToHtml(UserSetting user)
        {
            var printedIds = new List<ulong>();
            var sb = new StringBuilder();           
            if (this.pokemonsLessThanOneStep.Count > 0)
            {
                sb.AppendLine("<h2>Pokemon within 1 step:</h2>");
                foreach (var pokemon in this.pokemonsLessThanOneStep)
                {
                    var despawnSeconds = (pokemon.ExpirationTimestampMs -
                                          DateTime.UtcNow.ToUnixTime())/1000;
                    var despawnMinutes = despawnSeconds/60;
                    despawnSeconds = despawnSeconds%60;
                    var color = user.PokemonsToIgnore.Contains(pokemon.PokemonId) ? "Black" : "Red";
                    var mapLink = Utility.GenerateGoogleMapLink(pokemon.Latitude, pokemon.Longitude);
                    sb.Append(
                        $"<p><font color=\"{color}\">{pokemon.PokemonId} at {mapLink}, spawnId: {pokemon.SpawnpointId}, despawn in {despawnMinutes} minutes {despawnSeconds} seconds</font></p>");
                    printedIds.Add(pokemon.EncounterId);
                }
            }

            if (this.pokemonsLessThanTwoStep.Count != printedIds.Count)
            {
                sb.AppendLine("<h2>Pokemon within 2 steps:</h2>");

                foreach (var pokemon in this.pokemonsLessThanTwoStep)
                {
                    if (!printedIds.Contains(pokemon.EncounterId))
                    {
                        var despawnSeconds = pokemon.TimeTillHiddenMs;
                        var despawnMinutes = despawnSeconds/60;
                        despawnSeconds = despawnSeconds%60;
                        var color = user.PokemonsToIgnore.Contains(pokemon.PokemonData.PokemonId)
                            ? "Black"
                            : "Red";
                        var mapLink = Utility.GenerateGoogleMapLink(pokemon.Latitude, pokemon.Longitude);
                        sb.Append(
                            $"<p><font color=\"{color}\">{pokemon.PokemonData.PokemonId} at {mapLink}, spawnId: {pokemon.SpawnpointId}, despawn in {despawnMinutes} minutes {despawnSeconds} seconds</font></p>");
                        printedIds.Add(pokemon.EncounterId);
                    }
                }
            }
            if (this.pokemonsMoreThanTwoStep.Count != printedIds.Count)
            {
                sb.AppendLine("<h2>Pokemon > 200 meter away:</h2>");
                foreach (var pokemon in this.pokemonsMoreThanTwoStep)
                {
                    if (!printedIds.Contains(pokemon.EncounterId))
                    {
                        var color = user.PokemonsToIgnore.Contains(pokemon.PokemonId)? "Black": "Red";
                        sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonId}</p>");
                        printedIds.Add(pokemon.EncounterId);
                    }
                }
            }
            return sb.ToString();
        }

        private bool HasNewRarePokemonSpawned(UserSetting user)
        {
            return this.pokemonsMoreThanTwoStep.Any(p => !user.PokemonsToIgnore.Contains(p.PokemonId) &&
                    !this.lastScannedPokemons.Contains(p.EncounterId));
        }

        private bool HasNewRarePokemonBeenLured(UserSetting user)
        {
            return this.nearByPokeStops != null &&
                   this.nearByPokeStops.Any(p => !user.PokemonsToIgnore.Contains(p.LureInfo.ActivePokemonId) &&
                   !this.lastScannedPokemons.Contains(p.LureInfo.EncounterId));
        }
    }
}
