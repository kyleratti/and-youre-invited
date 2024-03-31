namespace AYI.Core.DataAccess.Contracts;

public interface ISqliteConnectionFactory
{
	public ValueTask<DbConnection<ReadWrite>> CreateConnection();
	public ValueTask<DbConnection<ReadOnly>> CreateReadOnlyConnection();
}
