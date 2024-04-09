namespace AYI.Core.DataAccess.Abstractions;

// ReSharper disable once UnusedTypeParameter
public interface IDatabaseConnection<TConnectionType> where TConnectionType : ConnectionType
{
	public Task<IEnumerable<T>> Query<T>(string sql, object? param = null, CancellationToken cancellationToken = default);
	public Task<T> QuerySingle<T>(string sql, object? param = null, CancellationToken cancellationToken = default);
	public Task Execute(string sql, object? param = null, CancellationToken cancellationToken = default);
	public Task<T?> ExecuteScalar<T>(string sql, object? param = null, CancellationToken cancellationToken = default);
}
