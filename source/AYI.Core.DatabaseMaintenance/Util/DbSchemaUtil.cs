using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Util;

public static class DbSchemaUtil
{
	public static async Task<bool> CheckIfTableExists(IDatabaseConnection<ReadOnly> connection, string tableName)
	{
		const string sql = @"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'table'
				AND name = @tableName)";

		return await connection.ExecuteScalar<bool>(sql, new { tableName});
	}

	public static async Task<bool> CheckIfColumnExists(IDatabaseConnection<ReadOnly> connection, string tableName, string columnName)
	{
		const string sql = @"
			SELECT EXISTS (
				SELECT 1
				FROM pragma_table_info(@tableName)
				WHERE name = @columnName)";

		return await connection.ExecuteScalar<bool>(sql, new { tableName, columnName });
	}
}
