using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Main.Domain.Concrete;
using Main.Domain.Core;
using Main.Domain.Enums;
using Main.Domain.Proxies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Service.Ordering.Domain.Core;
using Service.Ordering.Domain.Enums;
using Shared.Extensions;
using Shared.Repository.Core;
using Action = Main.Domain.Enums.Action;

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
                case 53:
                    return ProcessOrders(false);
                case 54:
                    return ProcessOrders(true);
                default:
                    Console.WriteLine("Inputed value has no matching menu option, Please try again...");
                    return Task.FromResult("Returning");
            }
        }
        /// <summary>
        /// https://markheath.net/post/constraining-concurrent-threads-csharp
        /// </summary>
        /// <param name="async"></param>
        /// <returns></returns>
        private async Task ProcessOrders(bool async)
        {
            Console.WriteLine("Processing orders --> asynchronous = {0}", async);

            List<OrderProxy> orders = new List<OrderProxy>();
            var response = await orderClient.GetByJsonAsync(Controllers.Orders.ToString(), new OrderProxy() { Items = new List<IItem>(), Stage = OrderStage.New});

            if (response.StatusCode == HttpStatusCode.OK)
            {
                orders = DeserilizeJson<OrderProxy>(await response.Content.ReadAsStringAsync());

                if (async)
                {
                    var maxThreads = 4;
                    var queue = new ConcurrentQueue<OrderProxy>(orders);
                    var tasks = new List<Task>();

                    for (int i = 0; i < maxThreads; i++)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            while (queue.TryDequeue(out OrderProxy order))
                            {
                                await ProcessOrder(order);
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);
                }
                else
                {
                    foreach (var order in orders)
                    {
                        await ProcessOrder(order);
                    }
                }
            }
            else
                Console.WriteLine("No new Orders exist...");
        }

        private async Task ProcessOrder(OrderProxy order)
        {
            order.Stage = OrderStage.Proccessing;
            var orderUpdate1 = await orderClient.PutAsJsonAsync(Controllers.Orders.ToString(), order);
            IEvent @event = null;

            if (orderUpdate1.StatusCode == HttpStatusCode.Accepted)
            {
                Console.WriteLine("Creating Event...");
                #region Create Event
                double totalPrice = 0;
                foreach (var item in order.Items)
                {
                    totalPrice = totalPrice + (item.Price * item.Amount);
                }

                var datas = new List<RollbackData>()
                {
                    new RollbackData() { Action = Action.Decrease, Number = order.FromAccount, Value = totalPrice, Service = Services.Banking},
                    new RollbackData() { Action = Action.Increase, Number = order.ToAccount, Value = totalPrice, Service = Services.Banking}
                };

                foreach (var item in order.Items)
                {
                    datas.Add(new RollbackData() { Action = Action.Decrease, Number = item.ItemNumber, Value = item.Amount, Service = Services.Ordering });
                }

                var dataString = new StringBuilder();

                foreach (var data in datas)
                {
                    dataString.AppendLine(data.ToString());
                } 
                #endregion

                @event = new Event()
                {
                    Stage = EventStage.TransferMoney, OrderNumber = order.OrderNumber,
                    RollbackDatas = new List<IRollbackData>(datas), DatasAsString = dataString.ToString()
                };

                Console.WriteLine("Adding the new Event to Database...");
                var createEvent = await mainClient.PostAsJsonAsync(Controllers.Events.ToString(), @event);

                if (createEvent.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("Getting the new Event from Database with the Id, that is required to continue...");
                    var getEvent = await mainClient.GetByJsonAsync(Controllers.Events.ToString(),
                        new Event() {OrderNumber = @event.OrderNumber, RollbackDatas = new List<IRollbackData>()});

                    if (getEvent.StatusCode == HttpStatusCode.OK)
                    {
                        @event = DeserilizeJson<Event>(await getEvent.Content.ReadAsStringAsync())
                            .Find(x => x.OrderNumber == @event.OrderNumber);

                        var transfer = (await Transfer(@event));
                        @event = transfer.@event;

                        if (@event.Stage == EventStage.StockUpdate)
                        {
                            var update = (await StockUpdate(@event));
                            @event = update.@event;

                            if (@event.Stage == EventStage.Completed)
                            {
                                order.Stage = OrderStage.Completed;
                                var orderUpdate2 = await orderClient.PutAsJsonAsync(Controllers.Orders.ToString(), order);

                                if (orderUpdate2.StatusCode == HttpStatusCode.Accepted)
                                {
                                    Console.WriteLine("Completed processing of Order {0}", order.OrderNumber);
                                }
                                else
                                {
                                    Console.WriteLine("Something went wrong updating Stage of Order {0} to Completed. --> {1}", order.OrderNumber, await orderUpdate2.Content.ReadAsStringAsync());
                                }
                            }
                            else
                            {
                                Console.WriteLine("Something went wrong updating the item stock, rolling back");
                                await StockUpdate(update.changes);
                                @event = (await Transfer(@event, true)).@event;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong transfering money, rolling back");
                            if (transfer.data == null)
                            {
                                @event = (await Transfer(@event, true)).@event;
                            }
                            else
                            {
                                @event = await Transfer(transfer.data, @event);
                            }
                        }
                    }
                    else
                        Console.WriteLine("Something went wrong getting the Event --> {0}", await getEvent.Content.ReadAsStringAsync());
                }
                else
                    Console.WriteLine("Something went wrong adding the Event --> {0}", await createEvent.Content.ReadAsStringAsync());
            }
            else
                Console.WriteLine("Something went wrong updating Stage of Order {0} to Processing. --> {1}", order.OrderNumber, await orderUpdate1.Content.ReadAsStringAsync());


        }

        private async Task StockUpdate(List<IRollbackData> changes)
        {
            
        }

        private async Task<(IEvent @event, List<IRollbackData> changes)> StockUpdate(IEvent @event)
        {
            var itemChanges = @event.RollbackDatas.Select(x => x).Where(x => x.Service == Services.Ordering).ToList();
            var successfulChanges = new List<IRollbackData>();

            foreach (var data in itemChanges)
            {
                var itemRequest = await orderClient.GetByJsonAsync(Controllers.Items.ToString(),
                    new ItemProxy() {ItemNumber = data.Number, Position = ItemPosition.Storage});

                if (itemRequest.StatusCode == HttpStatusCode.OK)
                {
                    var item = DeserilizeJson<ItemProxy>(await itemRequest.Content.ReadAsStringAsync()).FirstOrDefault();

                    if (item.Amount - data.Value >= 0)
                    {
                        item.Amount = (int)(item.Amount - data.Value);

                        var itemUpdate = await orderClient.PutAsJsonAsync(Controllers.Items.ToString(), item);

                        if (itemUpdate.StatusCode == HttpStatusCode.Accepted)
                        {
                            successfulChanges.Add(data);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        @event = await UpdateEvent(string.Format("Update Stock Failed - Out of stock of item: {0}", item.Name), EventStage.OutOfStock, @event);

                        return ValueTuple.Create(@event, successfulChanges);
                    }
                }
                else
                {
                    @event = await UpdateEvent("Update Stock Failed - Unable to locate Item", EventStage.MissingItem, @event);

                    return ValueTuple.Create(@event, successfulChanges);
                }
            }

            if (successfulChanges.Count == itemChanges.Count)
            {
                @event = await UpdateEvent("Update Stock Successful", EventStage.Completed, @event);

                return ValueTuple.Create(@event, successfulChanges);
            }
            else
            {
                @event = await UpdateEvent("Update Stock Failed - Failed to Update an Item", EventStage.UpdateError, @event);

                return ValueTuple.Create(@event, successfulChanges);
            }
        }

        //Method to rollback balance of a specific Account
        private async Task<IEvent> Transfer(IRollbackData data, IEvent @event)
        {
            Console.WriteLine("Rolling back account balance change...");

            var accountResponse = await orderClient.GetByJsonAsync(Controllers.Accounts.ToString(),
                new AccountProxy() {AccountNumber = data.Number});

            if (accountResponse.StatusCode == HttpStatusCode.OK)
            {
                var account = DeserilizeJson<AccountProxy>(await accountResponse.Content.ReadAsStringAsync()).FirstOrDefault();

                switch (data.Action)
                {
                    case Action.Decrease:
                        account.Balance = account.Balance + data.Value;
                        break;
                    case Action.Increase:
                        if (account.Balance - data.Value >= 0)
                            account.Balance = account.Balance - data.Value;
                        else
                        {
                            @event = await UpdateEvent("Transfer Failed - Insuficient Funds", EventStage.TotalFailure, @event);
                            return @event;
                        }
                        break;
                    default:
                        @event = await UpdateEvent("Transfer Failed - Unable to Determine action", EventStage.TotalFailure, @event);
                        return @event;
                }

                var updateResponse = await orderClient.PutAsJsonAsync(Controllers.Accounts.ToString(), account);

                if (updateResponse.StatusCode == HttpStatusCode.Accepted)
                    @event = await UpdateEvent("Transfer Successful - Account Rollback Completed", EventStage.Rollbacked, @event);
                else
                    @event = await UpdateEvent("Transfer Failed - Update Account Error", EventStage.TotalFailure, @event);

                return @event;
            }
            else
            {
                @event = await UpdateEvent("Transfer Failed - Unable to find Account", EventStage.TotalFailure, @event);
                return @event;
            }
        }

        private async Task<(IEvent @event, IRollbackData data)> Transfer(IEvent @event, bool rollback = false)
        {
            var from = @event.RollbackDatas.FirstOrDefault(x => x.Service == Services.Banking & x.Action == Action.Decrease).Number;
            var to = @event.RollbackDatas.FirstOrDefault(x => x.Service == Services.Banking & x.Action == Action.Increase).Number;

            if (rollback)
            {
                Console.WriteLine("Rolling back money transfer...");

                var tmp = from;
                from = to;
                to = tmp;
            }

            var fromAccountResponse = orderClient.GetByJsonAsync(Controllers.Accounts.ToString(), new AccountProxy() { AccountNumber = from });
            var toAccountResponse = orderClient.GetByJsonAsync(Controllers.Accounts.ToString(), new AccountProxy() { AccountNumber = to });
            var amount = @event.RollbackDatas.FirstOrDefault(x => x.Service == Services.Banking).Value;

            if ((await fromAccountResponse).StatusCode == HttpStatusCode.OK & (await toAccountResponse).StatusCode == HttpStatusCode.OK)
            {
                var fromAccount =
                        DeserilizeJson<AccountProxy>(await (await fromAccountResponse).Content.ReadAsStringAsync())
                            .FirstOrDefault(x => x.AccountNumber == from);
                var toAccount =
                    DeserilizeJson<AccountProxy>(await (await toAccountResponse).Content.ReadAsStringAsync())
                        .FirstOrDefault(x => x.AccountNumber == to);

                if (fromAccount.Balance - amount >= 0)
                {
                    fromAccount.Balance = fromAccount.Balance - amount;
                    toAccount.Balance = toAccount.Balance + amount;

                    var fromAccountResult = orderClient.PutAsJsonAsync(Controllers.Accounts.ToString(), fromAccount);
                    var toAccountResult = orderClient.PutAsJsonAsync(Controllers.Accounts.ToString(), toAccount);

                    if ((await fromAccountResult).StatusCode == HttpStatusCode.Accepted & (await toAccountResult).StatusCode == HttpStatusCode.Accepted)
                    {
                        if (!rollback)
                            @event = await UpdateEvent("Transfer Successful", EventStage.StockUpdate, @event);
                        else
                            @event = await UpdateEvent("Transfer Successful - Accounts Rollback Completed", EventStage.Rollbacked, @event);

                        var updateEvent = await mainClient.PutAsJsonAsync(Controllers.Events.ToString(), @event);
                        Console.WriteLine("Update Event StatusCode --> {0}", updateEvent.StatusCode.ToString());
                        return ValueTuple.Create<IEvent, IRollbackData>(@event, null);
                    }
                    else if ((await fromAccountResult).StatusCode == HttpStatusCode.Conflict & (await toAccountResult).StatusCode == HttpStatusCode.Conflict)
                    {
                        if (!rollback)
                            @event = await UpdateEvent("Transfer Failed - Update Accounts Error", EventStage.UpdateError, @event);
                        else
                            @event = await UpdateEvent("Transfer Failed - Update Accounts Error", EventStage.TotalFailure, @event);

                        return ValueTuple.Create<IEvent, IRollbackData>(@event, null);
                    }
                    else if ((await fromAccountResult).StatusCode == HttpStatusCode.Conflict | (await toAccountResult).StatusCode == HttpStatusCode.Conflict)
                    {
                        if (!rollback)
                            @event = await UpdateEvent("Transfer Failed - Update Account Error", EventStage.UpdateError, @event);
                        else
                            @event = await UpdateEvent("Transfer Failed - Update Account Error", EventStage.TotalFailure, @event);

                        IRollbackData data = null;

                        if ((await fromAccountResult).StatusCode == HttpStatusCode.Conflict)
                            data = @event.RollbackDatas.Select(x => x).Where(x =>
                                x.Action == Action.Decrease & x.Service == Services.Banking).FirstOrDefault();
                        else if ((await toAccountResult).StatusCode == HttpStatusCode.Conflict)
                            data = @event.RollbackDatas.Select(x => x).Where(x =>
                                x.Action == Action.Increase & x.Service == Services.Banking).FirstOrDefault();
                        else
                            Console.WriteLine("Huh? how did you get here? o.O");

                        return ValueTuple.Create(@event, data);
                    }
                    else
                    {
                        if (!rollback)
                            @event = await UpdateEvent("Transfer Failed - Unknown Error", EventStage.UnknownError, @event);
                        else
                            @event = await UpdateEvent("Transfer Failed - Unknown Error", EventStage.TotalFailure, @event);

                        return ValueTuple.Create<IEvent, IRollbackData>(@event, null);
                    }
                }
                else
                {
                    if (!rollback)
                        @event = await UpdateEvent("Transfer Failed - Insuficient Funds", EventStage.InsuficientFunds, @event);
                    else
                        @event = await UpdateEvent("Transfer Failed - Insuficient Funds", EventStage.TotalFailure, @event);

                    return ValueTuple.Create<IEvent, IRollbackData>(@event, null);
                }
            }
            else
            {
                var builder = new StringBuilder();

                builder.Append("Transfer Failed - ");

                if ((await fromAccountResponse).StatusCode == HttpStatusCode.NoContent)
                    builder.Append("Unable to find Buyer Account");
                if ((await fromAccountResponse).StatusCode == HttpStatusCode.NoContent & (await toAccountResponse).StatusCode == HttpStatusCode.NoContent)
                    builder.Append(", ");
                if ((await toAccountResponse).StatusCode == HttpStatusCode.NoContent)
                    builder.Append("Unable to find Seller Account");

                if (!rollback)
                    @event = await UpdateEvent(builder.ToString(), EventStage.MissingAccount, @event);
                else
                    @event = await UpdateEvent(builder.ToString(), EventStage.TotalFailure, @event);

                return ValueTuple.Create<IEvent, IRollbackData>(@event, null);
            }
        }

        private async Task<IEvent> UpdateEvent(string message, EventStage stage, IEvent @event)
        {
            Console.WriteLine(message);

            @event.Stage = stage;

            var updateEvent = await mainClient.PutAsJsonAsync(Controllers.Events.ToString(), @event);
            Console.WriteLine("Update Event StatusCode --> {0}, Event Stage --> {1}", updateEvent.StatusCode.ToString(), @event.Stage.ToString());

            return @event;
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

            var response = bankClient.PostAsJsonAsync(Controllers.People.ToString(), person);

            Console.WriteLine(await (await response).Content.ReadAsStringAsync());
        }

        private async Task AddItem()
        {
            Console.WriteLine("Creating item...");
            var random = new Random();

            var itemNumber = new StringBuilder();

            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                    itemNumber.Append(random.Next(1, 9));
                else
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

            var item = new ItemProxy() { Amount = amount, Description = description, ItemNumber = itemNumber.ToString(), Name = name, Price = price, Position = ItemPosition.Storage};

            var response = orderClient.PostAsJsonAsync(Controllers.Items.ToString(), item);

            Console.WriteLine(await (await response).Content.ReadAsStringAsync());
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
                people = DeserilizeJson<PersonProxy>(await getResponse.Content.ReadAsStringAsync());

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

                var builder = new StringBuilder();

                builder.Append("Pick Number\t");
                builder.Append("Name\t");

                while (owner == null)
                {
                    Console.WriteLine("Chose an Owner for the Account...");

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

                var postResponse = bankClient.PostAsJsonAsync(Controllers.Accounts.ToString(), account);

                Console.WriteLine(await (await postResponse).Content.ReadAsStringAsync());
            }
            else
            {
                Console.WriteLine("No People exist, need at least one to create an Account...");
            }
        }

        private async Task AddOrder()
        {
            Console.WriteLine("Creating Order...");
            var random = new Random();

            var accounts = VerifyAccounts();

            //var people = VerifyPeople();

            var items = VerifyItems();

            if (/*(await people).check &*/ (await accounts).check & (await items).check)
            {
                var orderNumber = new StringBuilder();

                for (int i = 0; i < 16; i++)
                {
                    if (i == 0)
                        orderNumber.Append(random.Next(1, 9));
                    else
                        orderNumber.Append(random.Next(0, 9));
                }

                Console.WriteLine();
                Console.WriteLine("First chose the account of the buyer...");
                AccountProxy fromAccount = PickAccount((await accounts).accounts);
                Console.WriteLine();

                Console.WriteLine("Secoundly chose the account of the seller...");
                AccountProxy toAccount = PickAccount((await accounts).accounts
                    .Where(x => x.Owner.PersonNumber != fromAccount.Owner.PersonNumber).ToList());
                Console.WriteLine();

                Console.WriteLine("Finally chose the items that the buyer wish to buy...");
                List<ItemProxy> orderItems = PickItems((await items).items);

                OrderProxy order = new OrderProxy()
                {
                    Items = new List<IItem>(orderItems), OrderNumber = orderNumber.ToString(),
                    FromAccount = fromAccount.AccountNumber, ToAccount = toAccount.AccountNumber, Stage = OrderStage.New
                };

                var postResponse = orderClient.PostAsJsonAsync(Controllers.Orders.ToString(), order);

                Console.WriteLine(await (await postResponse).Content.ReadAsStringAsync());
            }
        }

        #region Add Order Methods
        private List<ItemProxy> PickItems(List<ItemProxy> items)
        {
            Console.WriteLine();
            List<ItemProxy> resultItems = new List<ItemProxy>();
            int menuPick = -1;

            #region String Builders
            var builder = new StringBuilder();

            builder.Append("Pick Number\t");
            builder.Append("Id\t");
            builder.Append("ItemNumber\t");
            builder.Append("Name\t");
            builder.Append("Price\t");
            builder.Append("Amount\t");
            builder.Append("Description");

            var menu = new StringBuilder();

            
            menu.AppendLine("1 --> Buy an Item...");
            #endregion

            Console.WriteLine("Welcome to Wolf Marked!...");
            Console.WriteLine("Here is what you can do...");

            do
            {
                bool parse = false;

                #region Menu Selection

                if (menuPick != 0)
                {
                    
                    while (parse == false)
                    {
                        Console.WriteLine(menu.ToString());
                        Console.WriteLine("What do you want to do? ->");
                        var tmp = Console.ReadLine();

                        parse = int.TryParse(tmp, out menuPick);

                        if (parse == false)
                            Console.WriteLine("Unable to parse input to 'int' - try Again...");
                    }

                    Console.WriteLine(); 
                }

                #endregion

                if (menuPick == 1)
                {
                    ItemProxy template = null;

                    while (template == null)
                    {
                        List<ItemProxy> workList = null;

                        if (resultItems.Count == 0)
                            workList = items;
                        else
                            workList = items.Where(x => resultItems.Any(y => x.ItemNumber != y.ItemNumber)).ToList();

                        Console.WriteLine(builder.ToString());

                        for (int i = 0; i < workList.Count; i++)
                            Console.WriteLine(string.Format("{0} ->\t{1}", i + 1, workList[i].ToString()));
                        Console.WriteLine();

                        #region Item to Buy Determinator

                        parse = false;
                        int pick = 0;
                        while (parse == false)
                        {
                            Console.WriteLine("Which one do you chose? ->");
                            var tmp = Console.ReadLine();

                            parse = int.TryParse(tmp, out pick);

                            if (parse == false)
                                Console.WriteLine("Unable to parse input to 'int' - try Again...");
                        }

                        Console.WriteLine();

                        #endregion

                        if (pick <= workList.Count && pick > 0)
                            template = workList[pick - 1];
                        else
                            Console.WriteLine("Inputed Number doesn't match any people on the list, try again...");
                    }

                    #region Amount to Buy Determinator

                    parse = false;
                    int amount = 0;
                    while (parse == false)
                    {
                        Console.WriteLine("How many would you like to buy? ->");
                        var tmp = Console.ReadLine();

                        parse = int.TryParse(tmp, out amount);

                        if (parse == false)
                            Console.WriteLine("Unable to parse input to 'int' - try Again...");
                    }

                    #endregion

                    if (resultItems.Count == 0)
                        menu.AppendLine("0 --> End Shopping...");

                    resultItems.Add(new ItemProxy()
                    {
                        Amount = amount,
                        Position = ItemPosition.Order,
                        Name = template.Name,
                        Description = template.Description,
                        ItemNumber = template.ItemNumber,
                        Price = template.Price
                    });

                    Console.WriteLine();
                    if (resultItems.Count == items.Count)
                    {
                        Console.WriteLine("You have bought somthing of everything we own");
                        Console.WriteLine("Thanks for shopping at Wolf Marked, hope to see you soon!");
                        menuPick = 0;
                    }
                    else
	                {
                        Console.WriteLine("Continue shopping or are you done?..."); 
                    }
                }
                else if (menuPick == 0 & resultItems.Count != 0)
                {
                    Console.WriteLine("Thanks for shopping at Wolf Marked, hope to see you soon!");
                }
                else
                {
                    Console.WriteLine("Unknown menu selection, try again...");
                }
            } while (menuPick != 0);

            return resultItems;
        }

        private AccountProxy PickAccount(List<AccountProxy> accounts)
        {
            AccountProxy result = null;

            var builder = new StringBuilder();

            builder.Append("Pick Number\t");
            builder.Append("Id\t");
            builder.Append("AccountNumber\t");
            builder.Append("Balance\t");
            builder.Append("Owner Name\t");

            while (result == null)
            {
                Console.WriteLine(builder.ToString());

                for (int i = 0; i < accounts.Count; i++)
                {
                    Console.WriteLine(string.Format("{0} ->\t{1}", i + 1, accounts[i].ToString()));
                }

                bool parse = false;
                int pick = 0;
                while (parse == false)
                {
                    Console.WriteLine();
                    Console.WriteLine("Which one do you chose? ->");
                    var tmp = Console.ReadLine();

                    parse = int.TryParse(tmp, out pick);

                    if (parse == false)
                    {
                        Console.WriteLine("Unable to parse input to 'int' - Try Again...");
                    }
                }

                if (pick <= accounts.Count && pick > 0)
                    result = accounts[pick - 1];
                else
                    Console.WriteLine("Inputed Number doesn't match any people on the list, try again...");
            }

            return result;
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns></returns>
        private async Task<(bool check, List<PersonProxy> people)> VerifyPeople()
        {
            var people = new List<PersonProxy>();
            bool check = false;

            var peopleResponse = await bankClient.GetByJsonAsync(Controllers.People.ToString(), new PersonProxy());
            if (peopleResponse.StatusCode != HttpStatusCode.NoContent)
            {
                people = DeserilizeJson<PersonProxy>(await peopleResponse.Content.ReadAsStringAsync());
                if (people.Count > 1)
                {
                    Console.WriteLine("Enough people exist...");
                    check = true;
                }
                else
                {
                    Console.WriteLine("Only one Person exist, need at least two to create an Order...");
                }
            }
            else
            {
                Console.WriteLine("No People exist, need at least two to create an Order...");
            }

            return ValueTuple.Create(check, people);
        }

        private async Task<(bool check, List<AccountProxy> accounts)> VerifyAccounts()
        {
            List<AccountProxy> accounts = new List<AccountProxy>();
            bool check = false;

            var accountsResponse = await bankClient.GetByJsonAsync(Controllers.Accounts.ToString(), new AccountProxy());
            if (accountsResponse.StatusCode != HttpStatusCode.NoContent)
            {
                accounts = DeserilizeJson<AccountProxy>(await accountsResponse.Content.ReadAsStringAsync());
                if (accounts.Count > 1)
                {
                    int diffrentOwnerscount = 0;

                    for (int x = 0; x < accounts.Count; x++)
                    {
                        for (int y = 0; y < accounts.Count; y++)
                        {
                            if (x == y)
                                continue;

                            if (accounts[x].Owner.Name != accounts[y].Owner.Name)
                                diffrentOwnerscount++;
                        }
                    }

                    diffrentOwnerscount = diffrentOwnerscount / 2;

                    if (diffrentOwnerscount >= 2)
                    {
                        Console.WriteLine("Enough accounts with diffrent owners exist...");
                        check = true;
                    }
                    else
                    {
                        Console.WriteLine("Not enough accounts with diffrent owners exist, need at least two with diffrent owners to create an Order...");
                    }
                }
                else
                {
                    Console.WriteLine("Only one Account exist, need at least two with diffrent owners to create an Order...");
                }
            }
            else
            {
                Console.WriteLine("No Accounts exist, need at least two to create an Order...");
            }

            return ValueTuple.Create(check, accounts);
        }

        private async Task<(bool check, List<ItemProxy> items)> VerifyItems()
        {
            List<ItemProxy> items = new List<ItemProxy>();
            bool check = false;

            var itemsResponse = await orderClient.GetByJsonAsync(Controllers.Items.ToString(), new ItemProxy() { Position = ItemPosition.Storage });
            if (itemsResponse.StatusCode != HttpStatusCode.NoContent)
            {
                items = DeserilizeJson<ItemProxy>(await itemsResponse.Content.ReadAsStringAsync());
                Console.WriteLine("Enough Items exist...");
                check = true;
            }
            else
            {
                Console.WriteLine("No Items exist, need at least one to create an Order...");
            }

            return ValueTuple.Create(check, items);
        } 
        #endregion

        #endregion

        #region Display
        private async Task DisplayItems()
        {
            Console.WriteLine("Displaying all known Items...");

            List<ItemProxy> items = new List<ItemProxy>();
            var response = await orderClient.GetByJsonAsync(Controllers.Items.ToString(), new ItemProxy());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                items = DeserilizeJson<ItemProxy>(await response.Content.ReadAsStringAsync());

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
                events = DeserilizeJson<Event>(await response.Content.ReadAsStringAsync());

                var builder = new StringBuilder();

                builder.Append("Id\t");
                builder.Append("Stage\t");
                builder.Append("OrderNumber\t");
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
                people = DeserilizeJson<PersonProxy>(await response.Content.ReadAsStringAsync());

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
                accounts = DeserilizeJson<AccountProxy>(await response.Content.ReadAsStringAsync());

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
            var response = await orderClient.GetByJsonAsync(Controllers.Orders.ToString(), new OrderProxy() {Items = new List<IItem>()});

            if (response.StatusCode == HttpStatusCode.OK)
            {
                orders = DeserilizeJson<OrderProxy>(await response.Content.ReadAsStringAsync());

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
                    Console.WriteLine();
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
                builder.AppendLine("53 --> Process all new orders sequentially");
                builder.AppendLine("54 --> Process all new orders asynchronous");
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

        private List<T> DeserilizeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        } 
        #endregion
    }
}
