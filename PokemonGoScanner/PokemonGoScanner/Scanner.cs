namespace PokemonGoScanner
{
    using System;
    using System.Diagnostics;
    using System.Net.Mail;
    using System.Threading.Tasks;

    using Common;
    using Models;
    public class Scanner
    {
        public static async Task ExecuteScan(UserSetting user)
        {
            try
            {
                var googleLogin = new GoogleLogin();
                var nianticClient = new NianticClient();
                var emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost), user);
                await googleLogin.LoginAsync(user);
                await nianticClient.InitializeAsync(googleLogin.accessToken, user);
                Trace.TraceInformation($"Start scanning at Latitude:{user.Latitude}, Longitude:{user.Longitude}, for user {user.UserName}");
                await nianticClient.ScanAsync(user, emailAlerter);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message + " from " + e.Source);
                Trace.TraceInformation("Got an exception, restarting...");
            }
        }
    }
}
