namespace ScannerWorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AppModels;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using PokemonGoScanner;
    using PokemonGoScanner.Common;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();
        private List<Location> locations = new List<Location>();

        public override void Run()
        {
            Trace.TraceInformation("ScannerWorkerRole is running");

            try
            {
                Task.Run(() => this.RunAsync(this.cancellationTokenSource.Token));
                while (true)
                {
                    Thread.Sleep(3600000);
                };
            }
            finally
            {
                
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();
            Trace.TraceInformation("ScannerWorkerRole has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("ScannerWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();

            base.OnStop();

            Trace.TraceInformation("ScannerWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var runningTasks = new List<CancellationTokenSource>();
            while (!cancellationToken.IsCancellationRequested)
            {
                var newLocations = db.Locations.ToList();
                if (!this.IsLocationSame(newLocations))
                {
                    foreach(var cancelTokenSource in runningTasks)
                    {
                        cancelTokenSource.Cancel();
                    }
                    runningTasks.Clear();

                    foreach(var location in newLocations)
                    {
                        var cancelTokenSource = new CancellationTokenSource();
                        runningTasks.Add(cancelTokenSource);
                        Task.Run(() => this.RunAsyncAtLocation(location, cancelTokenSource.Token));
                    }
                    this.locations = newLocations;
                }
                await Task.Delay(Constant.ReloadDbDelayInMs);
            }
        }

        private async Task RunAsyncAtLocation(Location location, CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var scanProcessor = new ScannerProcessor();
                await scanProcessor.InitializeAsync(location);
                await scanProcessor.ExecuteContinuousScanAsync(cancelToken);
                await Task.Delay(Constant.RestartDelayInMs);
            }
        }

        private bool IsLocationSame(List<Location> newLocations)
        {
            var deleted = this.locations.Except(newLocations).ToList();
            var added = newLocations.Except(this.locations).ToList();

            return deleted.Count == 0 && added.Count == 0;
        }
    }
}
