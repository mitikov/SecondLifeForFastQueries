using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;

namespace SecondLife.For.FastQueries.DependencyInjection
{
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
