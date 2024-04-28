using System.Data.Common;
using AYI.Core.DatabaseMaintenance;
using DataAccess;
using DbAccess.Abstractions;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace AYI.Tests.Services.IntegrationTests.Database;

public sealed class TestDatabaseHelper : IDisposable, IDbConnectionFactory
{
	private readonly DbConnection _masterDbConnection;
	private readonly string _dbName;

	private TestDatabaseHelper(DbConnection masterDbConnection, string dbName)
	{
		_masterDbConnection = masterDbConnection;
		_dbName = dbName;
	}

	public static async Task<TestDatabaseHelper> CreateEmptyDatabase(string dbName)
	{
		var connection = new NonTransactionalDbConnection<ReadWrite>($"Data Source={dbName};Mode=Memory;Cache=Shared");
		await connection.OpenAsync();

		var migrationRunner = new MigrationRunner(A.Fake<ILogger<MigrationRunner>>());
		var result = await migrationRunner.RunMigrations(connection);

		if (result is not ExitStatus.Successful)
			throw new Exception($"Failed to run migrations. Exit status: {result:G} ({result:D})");

		return new TestDatabaseHelper(connection, dbName);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_masterDbConnection.Dispose();
	}

	/// <inheritdoc />
	public ValueTask<INonTransactionalDbConnection<ReadWrite>> CreateConnection()
	{
		var connection = new NonTransactionalDbConnection<ReadWrite>($"Data Source={_dbName};Mode=Memory;Cache=Shared");
		return ValueTask.FromResult((INonTransactionalDbConnection<ReadWrite>)connection);
	}

	/// <inheritdoc />
	public ValueTask<INonTransactionalDbConnection<ReadOnly>> CreateReadOnlyConnection()
	{
		var connection = new NonTransactionalDbConnection<ReadOnly>($"Data Source={_dbName};Mode=Memory;Cache=Shared");
		return ValueTask.FromResult((INonTransactionalDbConnection<ReadOnly>)connection);
	}
}
