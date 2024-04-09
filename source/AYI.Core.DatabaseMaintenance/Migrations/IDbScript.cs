using AYI.Core.DataAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public interface IDbScript
{
	public Task Execute(IDatabaseConnection<ReadWrite> connection);
}
