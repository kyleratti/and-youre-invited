namespace AYI.Core.Contracts;

public abstract record MessageType
{
	public T Merge<T>(Func<PlainText, T> onPlainText, Func<Html, T> onHtml) =>
		this switch
		{
			PlainText plainText => onPlainText(plainText),
			Html html => onHtml(html),
			_ => throw new InvalidOperationException(GetType().FullName),
		};

	public record PlainText(string Text) : MessageType;

	public record Html(string HtmlValue) : MessageType;
}

public interface IEmailSender
{
	public Task SendMessage(string recipient, string subject, MessageType message, CancellationToken cancellationToken);
}
