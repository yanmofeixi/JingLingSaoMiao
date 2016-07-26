namespace PokemonGoScanner.Common
{
    using System.Collections.Generic;
    using System.IO;
    using Models;
    public class Constant
    {
        public static string GoogleRefreshTokenPath = Directory.GetCurrentDirectory() + "\\GoogleToken.dat";

        public const string GoogleOauthTokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";

        public const string GoogleOauthEndpoint = "https://accounts.google.com/o/oauth2/device/code";

        public const string GoogleClientId = "848232511240-73ri3t7plvk96pj4f85uj8otdat2alem.apps.googleusercontent.com";

        public const string GoogleClientSecret = "NCjF1TLi2CcY6t5mt0ZveuL7";

        public const string GoogleEmailUri = "openid email https://www.googleapis.com/auth/userinfo.email";

        public const string GoogleDeviceUri = "http://www.google.com/device";

        public const string GoogleDeviceAuthUri = "http://oauth.net/grant_type/device/1.0";

        public const int GoogleGetDeviceCodeDelayInMs = 2000;

        public const int NianticConnectionRetryCount = 25;

        public const int NianticConnectionRetryDelayInMs = 1000;

        public const double DefaultAltitude = 10;

        public const string NianticRpcUrl = @"https://pgorelease.nianticlabs.com/plfe/rpc";

        public const int ScanRange = 10;

        public const int ScanDelayInSeconds = 30;

        //Edit below
        public const double DefaultLatitude = 47.651144;

        public const double DefaultLongitude = -122.130576;

        public static List<PokemonId> PokemonsDisplayInWhite = new List<PokemonId>
        {
            PokemonId.Caterpie,
            PokemonId.Drowzee,
            PokemonId.Pidgey,
            PokemonId.Rattata,
            PokemonId.Spearow,
            PokemonId.Weedle,
            PokemonId.Zubat,
        };
    }
}
