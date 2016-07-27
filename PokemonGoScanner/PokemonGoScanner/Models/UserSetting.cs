﻿namespace PokemonGoScanner.Models
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
#if DEBUG
            return new List<UserSetting>
            {
                new UserSetting
                {
                    UserName = "Debug",
                    Email = "",
                    Password = "",
                    Latitude = 47.659265,
                    Longitude = -122.140394,
                    PokemonsToIgnore = DefaultIgnoreList
                }
            };
#else
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
                    PokemonsToIgnore = new List<PokemonId>
                                            {
                                                PokemonId.Caterpie,
                                                PokemonId.Clefairy,
                                                PokemonId.Clefable,
                                                PokemonId.Drowzee,
                                                PokemonId.Fearow,
                                                PokemonId.Golbat,
                                                PokemonId.Goldeen,
                                                PokemonId.Horsea,
                                                PokemonId.Kakuna,
                                                PokemonId.Kingler,
                                                PokemonId.Krabby,
                                                PokemonId.Meowth,
                                                PokemonId.Metapod,
                                                PokemonId.NidoranFemale,
                                                PokemonId.NidoranMale,
                                                PokemonId.Nidorina,
                                                PokemonId.Nidorino,
                                                PokemonId.Onix,
                                                PokemonId.Paras,
                                                PokemonId.Parasect,
                                                PokemonId.Pidgeot,
                                                PokemonId.Pidgeotto,
                                                PokemonId.Pidgey,
                                                PokemonId.Poliwag,
                                                PokemonId.Psyduck,
                                                PokemonId.Raticate,
                                                PokemonId.Rattata,
                                                PokemonId.Scyther,
                                                PokemonId.Seaking,
                                                PokemonId.Shellder,
                                                PokemonId.Spearow,
                                                PokemonId.Staryu,
                                                PokemonId.Venonat,
                                                PokemonId.Weedle,
                                                PokemonId.Zubat,
                                            }
                },

                new UserSetting
                {
                    UserName = "Redwest-Office",
                    Email = "pkmscanner2@gmail.com",
                    Password = "pokemongo",
                    Latitude = 47.659265,
                    Longitude = -122.140394,
                    EmailToReceiveAlert = "yanmofeixi@gmail.com,317772270@qq.com",
                    PokemonsToIgnore = DefaultIgnoreList
                },

                new UserSetting
                {
                    UserName = "Junwei Hu-Home",
                    Email = "duxiaoccnn@gmail.com",
                    Password = "%TGB6yhn^YHN5tgb",
                    Latitude = 47.678306,
                    Longitude = -122.130660,
                    EmailToReceiveAlert = "317772270@qq.com",
                    PokemonsToIgnore = DefaultIgnoreList
                },
            };
#endif
        }

        public static List<PokemonId> DefaultIgnoreList = new List<PokemonId>
        {
            PokemonId.Caterpie,
            PokemonId.Clefairy,
            PokemonId.Clefable,
            PokemonId.Drowzee,
            PokemonId.Fearow,
            PokemonId.Golbat,
            PokemonId.Goldeen,
            PokemonId.Horsea,
            PokemonId.Kakuna,
            PokemonId.Kingler,
            PokemonId.Krabby,
            PokemonId.Meowth,
            PokemonId.Metapod,
            PokemonId.NidoranFemale,
            PokemonId.NidoranMale,
            PokemonId.Nidorina,
            PokemonId.Nidorino,
            PokemonId.Onix,
            PokemonId.Paras,
            PokemonId.Parasect,
            PokemonId.Pidgeot,
            PokemonId.Pidgeotto,
            PokemonId.Pidgey,
            PokemonId.Poliwag,
            PokemonId.Psyduck,
            PokemonId.Raticate,
            PokemonId.Rattata,
            PokemonId.Scyther,
            PokemonId.Seaking,
            PokemonId.Spearow,
            PokemonId.Staryu,
            PokemonId.Venonat,
            PokemonId.Weedle,
            PokemonId.Zubat,
        };
    }
}
