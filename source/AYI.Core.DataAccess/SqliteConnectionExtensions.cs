using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace AYI.Core.DataAccess;

public static class SqliteConnectionExtensions
{
	public static async Task<SqliteTransaction> CreateTransaction(this SqliteConnection connection, IsolationLevel isolationLevel)
	{
		await connection.OpenAsync();
		return connection.BeginTransaction(isolationLevel);
	}
}
