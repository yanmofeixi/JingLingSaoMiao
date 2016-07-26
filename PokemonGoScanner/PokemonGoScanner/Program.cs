using System.Net.Mail;

namespace PokemonGoScanner
{
    using System;
    using System.Threading.Tasks;

    using Common;
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var googleLogin = new GoogleLogin();
                var nianticClient = new NianticClient();
                var emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost));
                Task.Run(() => {
                    googleLogin.LoginAsync().Wait();
                    nianticClient.InitializeAsync(googleLogin.accessToken).Wait();
                    Console.WriteLine($"Start scanning at Latitude:{Constant.DefaultLatitude}, Longitude:{Constant.DefaultLongitude}");
                    nianticClient.ScanAsync(emailAlerter).Wait();
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
