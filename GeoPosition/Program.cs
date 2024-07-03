using System;
using System.Device.Location;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LocationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Request high accuracy

            watcher.StatusChanged += Watcher_StatusChanged;
            watcher.PositionChanged += async (s, e) => await Watcher_PositionChanged(e);

            Console.WriteLine("Starting GeoCoordinateWatcher...");
            watcher.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void Watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Ready:
                    Console.WriteLine("Location service is ready.");
                    break;
                case GeoPositionStatus.Initializing:
                    Console.WriteLine("Location service is initializing.");
                    break;
                case GeoPositionStatus.NoData:
                    Console.WriteLine("No location data is available.");
                    break;
                case GeoPositionStatus.Disabled:
                    Console.WriteLine("Location services are disabled.");
                    break;
            }
        }

        private static async Task Watcher_PositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var coord = e.Position.Location;

            if (!coord.IsUnknown)
            {
                Console.WriteLine($"Latitude: {coord.Latitude}, Longitude: {coord.Longitude}");

                // Perform reverse geocoding
                string address = await GetAddressFromCoordinates(coord.Latitude, coord.Longitude);
                Console.WriteLine($"Address: {address}");
            }
            else
            {
                Console.WriteLine("Location is unknown.");
            }
        }

        private static async Task<string> GetAddressFromCoordinates(double latitude, double longitude)
        {
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0"); // Nominatim requires a user agent
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                var address = json["display_name"]?.ToString();

                return address ?? "Address not found";
            }
        }
    }
}