using AYI.Core.DataModels;

namespace AYI.Presentation.WebApp.Events;

public class NewRsvpRecordedEventArgs(string invitationId, InvitationResponseDto response) : EventArgs
{
	public string InvitationId { get; } = invitationId;
	public InvitationResponseDto Response { get; } = response;
}
