using System.Text.Json;
using System.Text.Json.Serialization;
using EDTVelizy.API;
using EDTVelizy.Core;

namespace EDTVelizy.Demo;

class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Entrer une recherche ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                break;
            
            var request = new FederationRequest { SearchTerm = input };
            var groups = await Endpoints.GetGroups(request);
            Console.WriteLine($"Résultats pour '{input}':");
            foreach (var group in groups)
                Console.WriteLine("- " + group);
            
            Console.WriteLine();
        }
    }
}