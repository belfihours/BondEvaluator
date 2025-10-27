using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Infrastructure.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace BondEvaluator.Infrastructure.DependencyInjection;

public static class RegisterMappers
{
    public static IServiceCollection RegisterExternalServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStreamMapper, StreamMapper>();
        return serviceCollection;
    }
}