using AYI.Core.Contracts;
using AYI.Core.Contracts.Options;
using AYI.Core.DataModels;
using AYI.Core.Services;
using FakeItEasy;
using FruityFoundation.Base.Structures;
using FruityFoundation.FsBase;
using Microsoft.Extensions.Options;
using Invitations = AYI.Core.DataModels.Invitations;
using Task = System.Threading.Tasks.Task;

namespace AYI.Tests.Services.IntegrationTests;

public class NotificationServiceTests : BaseTestFixture
{
	private IInvitationService _fakeInvitationService = null!;
	private IEmailSender _fakeEmailSender = null!;

	private INotificationService _notificationService = null!;

	[SetUp]
	public new void SetUp()
	{
		_fakeInvitationService = A.Fake<IInvitationService>();
		_fakeEmailSender = A.Fake<IEmailSender>();

		_notificationService = new Notifications.NotificationService(
			ConnectionFactory,
			_fakeInvitationService,
			_fakeEmailSender);
	}

	[Test]
	public async Task SendNewRsvpRecordedNotification_SendsNotification_ToHosts()
	{
		// Arrange
		await using var evnt = await CreateTempScheduledEvent(eventId: nameof(SendNewRsvpRecordedNotification_SendsNotification_ToHosts));
		await using var host = await CreateTempContact("Host", "McHost", "host@party.party");
		await using var person = await CreateTempContact("I'm", "Invited", "invited@invite.net");
		await AddHostToEvent(host.ContactId, evnt.EventId);
		var inviteId = await CreateInvite(inviteId: "my-invite", person.ContactId, evnt.EventId);
		var auxData = new Invitations.SpringHasSprungAuxiliaryData("cats", "bananas");
		await SetRsvp(inviteId, InvitationResponseDto.Attending);
		A.CallTo(() => _fakeInvitationService.GetAuxiliaryData(inviteId, A<CancellationToken>._))
			.Returns(Invitations.AuxiliaryRsvpData.NewSpringHasSprung(auxData));

		// Act
		Assert.DoesNotThrowAsync(async () =>
			await _notificationService.SendNewRsvpRecordedNotification(
				inviteId,
				InvitationResponseDto.Attending,
				CancellationToken.None));

		// Assert
		var expectedBody = new MessageType.PlainText(
			"""
			Yay! I'm Invited is attending!

			Food Allergies: cats
			Food Being Brought: bananas

			==== Attending ====
			- I'm Invited

			==== Not Attending ====
			(none)

			==== No Response ====
			(none)
			""");
		A.CallTo(() => _fakeEmailSender.SendMessage(
				"host@party.party",
				"RSVP: I'm Invited is attending",
				A<MessageType>.That.IsEqualTo(expectedBody),
				A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
	}

	[Test]
	public async Task SendNewRsvpRecordedNotification_SendsNotification_IncludingAuxData_For_SpringHasSprung_ToHosts()
	{
		// Arrange
		var inviteId = Guid.NewGuid().ToString();
		await using var evnt = await CreateTempScheduledEvent(eventId: nameof(SendNewRsvpRecordedNotification_SendsNotification_IncludingAuxData_For_SpringHasSprung_ToHosts));
		await using var host = await CreateTempContact("Host", "McHost", "host@party.party");
		await using var person = await CreateTempContact("I'm", "Invited", "invited@invite.net");
		await using var person2 = await CreateTempContact("Not Responding");
		await AddHostToEvent(host.ContactId, evnt.EventId);
		await using var invite = await CreateTempInvite(inviteId, person.ContactId, evnt.EventId);
		await using var _  = await CreateTempInvite(inviteId: "another-invite", person2.ContactId, evnt.EventId);
		await SetRsvp(invite.InviteId, InvitationResponseDto.NotAttending);

		// Act
		Assert.DoesNotThrowAsync(async () =>
			await _notificationService.SendNewRsvpRecordedNotification(
				invite.InviteId,
				InvitationResponseDto.NotAttending,
				CancellationToken.None));

		// Assert
		var expectedBody = new MessageType.PlainText(
			"""
			Womp. I'm Invited can't make it.

			==== Attending ====
			(none)

			==== Not Attending ====
			- I'm Invited

			==== No Response ====
			- Not Responding
			""");
		A.CallTo(() => _fakeEmailSender.SendMessage(
				"host@party.party",
				"RSVP: I'm Invited is not attending",
				A<MessageType>.That.IsEqualTo(expectedBody),
				A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
	}
}
