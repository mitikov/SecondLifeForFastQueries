using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;

namespace SecondLife.For.FastQueries.DependencyInjection
{
    public sealed class CustomFactoryRegistration: IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<BaseFactory, DefaultFactoryForCacheableFastQuery>();
        }
    }
}
