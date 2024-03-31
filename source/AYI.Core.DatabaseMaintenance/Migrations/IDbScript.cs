using Microsoft.Data.Sqlite;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public interface IDbScript
{
	public Task Execute(SqliteConnection connection);
}
