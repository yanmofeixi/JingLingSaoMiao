namespace PokemonGoScanner.Models
{
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
    }
}
