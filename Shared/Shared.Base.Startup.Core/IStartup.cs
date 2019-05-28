using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Base.Startup.Core
{
   public interface IStartup
    {
        void SetupServices(IServiceCollection services);
    }
}
