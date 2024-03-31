using Microsoft.Data.Sqlite;

namespace AYI.Core.DataAccess;

// ReSharper disable once UnusedTypeParameter
public class DbConnection<TConnectionType> : SqliteConnection where TConnectionType : ConnectionType
{
	/// <summary>
	/// C'tor
	/// </summary>
	public DbConnection(string connectionString) : base(connectionString)
	{
	}
}
