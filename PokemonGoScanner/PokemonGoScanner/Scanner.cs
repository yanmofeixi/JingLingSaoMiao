namespace PokemonGoScanner
{
    using System;
    using System.Diagnostics;
    using System.Net.Mail;
    using System.Threading.Tasks;

    using AppModels;
    using Common;
    public class Scanner
    {
        public static async Task ExecuteScan(Location location)
        {
            try
            {
                var googleLogin = new GoogleLogin();
                var nianticClient = new NianticClient();
                var emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost), location);
                await googleLogin.LoginAsync(location);
                await nianticClient.InitializeAsync(googleLogin.accessToken, location);
                Trace.TraceInformation($"Start scanning at {location.Name}");
                await nianticClient.ScanAsync(location, emailAlerter);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message + " from " + e.Source);
                Trace.TraceInformation("Got an exception, restarting...");
            }
        }
    }
}
