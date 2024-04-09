using AYI.Core.DataAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AYI.Core.DataAccess;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSqliteConnectionFactory(this IServiceCollection services)
	{
		services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
		return services;
	}
}
