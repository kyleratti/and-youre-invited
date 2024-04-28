using AYI.Core.DataModels;

namespace AYI.Core.Contracts;

public interface INotificationService
{
	public Task SendNewRsvpRecordedNotification(string invitationId, InvitationResponseDto response, CancellationToken cancellationToken);
}
