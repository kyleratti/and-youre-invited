using System.Data;
using System.Reflection;
using AYI.Core.DatabaseMaintenance.Migrations;
using DbAccess.Abstractions;
using Microsoft.Extensions.Logging;

namespace AYI.Core.DatabaseMaintenance;

public class MigrationRunner(
	ILogger<MigrationRunner> _logger,
	IDbConnectionFactory _dbConnectionFactory
)
{
	public async Task<ExitStatus> RunMigrations()
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
				if (await IsDatabaseInitialized())
				{
					var hasRunSuccessfully = await HasScriptRunSuccessfully(migrationType.Name);

					if (hasRunSuccessfully)
						continue;
				}

				var migration = (IDbScript)Activator.CreateInstance(migrationType)!;

				await using var connection = await _dbConnectionFactory.CreateConnection();

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

	private async Task<bool> IsDatabaseInitialized()
	{
		await using var connection = await _dbConnectionFactory.CreateReadOnlyConnection();

		return await connection.QuerySingle<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'table'
				AND name = 'db_migrations'
			)");
	}

	private async Task<bool> HasScriptRunSuccessfully(string scriptName)
	{
		await using var connection = await _dbConnectionFactory.CreateReadOnlyConnection();

		return await connection.QuerySingle<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM db_migrations
				WHERE script_name = @scriptName
				AND is_success = 1
			)", new { scriptName });
	}

	private async Task<ExitStatus> RunScriptWithErrorHandling(INonTransactionalDbConnection<ReadWrite> connection, IDbScript script)
	{
		var scriptName = script.GetType().Name;

		try
		{
			if (script is INonTransactionalDbScript)
				await RunScriptImpl(connection, script, scriptName);
			else
				await WithTransaction(connection, tx => RunScriptImpl(tx, script, scriptName));

			return ExitStatus.Successful;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error running migration {MigrationName}", scriptName);
			await SetScriptAsFailed(connection, scriptName, ex);
			return ExitStatus.ScriptError;
		}

		static async Task RunScriptImpl(IDatabaseConnection<ReadWrite> connection, IDbScript script, string scriptName)
		{
			await script.Execute(connection);
			await SetScriptAsSuccessful(connection, scriptName);
		}
	}

	private static async Task WithTransaction(INonTransactionalDbConnection<ReadWrite> connection, Func<IDatabaseTransactionConnection<ReadWrite>, Task> action)
	{
		await using var transaction = await connection.CreateTransaction();

		await action(transaction);

		await transaction.Commit(CancellationToken.None);
	}

	private static async Task SetScriptAsSuccessful(IDatabaseConnection<ReadWrite> connection, string scriptName)
	{
		await connection.Execute("""
			INSERT INTO db_migrations (script_name, is_success, ran_at)
			VALUES (@scriptName, 1, CURRENT_TIMESTAMP)
			""", new { scriptName });
	}

	private static async Task SetScriptAsFailed(IDatabaseConnection<ReadWrite> connection, string scriptName, Exception ex)
	{
		await connection.Execute("""
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
