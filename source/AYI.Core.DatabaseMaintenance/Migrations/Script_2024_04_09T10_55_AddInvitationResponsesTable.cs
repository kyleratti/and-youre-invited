using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T10_55_AddInvitationResponsesTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "invitation_responses"))
			return;

		await connection.Execute("""
			create table invitation_responses
		(
		    invitation_id TEXT    not null
		        constraint PK_invitation_responses_invitation_id
		            primary key,
		    response      INTEGER not null,
		    created_at    TEXT    not null
		);
		""");
	}
}
