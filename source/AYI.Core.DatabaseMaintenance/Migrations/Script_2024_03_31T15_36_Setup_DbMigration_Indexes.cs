using Dapper;
using DbAccess.Abstractions;
using Microsoft.Data.Sqlite;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_03_31T15_36_Setup_DbMigration_Indexes : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		var indexExists = await connection.QuerySingle<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'index'
				AND name = 'IX_db_migrations_script_name')");

		if (indexExists)
			return;

		await connection.Execute(@"
			create index IX_db_migrations_script_name
			    on db_migrations (script_name);");
	}
}
