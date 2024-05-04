namespace DbAccess.Abstractions;

public interface IDbConnectionFactory
{
	public ValueTask<INonTransactionalDbConnection<ReadWrite>> CreateConnection();
	public ValueTask<INonTransactionalDbConnection<ReadOnly>> CreateReadOnlyConnection();
}
