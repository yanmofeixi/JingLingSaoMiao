namespace ScannerWorkerRole
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using PokemonGoScanner;
    using PokemonGoScanner.Common;
    using PokemonGoScanner.Models;
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public override void Run()
        {
            Trace.TraceInformation("ScannerWorkerRole is running");

            try
            {
                var users = UserSetting.InitializeUsers();
                foreach (var user in users)
                {
                    Task.Run(() => this.RunAsync(this.cancellationTokenSource.Token, user));
                }
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

        private async Task RunAsync(CancellationToken cancellationToken, UserSetting user)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Scanner.ExecuteScan(user);
                await Task.Delay(Constant.RestartDelayInMs);
            }
        }
    }
}
