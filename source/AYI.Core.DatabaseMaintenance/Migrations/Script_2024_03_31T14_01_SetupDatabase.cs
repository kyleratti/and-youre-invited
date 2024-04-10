using Dapper;
using DbAccess.Abstractions;
using Microsoft.Data.Sqlite;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_03_31T14_01_SetupDatabase : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		var migrationsTableExists = await connection.QuerySingle<bool>(@"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'table'
				AND name = 'db_migrations')");

		if (migrationsTableExists)
			return;

		await connection.Execute(@"
			create table db_migrations
			(
			    run_id            INTEGER
			        constraint PK_db_migrations_run_id
			            primary key autoincrement,
			    script_name       TEXT    not null,
			    is_success        INTEGER not null,
			    ran_at            TEXT    not null,
			    error_message     TEXT,
			    error_stack_trace TEXT
			);");
	}
}
