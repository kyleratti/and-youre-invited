using DbAccess.Abstractions;

namespace AYI.Tests.Services.IntegrationTests.Helpers;

public sealed class DisposableContact : IAsyncDisposable
{
	private readonly IDbConnectionFactory _connectionFactory;

	public int ContactId { get; }

	public DisposableContact(IDbConnectionFactory connectionFactory, int contactId)
	{
		_connectionFactory = connectionFactory;
		ContactId = contactId;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await using var conn = await _connectionFactory.CreateConnection();
		await using var tx = await conn.CreateTransaction();

		var allInvites = (await tx.Query<string>("SELECT invitation_id FROM invitations WHERE contact_id = @contactId", new { contactId = ContactId })).ToArray();
		await tx.Execute("DELETE FROM invitation_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitation_auxiliary_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitations WHERE contact_id = @contactId", new { contactId = ContactId });
		await tx.Execute("DELETE FROM event_hosts WHERE contact_id = @contactId", new { contactId = ContactId });
		await tx.Execute("DELETE FROM contacts WHERE contact_id = @contactId", new { contactId = ContactId });

		await tx.Commit(CancellationToken.None);
	}
}
