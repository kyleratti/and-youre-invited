using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Util;

public static class DbSchemaUtil
{
	public static async Task<bool> CheckIfTableExists(IDatabaseConnection<ReadWrite> connection, string tableName)
	{
		const string sql = @"
			SELECT EXISTS (
				SELECT 1
				FROM sqlite_master
				WHERE type = 'table'
				AND name = @tableName)";

		return await connection.ExecuteScalar<bool>(sql, new { tableName});
	}
}
