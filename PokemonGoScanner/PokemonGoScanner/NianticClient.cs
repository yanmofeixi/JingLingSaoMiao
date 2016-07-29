namespace PokemonGoScanner
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using AppModels;
    using Common;
    using POGOProtos.Enums;
    using POGOProtos.Map.Fort;
    using POGOProtos.Map.Pokemon;
    public class NianticClient
    {
        private NianticRequestSender requestSender;
        private List<ulong> lastScannedPokemons = new List<ulong>(); 
        private List<MapPokemon> pokemonsLessThanOneStep;
        private List<WildPokemon> pokemonsLessThanTwoStep;
        private List<NearbyPokemon> pokemonsMoreThanTwoStep;
        private List<FortData> nearByPokeStops;
        private Location scanLocation;
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        public NianticClient(Location location)
        {
            this.scanLocation = location;
        }

        public async Task InitializeAsync(string google_token)
        {
            this.requestSender = new NianticRequestSender(google_token);
            await this.requestSender.Initialize(this.scanLocation);
        }

        public async Task ScanAsync(EmailAlerter alerter, CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await this.GetPokemonsAsync(this.scanLocation);
                this.Print();
                this.SendEmailForSubscribedUsers(alerter);
                this.lastScannedPokemons = this.pokemonsMoreThanTwoStep.Select(p => p.EncounterId).ToList();
                this.lastScannedPokemons.AddRange(this.nearByPokeStops.Select(p => p.LureInfo.EncounterId).ToList());
                Trace.TraceInformation($"Found {this.pokemonsMoreThanTwoStep.Count} pokemons. Rescan in {Constant.ScanDelayInSeconds} seconds");
                await Task.Delay(Constant.ScanDelayInSeconds * 1000);
                Trace.TraceInformation("");
            }
        }

        private void SendEmailForSubscribedUsers(EmailAlerter alerter)
        {
            if (Constant.EnableEmailAlert)
            {
                var subscriptions = db.LocationSubscriptions.Where(l => l.LocationId == this.scanLocation.Id);
                var subscribedUsers = subscriptions.Where(sb => sb.User.IsActive).Select(s => s.User);
                foreach (var user in subscribedUsers)
                {
                    var list = Utility.ConvertToPokemonIdList(user.IgnoreList);
                    if (this.HasNewRarePokemonSpawned(list) || this.HasNewRarePokemonBeenLured(list))
                    {
                        var html = PrintToHtml(list);
                        alerter.Send($"{this.scanLocation.Name}", html, user.EmailForAlert);
                    }
                }
            }
        }


        private async Task GetPokemonsAsync(Location location)
        {
            var response = await this.requestSender.SendMapRequest(location);
            this.nearByPokeStops = response.MapCells.SelectMany(x => x.Forts).Where(y => y.Type == FortType.Checkpoint && y.LureInfo != null).ToList();
            this.pokemonsLessThanOneStep = response.MapCells.SelectMany(x => x.CatchablePokemons).ToList();
            this.pokemonsLessThanTwoStep = response.MapCells.SelectMany(x => x.WildPokemons).ToList();
            this.pokemonsMoreThanTwoStep = response.MapCells.SelectMany(x => x.NearbyPokemons).ToList();
        }

        private void Print()
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

        public string PrintToHtml(List<PokemonId> ignoreList)
        {
            var printedIds = new List<ulong>();
            var sb = new StringBuilder();           
            if (this.pokemonsLessThanOneStep.Count > 0)
            {
                sb.AppendLine("<h2>Pokemon within 1 step:</h2>");
                foreach (var pokemon in this.pokemonsLessThanOneStep)
                {
                    var color = ignoreList.Contains(pokemon.PokemonId) ? "Black" : "Red";
                    sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonId} at {GoogleMapHelper.GetGMapLink(this.scanLocation)} "
                        + $", spawnId: {pokemon.SpawnPointId}{Utility.GetDespawnString(pokemon)}</font></p>");
                    printedIds.Add(pokemon.EncounterId);
                }
            }

            if (this.nearByPokeStops.Count > 0)
            {
                sb.AppendLine("<h2>Pokemons lured at pokestops:</h2>");
                foreach (var pokeStop in this.nearByPokeStops)
                {
                    var color = ignoreList.Contains(pokeStop.LureInfo.ActivePokemonId) ? "Black" : "Red";
                    sb.Append($"<p><font color=\"{color}\">{pokeStop.LureInfo.ActivePokemonId} at {GoogleMapHelper.GetGMapLink(pokeStop)} "
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
                        var color = ignoreList.Contains(pokemon.PokemonData.PokemonId) ? "Black" : "Red";
                        sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonData.PokemonId} at {GoogleMapHelper.GetGMapLink(this.scanLocation)} "
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
                        var color = ignoreList.Contains(pokemon.PokemonId)? "Black": "Red";
                        sb.Append($"<p><font color=\"{color}\">{pokemon.PokemonId}</p>");
                        printedIds.Add(pokemon.EncounterId);
                    }
                }
            }
            return sb.ToString();
        }

        private bool HasNewRarePokemonSpawned(List<PokemonId> ignoreList)
        {
            return this.pokemonsLessThanTwoStep.Any(p => !ignoreList.Contains(p.PokemonData.PokemonId) &&
                    !this.lastScannedPokemons.Contains(p.EncounterId));
        }

        private bool HasNewRarePokemonBeenLured(List<PokemonId> ignoreList)
        {
            return this.nearByPokeStops != null &&
                   this.nearByPokeStops.Any(p => !ignoreList.Contains(p.LureInfo.ActivePokemonId) &&
                   !this.lastScannedPokemons.Contains(p.LureInfo.EncounterId));
        }
    }
}
