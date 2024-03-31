using System.Data;
using AYI.Core.DataAccess;
using AYI.Core.DataAccess.Contracts;
using AYI.Core.DatabaseMaintenance;
using Microsoft.Extensions.Logging;

namespace AYI.Presentation.DbMaintenanceRunner.Services;

public class CliRunner(
	ILogger<CliRunner> _logger,
	MigrationRunner _migrationRunner,
	ISqliteConnectionFactory sqliteConnectionFactory
)
{
	public async Task<int> Run()
	{
		await using var connection = await sqliteConnectionFactory.CreateConnection();
		// Create a serializable transaction to force a lock on the database
		await using var tx = await connection.CreateTransaction(IsolationLevel.Serializable);

		var exitStatus = await _migrationRunner.RunMigrations(connection);

		await tx.CommitAsync();

		return exitStatus switch
		{
			ExitStatus.Successful => 0,
			ExitStatus.ScriptError => 1,
			ExitStatus.UnknownError => 2,
			_ => throw new NotImplementedException($"Unhandled exit status: {exitStatus:G} ({exitStatus:D})"),
		};
	}
}
