namespace PokemonGoScanner
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;

    using AppModels;
    using Common;
    public class ScannerProcessor
    {
        private EmailAlerter emailAlerter;
        private GoogleLogin googleLogin;
        private NianticClient nianticClient;
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        public async Task InitializeAsync(Location location)
        {
            var scanner = db.Locations.SingleOrDefault(l => l.Id == location.Id).Scanner;
            this.googleLogin = new GoogleLogin();
            this.nianticClient = new NianticClient(location);
            this.emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost), scanner);
            await this.googleLogin.LoginAsync(scanner);
            await this.nianticClient.InitializeAsync(googleLogin.accessToken);
        }

        public async Task ExecuteScan(Location location, CancellationToken cancelToken)
        {
            try
            {
                Trace.TraceInformation($"Start scanning at {location.Name}");
                await this.nianticClient.ScanAsync(emailAlerter, cancelToken);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message + " from " + e.Source);
                Trace.TraceInformation("Got an exception, restarting...");
            }
        }
    }
}
