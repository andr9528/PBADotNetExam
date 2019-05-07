using Main.Base.Startup;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application
{
    class Startup : StartupBase
    {
        public Startup() : base(connectionStringName: ConnectionString)
        {
            
        }

        private static string ConnectionString
        {
            get
            {
                switch (Environment.MachineName)
                {
                    case "ANDRE-PC":
                        return "WolfStatic";
                    default:
                        return "Default";
                }
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SetupServices(services);
        }
    }
}
