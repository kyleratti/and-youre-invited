namespace AYI.Core.DataAccess.Abstractions;

public interface IDbConnectionFactory
{
	public ValueTask<INonTransactionalDbConnection<ReadWrite>> CreateConnection();
	public ValueTask<INonTransactionalDbConnection<ReadOnly>> CreateReadOnlyConnection();
}
