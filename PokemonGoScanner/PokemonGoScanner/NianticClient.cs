namespace PokemonGoScanner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Common;
    using Google.Protobuf;
    using Models;
    using POGOProtos.Networking.Requests;
    using POGOProtos.Map.Fort;
    using POGOProtos.Map.Pokemon;
    using POGOProtos.Networking.Responses;
    public class NianticClient
    {
        private NianticRequestSender requestSender;
        private List<ulong> lastScannedPokemons = new List<ulong>(); 
        private List<MapPokemon> pokemonsLessThanOneStep;
        private List<WildPokemon> pokemonsLessThanTwoStep;
        private List<NearbyPokemon> pokemonsMoreThanTwoStep;
        private List<FortData> nearByPokeStops;

        public NianticClient()
        {

        }

        public async Task InitializeAsync(string google_token, UserSetting user)
        {
            this.requestSender = new NianticRequestSender(google_token);
            await this.requestSender.Initialize(user);
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
            var response = await this.requestSender.SendMapRequest(user);
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
                    var color = user.PokemonsToIgnore.Contains(pokemon.PokemonId) ? "Black" : "Red";
                    sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonId} at {GoogleMapHelper.GetGMapLink(pokemon.Latitude, pokemon.Longitude)} "
                        + $", spawnId: {pokemon.SpawnPointId}{Utility.GetDespawnString(pokemon)}</font></p>");
                    printedIds.Add(pokemon.EncounterId);
                }
            }

            if (this.nearByPokeStops.Count > 0)
            {
                sb.AppendLine("<h2>Pokemons lured at pokestops:</h2>");
                foreach (var pokeStop in this.nearByPokeStops)
                {
                    var color = user.PokemonsToIgnore.Contains(pokeStop.LureInfo.ActivePokemonId) ? "Black" : "Red";
                    sb.Append($"<p><font color=\"{color}\">{pokeStop.LureInfo.ActivePokemonId} at {GoogleMapHelper.GetGMapLink(pokeStop.Latitude, pokeStop.Longitude, "this PokeStop")} "
                        + $", PokeStopId: {pokeStop.Id}{Utility.GetDespawnString(pokeStop.LureInfo)}</font></p>");
                }
            }

            if (this.pokemonsLessThanTwoStep.Count != printedIds.Count)
            {
                sb.AppendLine("<h2>Pokemon within 2 steps:</h2>");

                foreach (var pokemon in this.pokemonsLessThanTwoStep)
                {
                    if (!printedIds.Contains(pokemon.EncounterId))
                    {
                        var color = user.PokemonsToIgnore.Contains(pokemon.PokemonData.PokemonId) ? "Black" : "Red";
                        sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonData.PokemonId} at {GoogleMapHelper.GetGMapLink(pokemon.Latitude, pokemon.Longitude)} "
                            + $", spawnId: {pokemon.SpawnPointId}{Utility.GetDespawnString(pokemon)}</font></p>");
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
