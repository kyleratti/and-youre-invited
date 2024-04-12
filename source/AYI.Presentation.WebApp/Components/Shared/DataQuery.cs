namespace AYI.Presentation.WebApp.Components.Shared;

public abstract record DataQuery<TValue>
{
	public static readonly DataQuery<TValue> Loading = new Pending();
	public static DataQuery<TValue> NewSuccess(TValue data) => new Success(data);
	public static DataQuery<TValue> NewError(string message) => new Error(message);

	public record Pending : DataQuery<TValue>;
	public record Success(TValue Data) : DataQuery<TValue>;

	public record Error(string Message) : DataQuery<TValue>;
}
