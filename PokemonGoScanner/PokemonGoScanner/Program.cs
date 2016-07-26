namespace PokemonGoScanner
{
    using System;
    using System.Threading.Tasks;

    using Common;
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var googleLogin = new GoogleLogin();
                var nianticClient = new NianticClient();
                Task.Run(() => {
                    googleLogin.LoginAsync().Wait();
                    nianticClient.InitializeAsync(googleLogin.accessToken).Wait();
                    Console.WriteLine($"Start scanning at Latitude:{Constant.DefaultLatitude}, Longitude:{Constant.DefaultLongitude}");
                    nianticClient.ScanAsync().Wait();
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
