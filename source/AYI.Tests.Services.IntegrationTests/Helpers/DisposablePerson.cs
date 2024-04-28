using DbAccess.Abstractions;

namespace AYI.Tests.Services.IntegrationTests.Helpers;

public sealed class DisposablePerson : IAsyncDisposable
{
	private readonly IDbConnectionFactory _connectionFactory;

	public int PersonId { get; }

	public DisposablePerson(IDbConnectionFactory connectionFactory, int personId)
	{
		_connectionFactory = connectionFactory;
		PersonId = personId;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await using var conn = await _connectionFactory.CreateConnection();
		await using var tx = await conn.CreateTransaction();

		var allInvites = (await tx.Query<string>("SELECT invitation_id FROM invitations WHERE person_id = @personId", new { personId = PersonId })).ToArray();
		await tx.Execute("DELETE FROM invitation_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitation_auxiliary_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitations WHERE person_id = @personId", new { personId = PersonId });
		await tx.Execute("DELETE FROM event_hosts WHERE person_id = @personId", new { personId = PersonId });
		await tx.Execute("DELETE FROM people WHERE person_id = @personId", new { personId = PersonId });

		await tx.Commit(CancellationToken.None);
	}
}
