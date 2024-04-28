using DbAccess.Abstractions;

namespace AYI.Tests.Services.IntegrationTests.Helpers;

public sealed class DisposableScheduledEvent : IAsyncDisposable
{
	private readonly IDbConnectionFactory _connectionFactory;
	public string EventId { get; }

	public DisposableScheduledEvent(IDbConnectionFactory connectionFactory, string eventId)
	{
		_connectionFactory = connectionFactory;
		EventId = eventId;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await using var conn = await _connectionFactory.CreateConnection();
		await using var tx = await conn.CreateTransaction();

		/*var allInvites = (await tx.Query<string>("SELECT invitation_id FROM invitations WHERE scheduled_event_id = @eventId", new { eventId = _eventId })).ToArray();
		await tx.Execute("DELETE FROM invitation_auxiliary_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitation_responses WHERE invitation_id IN @inviteIds", new { inviteIds = allInvites });
		await tx.Execute("DELETE FROM invitations WHERE scheduled_event_id = @eventId", new { eventId = _eventId });*/
		await tx.Execute("DELETE FROM event_hosts WHERE event_id = @eventId", new { eventId = EventId });
		await tx.Execute("DELETE FROM scheduled_events WHERE event_id = @eventId", new { eventId = EventId });

		await tx.Commit(CancellationToken.None);
	}
}
