using System;
using System.Collections.Generic;
using System.Linq;
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
                case 41:
                    return DisplayAccounts();
                case 51:
                    return DisplayOrders();
                case 22:
                    return AddItem();
                case 32:
                    return AddPerson();
                case 42:
                    return AddAccount();
                case 52:
                    return AddOrder();
                default:
                    Console.WriteLine("Inputed value has no matching menu option, Please try again...");
                    return Task.FromResult("Returning");
            }
        }

        #region Add
        private async Task AddPerson()
        {
            Console.WriteLine("Creating person...");
            var random = new Random();

            var personNumber = new StringBuilder();

            #region PersonNumber Generator
            int day = 0;
            var month = random.Next(1, 12);
            var year = random.Next(0, 99);
            var finalDigets = random.Next(1, 9999);

            int[] longMonths = new[] { 1, 3, 5, 7, 8, 10, 12 };
            int[] shortMonths = new[] { 4, 6, 9, 11 };

            #region Day Setter
            if (longMonths.Contains(month))
            {
                day = random.Next(1, 31);
            }
            else if (shortMonths.Contains(month))
            {
                day = random.Next(1, 30);
            }
            else if (month == 2)
            {
                if (year % 4 == 0)
                {
                    day = random.Next(1, 29);
                }
                else
                {
                    day = random.Next(1, 28);
                }
            }
            #endregion

            #region Appender
            if (day < 10)
                personNumber.Append(0);
            personNumber.Append(day);

            if (month < 10)
                personNumber.Append(0);
            personNumber.Append(month);

            if (year < 10)
                personNumber.Append(0);
            personNumber.Append(year);

            if (finalDigets < 1000)
            {
                personNumber.Append(0);

                if (finalDigets < 100)
                {
                    personNumber.Append(0);

                    if (finalDigets < 10)
                    {
                        personNumber.Append(0);
                    }
                }
            }

            personNumber.Append(finalDigets);
            #endregion

            #endregion

            Console.WriteLine("Person Name ->");
            var name = Console.ReadLine();

            Console.WriteLine("Person Address ->");
            var address = Console.ReadLine();

            var person = new PersonProxy() { Address = address, Name = name, PersonNumber = personNumber.ToString() };

            var response = await bankClient.PostAsJsonAsync(Controllers.People.ToString(), person);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
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
                    Console.WriteLine("Unable to parse input to 'int' - Try Again...");
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
                    Console.WriteLine("Unable to parse input to 'double' - Try Again...");
                }
            }

            var item = new ItemProxy() { Amount = amount, Description = description, ItemNumber = itemNumber.ToString(), Name = name, Price = price };

            var response = await orderClient.PostAsJsonAsync(Controllers.Items.ToString(), item);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private async Task AddAccount()
        {
            Console.WriteLine("Creating Account...");
            var random = new Random();

            Console.WriteLine("Verifying that atleast one person exist...");
            List<PersonProxy> people = new List<PersonProxy>();
            var getResponse = await bankClient.GetByJsonAsync(Controllers.People.ToString(), new PersonProxy());

            if (getResponse.StatusCode != HttpStatusCode.NoContent )
            {
                people = DeserilizeJson(await getResponse.Content.ReadAsStringAsync(), people);

                var accountNumber = new StringBuilder();

                for (int i = 0; i < 10; i++)
                {
                    if (i == 0)
                        accountNumber.Append(random.Next(1, 9));
                    else
                        accountNumber.Append(random.Next(0, 9));
                }

                bool parseBalance = false;
                double balance = 0.00;
                while (parseBalance == false)
                {
                    Console.WriteLine("Account Balance ->");
                    var tmp = Console.ReadLine();

                    tmp = tmp.Replace('.', ',');

                    parseBalance = double.TryParse(tmp, out balance);

                    if (parseBalance == false)
                    {
                        Console.WriteLine("Unable to parse input to 'double' - Try Again...");
                    }
                }

                PersonProxy owner = null;

                while (owner == null)
                {
                    Console.WriteLine("Chose an Owner for the Account...");

                    var builder = new StringBuilder();

                    builder.Append("Pick Number\t");
                    builder.Append("Name\t");

                    Console.WriteLine(builder.ToString());

                    for (int i = 0; i < people.Count; i++)
                    {
                        Console.WriteLine(string.Format("{0} ->\t{1}", i+1, people[i].Name));
                    }

                    bool parse = false;
                    int pick = 0;
                    while (parse == false)
                    {
                        Console.WriteLine("Which one do you chose? ->");
                        var tmp = Console.ReadLine();

                        parse = int.TryParse(tmp, out pick);

                        if (parse == false)
                        {
                            Console.WriteLine("Unable to parse input to 'int' - Try Again...");
                        }
                    }

                    if (pick <= people.Count && pick > 0)
                        owner = people[pick - 1];
                    else
                        Console.WriteLine("Inputed Number doesn't match any people on the list, try again..."); 
                }

                var account = new AccountProxy() {AccountNumber = accountNumber.ToString(), Balance = balance, FK_Owner = owner.Id};

                var postResponse = await bankClient.PostAsJsonAsync(Controllers.Accounts.ToString(), account);

                Console.WriteLine(await postResponse.Content.ReadAsStringAsync());
            }
            else
            {
                Console.WriteLine("No People exist, need at least one to create an Account...");
            }
        }

        private async Task AddOrder()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Display
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

        private async Task DisplayAccounts()
        {
            Console.WriteLine("Displaying all known Accounts...");

            List<AccountProxy> accounts = new List<AccountProxy>();
            var response = await bankClient.GetByJsonAsync(Controllers.Accounts.ToString(), new AccountProxy());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                accounts = DeserilizeJson(await response.Content.ReadAsStringAsync(), accounts);

                var builder = new StringBuilder();

                builder.Append("Id\t");
                builder.Append("AccountNumber\t");
                builder.Append("Balance\t");
                builder.Append("Owner Name\t");

                Console.WriteLine(builder.ToString());

                Print(accounts);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No Accounts exist...");
            }

        }

        private async Task DisplayOrders()
        {
            Console.WriteLine("Displaying all known Orders...");

            List<OrderProxy> orders = new List<OrderProxy>();
            var response = await orderClient.GetByJsonAsync(Controllers.Orders.ToString(), new OrderProxy());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                orders = DeserilizeJson(await response.Content.ReadAsStringAsync(), orders);

                var orderBuilder = new StringBuilder();
                #region Order Builder
                orderBuilder.Append("Id\t");
                orderBuilder.Append("OrderNumber\t");
                orderBuilder.Append("Stage\t");
                orderBuilder.Append("From Account\t");
                orderBuilder.Append("To Account\t");
                orderBuilder.Append("Items Count\t"); 
                #endregion

                var itemBuilder = new StringBuilder();
                #region Item Builder
                itemBuilder.Append("\t");
                itemBuilder.Append("Id\t");
                itemBuilder.Append("ItemNumber\t");
                itemBuilder.Append("Name\t");
                itemBuilder.Append("Price\t");
                itemBuilder.Append("Amount\t");
                itemBuilder.Append("Description"); 
                #endregion

                foreach (var order in orders)
                {
                    Console.WriteLine(Environment.NewLine + orderBuilder.ToString());
                    Console.WriteLine(order.ToString());
                    Console.WriteLine(string.Format("\t{0}", itemBuilder.ToString()));
                    foreach (var item in order.Items)
                    {
                        Console.WriteLine(string.Format("\t{0}", item.ToString()));
                    }
                }
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No Orders exist...");
            }
        }
        #endregion

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
                builder.AppendLine("41 --> Display existing Accounts");
                builder.AppendLine("51 --> Display existing Orders");
                builder.AppendLine("22 --> Add an Item");
                builder.AppendLine("32 --> Add a Person");
                builder.AppendLine("42 --> Add an Account");
                builder.AppendLine("52 --> Add an Order");
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

        #region Helpers
        private void Print<T>(List<T> list) where T : class, IEntity
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
        #endregion
    }
}
