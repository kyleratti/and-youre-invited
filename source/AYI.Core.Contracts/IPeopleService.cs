using AYI.Core.DataModels;

namespace AYI.Core.Contracts;

public interface IPeopleService
{
	public Task<IAsyncEnumerable<Contact>> GetPeopleById(IReadOnlyCollection<int> personIds, CancellationToken cancellationToken);
}
