using System.Net.Mail;

namespace PokemonGoScanner
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Common;
    using Models;
    class Program
    {
        static void Main(string[] args)
        {
            var users = UserSetting.InitializeUsers();
            foreach(var user in users)
            {
                Task.Run(() => Execute(user));
            }
            Console.ReadLine();
        }

        private static async Task Execute(UserSetting user)
        {
            while (true)
            {
                try
                {
                    var googleLogin = new GoogleLogin();
                    var nianticClient = new NianticClient();
                    var emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost), user);
                    await googleLogin.LoginAsync(user);
                    await nianticClient.InitializeAsync(googleLogin.accessToken, user);
                    Console.WriteLine($"Start scanning at Latitude:{user.Latitude}, Longitude:{user.Longitude}, for user {user.UserName}");
                    await nianticClient.ScanAsync(user, emailAlerter);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " from " + e.Source);
                    Console.WriteLine("Got an exception, restarting...");
                    await Execute(user);
                }
                await Task.Delay(Constant.RestartDelayInMs);
            }
        }
    }
}
