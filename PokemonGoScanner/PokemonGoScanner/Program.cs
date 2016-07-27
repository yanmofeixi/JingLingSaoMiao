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
            var users = InitializeUsers();

            try
            {
                foreach(var user in users)
                {
                    var googleLogin = new GoogleLogin();
                    var nianticClient = new NianticClient();
                    var emailAlerter = new EmailAlerter(new SmtpClient(Constant.EmailHost), user);
                    Task.Run(() => {
                        googleLogin.LoginAsync(user).Wait();
                        nianticClient.InitializeAsync(googleLogin.accessToken, user).Wait();
                        Console.WriteLine($"Start scanning at Latitude:{user.Latitude}, Longitude:{user.Longitude}, for user {user.UserName}");
                        nianticClient.ScanAsync(user, emailAlerter).Wait();
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        private static List<UserSetting> InitializeUsers()
        {
            return new List<UserSetting>
            {
                new UserSetting
                {
                    UserName = "Qiyang Lu-Home",
                    Email = "pkmscanner1@gmail.com",
                    Password = "pokemongo",
                    Latitude = 47.651168,
                    Longitude = -122.130718,
                    EmailToReceiveAlert = "yanmofeixi@gmail.com",
                    PokemonsToIgnore = Constant.DefaultIgnoreList
                },

                new UserSetting
                {
                    UserName = "Redwest-Office",
                    Email = "pkmscanner2@gmail.com",
                    Password = "pokemongo",
                    Latitude = 47.659265,
                    Longitude = -122.140394,
                    EmailToReceiveAlert = "yanmofeixi@gmail.com,317772270@qq.com",
                    PokemonsToIgnore = Constant.DefaultIgnoreList
                },

                new UserSetting
                {
                    UserName = "Junwei Hu-Home",
                    Email = "duxiaoccnn@gmail.com",
                    Password = "%TGB6yhn^YHN5tgb",
                    Latitude = 47.678306,
                    Longitude = -122.130660,
                    EmailToReceiveAlert = "317772270@qq.com",
                    PokemonsToIgnore = Constant.DefaultIgnoreList
                },
            };
        }
    }
}
