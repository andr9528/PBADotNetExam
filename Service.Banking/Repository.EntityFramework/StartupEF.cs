using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Core;


namespace Repository.EntityFramework
{
    public class StartupEf : IStartupEF
    {
        public StartupEf(IConfiguration configuration, string connectionStringName)
        {
            Configuration = configuration;
            ConnectionStringName = connectionStringName;
        }

        public IConfiguration Configuration { get; }
        private  string ConnectionStringName { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EntityRepository>(option => option.UseSqlServer(Configuration.GetConnectionString(ConnectionStringName)));

            services.AddTransient(typeof(IGenericRepository), typeof(GenericEntityRepositoryHandler));
            services.AddTransient(typeof(ISerializableRepository), typeof(SerializableEntityRepositoryHandler));
        }
    }

    public interface IStartupEF
    {
        void ConfigureServices(IServiceCollection services);
    }
}
