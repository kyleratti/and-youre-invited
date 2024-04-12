using AYI.Core.DataModels;
using FruityFoundation.Base.Structures;

namespace AYI.Core.Contracts;

public interface IInvitationService
{
	public Task<IAsyncEnumerable<Invitation>> GetInvitationsForEvent(string eventId, CancellationToken cancellationToken);
}
