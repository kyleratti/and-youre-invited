using DbAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSqliteConnectionFactory(this IServiceCollection services)
	{
		services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
		return services;
	}
}
