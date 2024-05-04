using AYI.Core.DataModels;

namespace AYI.Core.Contracts;

public interface IContactService
{
	public Task<IAsyncEnumerable<Contact>> GetContactsById(IReadOnlyCollection<int> contactIds, CancellationToken cancellationToken);
}
