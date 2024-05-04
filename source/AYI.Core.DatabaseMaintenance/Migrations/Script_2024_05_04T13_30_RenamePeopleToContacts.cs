using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_05_04T13_30_RenamePeopleToContacts : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (!await DbSchemaUtil.CheckIfTableExists(connection, "people"))
			return;

		await connection.Execute(@"alter table people rename to contacts;");
	}
}
