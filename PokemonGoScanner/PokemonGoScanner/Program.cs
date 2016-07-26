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
                    Task.Run(() => {
                        googleLogin.LoginAsync(user).Wait();
                        nianticClient.InitializeAsync(googleLogin.accessToken, user).Wait();
                        Console.WriteLine($"Start scanning at Latitude:{user.Latitude}, Longitude:{user.Longitude}, for user {user.UserName}");
                        nianticClient.ScanAsync(user).Wait();
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
                    UserName = "Qiyang Lu-Office",
                    Email = "pkmscanner2@gmail.com",
                    Password = "pokemongo",
                    Latitude = 47.659265,
                    Longitude = -122.140394,
                    EmailToReceiveAlert = "yanmofeixi@gmail.com",
                    PokemonsToIgnore = Constant.DefaultIgnoreList
                },
            };
        }
    }
}
