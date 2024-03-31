using System.Reflection;
using AYI.Core.DataAccess;
using AYI.Core.DatabaseMaintenance.Migrations;
using Dapper;
using Microsoft.Extensions.Logging;

namespace AYI.Core.DatabaseMaintenance;

public class MigrationRunner(
	ILogger<MigrationRunner> _logger
)
{
	public async Task<ExitStatus> RunMigrations(DbConnection<ReadWrite> connection)
	{
		var migrationTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where(t => t.IsAssignableTo(typeof(IDbScript)))
			.Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
			.OrderBy(t => t.Name)
			.ToArray();

		foreach (var migrationType in migrationTypes)
		{
			try
			{
				var isDatabaseInitialized = await IsDatabaseInitialized(connection);

				if (isDatabaseInitialized)
				{
					var hasRunSuccessfully = await HasScriptRunSuccessfully(connection, migrationType.Name);

					if (hasRunSuccessfully)
						continue;
				}

				var migration = (IDbScript)Activator.CreateInstance(migrationType)!;

				var exitStatus = await RunScriptWithErrorHandling(connection, migration);

				if (exitStatus is not ExitStatus.Successful)
					return exitStatus;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error running migration {MigrationName}", migrationType.Name);
				return ExitStatus.UnknownError;
			}
		}

		return ExitStatus.Successful;
	}

	private async Task<bool> IsDatabaseInitialized(DbConnection<ReadWrite> connection) =>
		await connection.QuerySingleAsync<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'table'
				AND name = 'db_migrations'
			)");

	private async Task<bool> HasScriptRunSuccessfully(DbConnection<ReadWrite> connection, string scriptName) =>
		await connection.QuerySingleAsync<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM db_migrations
				WHERE script_name = @scriptName
				AND is_success = 1
			)", new { scriptName });

	private async Task<ExitStatus> RunScriptWithErrorHandling(DbConnection<ReadWrite> connection, IDbScript script)
	{
		var scriptName = script.GetType().Name;

		try
		{
			await script.Execute(connection);
			await SetScriptAsSuccessful(connection, scriptName);
			return ExitStatus.Successful;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error running migration {MigrationName}", scriptName);
			await SetScriptAsFailed(connection, scriptName, ex);
			return ExitStatus.ScriptError;
		}
	}

	private static async Task SetScriptAsSuccessful(DbConnection<ReadWrite> connection, string scriptName)
	{
		await connection.ExecuteAsync("""
			INSERT INTO db_migrations (script_name, is_success, ran_at)
			VALUES (@scriptName, 1, CURRENT_TIMESTAMP)
			""", new { scriptName });
	}

	private static async Task SetScriptAsFailed(DbConnection<ReadWrite> connection, string scriptName, Exception ex)
	{
		await connection.ExecuteAsync("""
			INSERT INTO db_migrations (script_name, is_success, ran_at, error_message, error_stack_trace)
			VALUES (@scriptName, 0, CURRENT_TIMESTAMP, @errorMessage, @errorStackTrace)
			""", new
		{
			scriptName,
			errorMessage = ex.Message,
			errorStackTrace = ex.StackTrace
		});
	}
}
