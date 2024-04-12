using AYI.Core.DataModels;
using FruityFoundation.Base.Structures;

namespace AYI.Core.Contracts;

public interface IScheduledEventService
{
	public Task<Maybe<ScheduledEvent>> GetScheduledEventByInviteId(string inviteId, CancellationToken cancellationToken);
	public Task<Maybe<EventInfo>> GetEventInfoByInviteId(string inviteId, CancellationToken cancellationToken);
}
