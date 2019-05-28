using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Banking.Repository.EntityFramework;
using Shared.Base.Startup.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Service.Banking.Base.Startup
{
    public abstract class StartupBase
    {
        internal List<IStartup> modules = new List<IStartup>();

        public StartupBase(IConfiguration config = null, string connectionStringName = "")
        {
            if (config == null)
            {
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                Configuration = builder.Build(); 
            }
            else
                Configuration = config;

            if (connectionStringName != "")
            {
                modules.Add(new StartupEf(Configuration, connectionStringName));
            }
        }

        public IConfiguration Configuration { get; protected set; }

        public void SetupServices(IServiceCollection services)
        {
            Console.WriteLine("Setting up Services...");

            foreach (var module in modules)
            {
                module.SetupServices(services);
            }
        }
    }
}
