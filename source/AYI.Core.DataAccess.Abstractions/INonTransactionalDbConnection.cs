using System.Data;

namespace AYI.Core.DataAccess.Abstractions;

public interface INonTransactionalDbConnection<TConnectionType> : IDatabaseConnection<TConnectionType>, IDisposable, IAsyncDisposable
	where TConnectionType : ConnectionType
{
	public Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction();
	public Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction(IsolationLevel isolationLevel);
	public Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction(IsolationLevel isolationLevel, bool deferred);
}
