namespace PokemonGoScanner.Models
{
    using Common;
    using System.Collections.Generic;


    public class UserSetting
    {
        public string UserName;

        public string Email;

        public string Password;

        public double Latitude;

        public double Longitude;

        //Comma seperated
        public string EmailToReceiveAlert;

        public List<PokemonId> PokemonsToIgnore;

        public static List<UserSetting> InitializeUsers()
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
