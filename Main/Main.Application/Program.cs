using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Main.Domain.Concrete;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared.Extensions;

namespace Main.Application
{
    class Program
    {
        readonly HttpClient orderClient = new HttpClient();
        readonly HttpClient bankClient = new HttpClient();
        readonly HttpClient mainClient = new HttpClient();

        private string Menu = "";
        private int MenuSelection = -1;

        static void Main(string[] args)
        {
            Console.WriteLine("Your Computer is called: {0}", Environment.MachineName);

            var program = new Program();
            program.RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            Configure();

            while (MenuSelection != 0)
            {
                ShowMenu();
                await HandleRequest();
            }
        }

        private Task HandleRequest()
        {
            bool inputResult = int.TryParse(Console.ReadLine(), out MenuSelection);
            if (inputResult == false)
            {
                Console.WriteLine("Unable to Parse input to 'Int', Please try again...");
                return Task.FromResult("Returning");
            }

            switch (MenuSelection)
            {
                case 0:
                    Console.WriteLine("Shutting Down...");
                    return Task.FromResult("Returning");
                case 1:
                    return DisplayEvents();
                default:
                    Console.WriteLine("Inputed value has no matching menu option, Please try again...");
                    return Task.FromResult("Returning");
            }
        }

        private async Task DisplayEvents()
        {
            Console.WriteLine("Displaying all known Events...");

            List<Event> events = new List<Event>();
            var response = await mainClient.GetByJsonAsync("Events", new Event());
            if (response.IsSuccessStatusCode)
            {
                var stringJson = await response.Content.ReadAsStringAsync();
                events = JsonConvert.DeserializeObject<List<Event>>(stringJson);

                foreach (var @event in events)
                {
                    Console.WriteLine(@event.ToString());
                }
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No Events exist...");
            }
        }

        private void ShowMenu()
        {
            if (Menu == "")
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Type the marked number of the desired action and hit enter...");
                builder.AppendLine("1 --> Display existing Events");
                builder.AppendLine("0 --> Exit Program");

                Menu = builder.ToString();
                Console.WriteLine(Menu);
            }
            else
                Console.WriteLine(Menu);
        }

        /// <summary>
        /// Configures the HttpClients for the api's of the micro services and main api.
        /// </summary>
        private void Configure()
        {
            orderClient.BaseAddress = new Uri("http://localhost:5004/api/");
            orderClient.DefaultRequestHeaders.Accept.Clear();
            orderClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            bankClient.BaseAddress = new Uri("http://localhost:5002/api/");
            bankClient.DefaultRequestHeaders.Accept.Clear();
            bankClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            mainClient.BaseAddress = new Uri("http://localhost:5000/api/");
            mainClient.DefaultRequestHeaders.Accept.Clear();
            mainClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
