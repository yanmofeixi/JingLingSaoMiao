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

        public const string AndroidService = "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com";

        public const string AndroidAppName = "com.nianticlabs.pokemongo";

        public const string AndroidClientSignature = "321187995bc7cdc2b5fc91b11a96e2baa8602c62";

        public const string b64Key = "AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pKRI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/6rmf5AAAAAwEAAQ==";

        public const string version = "0.0.5";

        public const string authUrl = "https://android.clients.google.com/auth";

        public static string userAgent = "GPSOAuthSharp/" + version;

        public const int NianticConnectionRetryCount = 25;

        public const int NianticConnectionRetryDelayInMs = 1000;

        public const double DefaultAltitude = 10;

        public const string NianticRpcUrl = @"https://pgorelease.nianticlabs.com/plfe/rpc";

        public const int ScanRange = 10;

        public const int ScanDelayInSeconds = 60;

        public const string EmailHost = "smtp.gmail.com";

        public const int RestartDelayInMs = 10000;

#if DEBUG
        public const bool UseEmailPasswordToLogin = false;

        public const bool EnableEmailAlert = false;
#else
        public const bool UseEmailPasswordToLogin = true;

        public const bool EnableEmailAlert = true;
#endif
    }
}
