using Microsoft.Extensions.DependencyInjection;
using OpenFTTH.EventSourcing;
using OpenFTTH.EventSourcing.InMem;
using System.Reflection;

namespace OpenFTTH.Core.Address.Tests;

internal static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemEventStore>();
        {
            var businessAssemblies = new Assembly[]
            {
                AppDomain.CurrentDomain.Load("OpenFTTH.Core.Address"),
            };

            services.AddProjections(businessAssemblies);
        }
    }
}
