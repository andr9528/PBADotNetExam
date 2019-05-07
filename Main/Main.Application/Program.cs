using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application
{
    class Program
    {
        readonly HttpClient orderClient = new HttpClient();
        readonly HttpClient bankClient = new HttpClient();

        private string Menu = "";
        private int MenuSelection = -1;

        static void Main(string[] args)
        {
            Console.WriteLine("Your Computer is called: {0}", Environment.MachineName);

            var startup = new Startup();
            var services = new ServiceCollection();

            startup.ConfigureServices(services);

            var provider = services.BuildServiceProvider();

            var program = new Program();
            program.RunAsync(provider).GetAwaiter().GetResult();
        }

        private async Task RunAsync(ServiceProvider provider)
        {
            Configure();

            while (MenuSelection != 0)
            {
                ShowMenu();
                await HandleRequest(provider);
            }
        }

        private Task HandleRequest(ServiceProvider provider)
        {
            throw new NotImplementedException();
        }

        private void ShowMenu()
        {
            if (Menu == "")
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Type the marked number of the desired action and hit enter...");
                builder.AppendLine("0 --> Exit Program");

                Menu = builder.ToString();
                Console.WriteLine(Menu);
            }
            else
                Console.WriteLine(Menu);
        }

        /// <summary>
        /// Configures the HttpClients for the api's of the micro services
        /// </summary>
        private void Configure()
        {
            orderClient.BaseAddress = new Uri("http://localhost:5001/api/");
            orderClient.DefaultRequestHeaders.Accept.Clear();
            orderClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            bankClient.BaseAddress = new Uri("http://localhost:5001/api/");
            bankClient.DefaultRequestHeaders.Accept.Clear();
            bankClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
