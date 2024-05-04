using System.Data;
using AYI.Core.DatabaseMaintenance;
using DbAccess.Abstractions;
using Microsoft.Extensions.Logging;

namespace AYI.Presentation.DbMaintenanceRunner.Services;

public class CliRunner(
	MigrationRunner _migrationRunner
)
{
	public async Task<int> Run()
	{
		var exitStatus = await _migrationRunner.RunMigrations();

		return exitStatus switch
		{
			ExitStatus.Successful => 0,
			ExitStatus.ScriptError => 1,
			ExitStatus.UnknownError => 2,
			_ => throw new NotImplementedException($"Unhandled exit status: {exitStatus:G} ({exitStatus:D})"),
		};
	}
}
