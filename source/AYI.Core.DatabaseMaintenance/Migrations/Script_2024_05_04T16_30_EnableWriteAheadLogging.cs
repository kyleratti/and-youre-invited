using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_05_04T16_30_EnableWriteAheadLogging : INonTransactionalDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		await connection.Execute("PRAGMA journal_mode=WAL;");
	}
}
