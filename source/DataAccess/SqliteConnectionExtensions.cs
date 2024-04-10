using System.Data;
using Microsoft.Data.Sqlite;

namespace DataAccess;

public static class SqliteConnectionExtensions
{
	public static async Task<SqliteTransaction> CreateTransaction(this SqliteConnection connection, IsolationLevel isolationLevel)
	{
		await connection.OpenAsync();
		return connection.BeginTransaction(isolationLevel);
	}
}
