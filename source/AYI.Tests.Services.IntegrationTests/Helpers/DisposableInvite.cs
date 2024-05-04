using DbAccess.Abstractions;

namespace AYI.Tests.Services.IntegrationTests.Helpers;

public sealed class DisposableInvite : IAsyncDisposable
{
	private readonly IDbConnectionFactory _connectionFactory;

	public string InviteId { get; }

	public DisposableInvite(IDbConnectionFactory connectionFactory, string inviteId)
	{
		_connectionFactory = connectionFactory;
		InviteId = inviteId;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await using var conn = await _connectionFactory.CreateConnection();
		await using var tx = await conn.CreateTransaction();

		await tx.Execute("DELETE FROM invitation_auxiliary_responses WHERE invitation_id = @inviteId", new { inviteId = InviteId });
		await tx.Execute("DELETE FROM invitation_responses WHERE invitation_id = @inviteId", new { inviteId = InviteId });
		await tx.Execute("DELETE FROM invitations WHERE invitation_id = @inviteId", new { inviteId = InviteId });

		await tx.Commit(CancellationToken.None);
	}
}
