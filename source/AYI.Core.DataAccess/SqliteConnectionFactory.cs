using AYI.Core.DataAccess.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace AYI.Core.DataAccess;

public class SqliteConnectionFactory(IConfiguration _config) : ISqliteConnectionFactory
{
	public ValueTask<DbConnection<ReadWrite>> CreateConnection()
	{
		var connectionString = _config.GetConnectionString("Database");

		if (string.IsNullOrWhiteSpace(connectionString))
			throw new ApplicationException("Connection string not found: Database");

		var connection = new DbConnection<ReadWrite>(connectionString);
		return ValueTask.FromResult(connection);
	}

	public ValueTask<DbConnection<ReadOnly>> CreateReadOnlyConnection()
	{
		var connectionString = _config.GetConnectionString("Database_ReadOnly");

		if (string.IsNullOrWhiteSpace(connectionString))
			throw new ApplicationException("Connection string not found: Database_ReadOnly");

		var connection = new DbConnection<ReadOnly>(connectionString);
		return ValueTask.FromResult(connection);
	}
}
