using AYI.Core.DataModels;
using FruityFoundation.Base.Structures;

namespace AYI.Core.Contracts;

public interface IInvitationService
{
	public Task<Maybe<Invitations.AuxiliaryRsvpData>> GetAuxiliaryData(string inviteId, CancellationToken cancellationToken);
}
