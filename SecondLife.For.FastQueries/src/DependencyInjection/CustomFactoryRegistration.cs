using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;

namespace SecondLife.For.FastQueries.DependencyInjection
{
    /// <summary>
    /// Registers factory to provide home-baked databases with optional fast-query result caching.
    /// </summary>
    [UsedImplicitly]
    public sealed class CustomFactoryRegistration: IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            var defaultFactoryRegistration = serviceCollection.First(descriptor => descriptor.ServiceType == typeof(BaseFactory));            
            serviceCollection.Remove(defaultFactoryRegistration);

            serviceCollection.AddSingleton<BaseFactory, DefaultFactoryForCacheableFastQuery>();
        }
    }
}
