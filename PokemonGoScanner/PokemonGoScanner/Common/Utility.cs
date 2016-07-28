namespace PokemonGoScanner.Common
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Models;
    public static class Utility
    {
        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }

        public static double CalculateDistanceInMeters(double sourceLatitude, double sourceLongitude, double destLatitude, double destLongitude)
        {
            var sourceLocation = new GeoCoordinate(sourceLatitude, sourceLongitude);
            var targetLocation = new GeoCoordinate(destLatitude, destLongitude);

            return sourceLocation.GetDistanceTo(targetLocation);
        }

        public static RSAParameters KeyFromB64(string b64Key)
        {
            byte[] decoded = Convert.FromBase64String(b64Key);
            int modLength = BitConverter.ToInt32(decoded.Take(4).Reverse().ToArray(), 0);
            byte[] mod = decoded.Skip(4).Take(modLength).ToArray();
            int expLength = BitConverter.ToInt32(decoded.Skip(modLength + 4).Take(4).Reverse().ToArray(), 0);
            byte[] exponent = decoded.Skip(modLength + 8).Take(expLength).ToArray();
            RSAParameters rsaKeyInfo = new RSAParameters();
            rsaKeyInfo.Modulus = mod;
            rsaKeyInfo.Exponent = exponent;
            return rsaKeyInfo;
        }

        public static byte[] KeyToStruct(RSAParameters key)
        {
            byte[] modLength = { 0x00, 0x00, 0x00, 0x80 };
            byte[] mod = key.Modulus;
            byte[] expLength = { 0x00, 0x00, 0x00, 0x03 };
            byte[] exponent = key.Exponent;
            return CombineBytes(modLength, mod, expLength, exponent);
        }

        public static Dictionary<string, string> ParseAuthResponse(string text)
        {
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            foreach (string line in text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split('=');
                responseData.Add(parts[0], parts[1]);
            }
            return responseData;
        }

        public static string GenerateGoogleMapLink(double latitude, double longitude, string text = "Google Map")
        {
            var sb = new StringBuilder();
            sb.Append($"<a href=\"{Constant.GoogleMapUrl}");
            sb.Append(latitude);
            sb.Append(",");
            sb.Append(longitude);
            sb.Append($"&z=17\">{text}</a>");
            return sb.ToString();
        }

        public static string GetDespawnString(WildPokemon pokemon)
        {
            var despawnSeconds = pokemon.TimeTillHiddenMs;
            var despawnMinutes = despawnSeconds / 60;
            despawnSeconds = despawnSeconds % 60;
            return $", despawn in {despawnMinutes} minutes { despawnSeconds} seconds";
        }

        public static string GetDespawnString(FortLureInfo lureInfo)
        {
            var despawnSeconds = (lureInfo.LureExpiresTimestampMs - DateTime.UtcNow.ToUnixTime()) / 1000;
            var despawnMinutes = despawnSeconds / 60;
            despawnSeconds = despawnSeconds % 60;
            return $", despawn in {despawnMinutes} minutes { despawnSeconds} seconds";
        }

        public static string GetDespawnString(MapPokemon pokemon)
        {
            var despawnSeconds = (pokemon.ExpirationTimestampMs - DateTime.UtcNow.ToUnixTime()) / 1000;
            var despawnMinutes = despawnSeconds / 60;
            despawnSeconds = despawnSeconds % 60;
            return $", despawn in {despawnMinutes} minutes { despawnSeconds} seconds";
        }

        public static string CreateSignature(string email, string password, RSAParameters key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(key);
            SHA1 sha1 = SHA1.Create();
            byte[] prefix = { 0x00 };
            byte[] hash = sha1.ComputeHash(KeyToStruct(key)).Take(4).ToArray();
            byte[] encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(email + "\x00" + password), true);
            return UrlSafeBase64(CombineBytes(prefix, hash, encrypted));
        }

        public static ulong FloatAsUlong(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static string UrlSafeBase64(byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray).Replace('+', '-').Replace('/', '_');
        }

        public static byte[] CombineBytes(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
