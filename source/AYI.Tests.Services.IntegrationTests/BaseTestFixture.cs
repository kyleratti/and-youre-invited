using System.Diagnostics.CodeAnalysis;
using AYI.Core.Contracts;
using AYI.Core.DataModels;
using AYI.Core.Services;
using AYI.Tests.Services.IntegrationTests.Database;
using AYI.Tests.Services.IntegrationTests.Helpers;
using DbAccess.Abstractions;
using FruityFoundation.Base.Structures;
using Invitations = AYI.Core.DataModels.Invitations;

namespace AYI.Tests.Services.IntegrationTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public class BaseTestFixture
{
	protected TestDatabaseHelper ConnectionFactory = null!;

	[OneTimeSetUp]
	public async Task SetUp()
	{
		ConnectionFactory = await TestDatabaseHelper.CreateEmptyDatabase("IntegrationTests");
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		ConnectionFactory.Dispose();
	}

	[Test]
	public void NoOpTest() => Assert.Pass();

	protected async Task<DisposableContact> CreateTempContact(string firstName, Maybe<string> lastName = default, Maybe<string> email = default)
	{
		var contactId = await CreateContact(firstName, lastName, email);
		return new DisposableContact(ConnectionFactory, contactId);
	}

	protected async Task<int> CreateContact(string firstName, Maybe<string> lastName, Maybe<string> email)
	{
		await using var conn = await ConnectionFactory.CreateConnection();

		var contactId = await conn.ExecuteScalar<int>(
			"""
			INSERT INTO contacts (first_name, last_name, phone_number_e164, email_address)
			VALUES (@firstName, @lastName, NULL, @email);
			SELECT last_insert_rowid();
			""", new
			{
				firstName,
				lastName = lastName.OrValue(null!),
				email = email.OrValue(null!),
			});

		return contactId;
	}

	protected async Task AddHostToEvent(int contactId, string eventId)
	{
		await using var conn = await ConnectionFactory.CreateConnection();

		await conn.Execute(
			"""
			INSERT INTO event_hosts (event_id, contact_id)
			VALUES (@eventId, @contactId)
			""", new
			{
				eventId, contactId = contactId,
			});
	}

	protected async Task<DisposableScheduledEvent> CreateTempScheduledEvent(
		string eventId,
		Maybe<string> title = default,
		Maybe<DateTimeOffset> startsAt = default,
		Maybe<string> street1 = default,
		Maybe<string> street2 = default,
		Maybe<string> city = default,
		Maybe<string> state = default,
		Maybe<string> zipCode = default,
		Maybe<DateTimeOffset> endsAt = default
	)
	{
		var createdEventId = await CreateScheduledEvent(eventId, title, startsAt, street1, street2, city, state, zipCode);
		return new DisposableScheduledEvent(ConnectionFactory, createdEventId);
	}

	protected async Task<string> CreateScheduledEvent(
		string eventId,
		Maybe<string> title = default,
		Maybe<DateTimeOffset> startsAt = default,
		Maybe<string> street1 = default,
		Maybe<string> street2 = default,
		Maybe<string> city = default,
		Maybe<string> state = default,
		Maybe<string> zipCode = default,
		Maybe<DateTimeOffset> endsAt = default
	)
	{
		await using var conn = await ConnectionFactory.CreateConnection();
		await using var tx = await conn.CreateTransaction();

		var locationId = await CreateLocationImpl(
			tx,
			street1.OrValue("1 Street Lane"),
			street2,
			city.OrValue("My City"),
			state.OrValue("MD"),
			zipCode.OrValue("12345"));
		await CreateScheduledEventImpl(tx, eventId, urlSlug: eventId, locationId, title.OrValue(eventId), startsAt.OrValue(DateTimeOffset.Now), endsAt);

		await tx.Commit(CancellationToken.None);

		return eventId;
	}
	
	protected async Task<DisposableInvite> CreateTempInvite(string inviteId, int contactId, string eventId, Maybe<bool> canViewGuestList = default)
	{
		var createdInviteId = await CreateInvite(inviteId, contactId, eventId, canViewGuestList);
		return new DisposableInvite(ConnectionFactory, createdInviteId);
	}

	protected async Task<string> CreateInvite(string inviteId, int contactId, string eventId, Maybe<bool> canViewGuestList = default)
	{
		await using var conn = await ConnectionFactory.CreateConnection();
		await conn.Execute(
			"""
			INSERT INTO invitations (invitation_id, contact_id, can_view_guest_list, created_at, scheduled_event_id)
			VALUES (@inviteId, @contactId, @canViewGuestList, CURRENT_TIMESTAMP, @eventId)
			""", new
			{
				inviteId,
				contactId,
				canViewGuestList = canViewGuestList.OrValue(false),
				eventId,
			});

		return inviteId;
	}

	protected async Task SetRsvp(string inviteId, InvitationResponseDto response, Maybe<Invitations.AuxiliaryRsvpData> auxiliaryData = default)
	{
		IScheduledEventService scheduledEventService = new ScheduledEvents.ScheduledEventService(ConnectionFactory);
		await scheduledEventService.RecordRsvp(inviteId, response, auxiliaryData, CancellationToken.None);
	}

	private static async Task<int> CreateLocationImpl(IDatabaseConnection<ReadWrite> db, string street1, Maybe<string> street2, string city, string state, string zipCode)
	{
		var locationId = await db.ExecuteScalar<int>(
			"""
			INSERT INTO locations (street_1, street_2, city, state, zip_code)
			VALUES (@street1, @street2, @city, @state, @zipCode);
			SELECT last_insert_rowid();
			""", new
			{
				street1,
				street2 = street2.OrValue(null!),
				city,
				state,
				zipCode,
			});

		return locationId;
	}

	private static async Task CreateScheduledEventImpl(
		IDatabaseConnection<ReadWrite> db,
		string eventId,
		string urlSlug,
		int locationId,
		string title,
		DateTimeOffset startsAt,
		Maybe<DateTimeOffset> endsAt
	)
	{
		await db.Execute(
			"""
			INSERT INTO scheduled_events (event_id, url_slug, location_id, title, starts_at, ends_at)
			VALUES (
				@eventId
				,@urlSlug
				,@locationId
				,@title
				,@startsAt
				,@endsAt);
			""", new
			{
				eventId,
				urlSlug,
				locationId,
				title,
				startsAt,
				endsAt = endsAt.ToNullable(),
			});
	}
}
