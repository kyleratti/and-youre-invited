// See https://aka.ms/new-console-template for more information

using AYI.Core.DatabaseMaintenance;
using AYI.Presentation.DbMaintenanceRunner.Services;
using DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((context, configBuilder) =>
	{
		configBuilder
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

		if (context.HostingEnvironment.IsDevelopment())
			configBuilder.AddUserSecrets<Program>();

		var config = configBuilder.Build();

		var appConfigConnectionString = config.GetConnectionString("AzureAppConfig");

		if (string.IsNullOrEmpty(appConfigConnectionString))
			return;

		configBuilder.AddAzureAppConfiguration(appConfig =>
		{
			appConfig.Connect(appConfigConnectionString)
				.Select(keyFilter: "AYI:*")
				.Select(keyFilter: "AYI:*", labelFilter: context.HostingEnvironment.EnvironmentName)
				.TrimKeyPrefix("AYI:");
		});
	})
	.ConfigureLogging((_, loggingBuilder) =>
	{
		loggingBuilder.ClearProviders();
		loggingBuilder.AddConsole();
		loggingBuilder.AddDebug();
	})
	.ConfigureServices((context, services) =>
	{
		services.AddSingleton<CliRunner>();

		services.AddSingleton<MigrationRunner>();
		services.AddSqliteConnectionFactory();
	});

var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
await using var scope = scopeFactory.CreateAsyncScope();

var cliRunner = scope.ServiceProvider.GetRequiredService<CliRunner>();

var exitCode = await cliRunner.Run();

return exitCode;
