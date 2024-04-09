namespace AYI.Core.DataAccess.Abstractions;

public interface IDatabaseTransactionConnection<TConnectionType> : IDatabaseConnection<TConnectionType>, IDisposable, IAsyncDisposable
	where TConnectionType : ConnectionType
{
	public Task Commit(CancellationToken cancellationToken);
}
