using AYI.Core.DataAccess.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AYI.Core.DataAccess;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSqliteConnectionFactory(this IServiceCollection services)
	{
		services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
		return services;
	}
}
