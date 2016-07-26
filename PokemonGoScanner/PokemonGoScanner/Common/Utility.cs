namespace PokemonGoScanner.Common
{
    using System;
    public class Utility
    {
        public static ulong FloatAsUlong(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
