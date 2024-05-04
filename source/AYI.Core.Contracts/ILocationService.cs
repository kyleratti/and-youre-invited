using AYI.Core.DataModels;
using FruityFoundation.Base.Structures;

namespace AYI.Core.Contracts;

public interface ILocationService
{
	public Task<Maybe<Location>> GetLocationById(int locationId, CancellationToken cancellationToken);
}
