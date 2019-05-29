using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Main.Domain.Concrete;
using Main.Domain.Proxies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared.Extensions;
using Shared.Repository.Core;

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
                case 11:
                    return DisplayEvents();
                case 21:
                    return DisplayItems();
                case 31:
                    return DisplayPeople();
                case 22:
                    return AddItem();
                case 32:
                    return AddPerson();
                default:
                    Console.WriteLine("Inputed value has no matching menu option, Please try again...");
                    return Task.FromResult("Returning");
            }
        }

        private async Task AddPerson()
        {
            throw new NotImplementedException();
        }

        private async Task DisplayPeople()
        {
            Console.WriteLine("Displaying all known People...");

            List<PersonProxy> people = new List<PersonProxy>();
            var response = await bankClient.GetByJsonAsync(Controllers.People.ToString(), new PersonProxy());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                people = DeserilizeJson(await response.Content.ReadAsStringAsync(), people);

                var builder = new StringBuilder();

                builder.Append("Id\t");
                builder.Append("PersonNumber\t");
                builder.Append("Name\t");
                builder.Append("Address\t");

                Console.WriteLine(builder.ToString());

                Print(people);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No People exist...");
            }
        }

        private async Task AddItem()
        {
            Console.WriteLine("Creating item...");
            var random = new Random();

            var itemNumber = new StringBuilder();

            for (int i = 0; i < 12; i++)
            {
                itemNumber.Append(random.Next(0, 9));
            }

            Console.WriteLine("Item Name ->");
            var name = Console.ReadLine();

            Console.WriteLine("Item Description ->");
            var description = Console.ReadLine();

            bool parseAmount = false;
            int amount = 0;
            while (parseAmount == false)
            {
                Console.WriteLine("Item Amount ->");
                var tmp = Console.ReadLine();

                parseAmount = int.TryParse(tmp, out amount);

                if (parseAmount == false)
                {
                    Console.WriteLine("Unable to parse input to int - Try Again...");
                }
            }

            bool parsePrice = false;
            double price = 0.00;
            while (parsePrice == false)
            {
                Console.WriteLine("Item Price ->");
                var tmp = Console.ReadLine();

                tmp = tmp.Replace('.', ',');

                parsePrice = double.TryParse(tmp, out price);

                if (parsePrice == false)
                {
                    Console.WriteLine("Unable to parse input to double - Try Again...");
                }
            }

            var item = new ItemProxy() {Amount = amount, Description = description, ItemNumber = itemNumber.ToString(), Name = name, Price = price};

            var response = await orderClient.PostAsJsonAsync(Controllers.Items.ToString(), item);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private async Task DisplayItems()
        {
            Console.WriteLine("Displaying all known Items...");

            List<ItemProxy> items = new List<ItemProxy>();
            var response = await orderClient.GetByJsonAsync(Controllers.Items.ToString(), new ItemProxy());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                items = DeserilizeJson(await response.Content.ReadAsStringAsync(), items);

                var builder = new StringBuilder();

                builder.Append("Id\t");
                builder.Append("ItemNumber\t");
                builder.Append("Name\t");
                builder.Append("Price\t");
                builder.Append("Amount\t");
                builder.Append("Description");

                Console.WriteLine(builder.ToString());

                Print(items);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No Items exist...");
            }
        }

        private async Task DisplayEvents()
        {
            Console.WriteLine("Displaying all known Events...");

            List<Event> events = new List<Event>();
            var response = await mainClient.GetByJsonAsync(Controllers.Events.ToString(), new Event());
            // Original propertie to test for succes
            //response.IsSuccessStatusCode
            if (response.StatusCode == HttpStatusCode.OK)
            {
                events = DeserilizeJson(await response.Content.ReadAsStringAsync(), events);

                var builder = new StringBuilder();

                builder.Append("Id\t");
                builder.Append("Stage\t");
                builder.Append("Description");

                Console.WriteLine(builder.ToString());

                Print(events);
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

                builder.AppendLine();
                builder.AppendLine("Type the marked number of the desired action and hit enter...");
                builder.AppendLine("11 --> Display existing Events");
                builder.AppendLine("21 --> Display existing Items");
                builder.AppendLine("31 --> Display existing People");
                builder.AppendLine("22 --> Add an Item");
                builder.AppendLine("32 --> Add a Person");
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

        private void Print<T>(List<T> list) where T: class, IEntity
        {
            foreach (var entity in list)
            {
                Console.WriteLine(entity.ToString());
            }
        }

        private List<T> DeserilizeJson<T>(string json, List<T> type)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
    }
}
