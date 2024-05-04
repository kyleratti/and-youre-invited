using DbAccess.Abstractions;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class DbConnectionFactory(IConfiguration _config) : IDbConnectionFactory
{
	public ValueTask<INonTransactionalDbConnection<ReadWrite>> CreateConnection()
	{
		var connectionString = _config.GetConnectionString("Database");

		if (string.IsNullOrWhiteSpace(connectionString))
			throw new ApplicationException("Connection string not found: Database");

		var connection = new NonTransactionalDbConnection<ReadWrite>(connectionString);
		return ValueTask.FromResult((INonTransactionalDbConnection<ReadWrite>)connection);
	}

	public ValueTask<INonTransactionalDbConnection<ReadOnly>> CreateReadOnlyConnection()
	{
		var connectionString = _config.GetConnectionString("Database_ReadOnly");

		if (string.IsNullOrWhiteSpace(connectionString))
			throw new ApplicationException("Connection string not found: Database_ReadOnly");

		var connection = new NonTransactionalDbConnection<ReadOnly>(connectionString);
		return ValueTask.FromResult((INonTransactionalDbConnection<ReadOnly>)connection);
	}
}
