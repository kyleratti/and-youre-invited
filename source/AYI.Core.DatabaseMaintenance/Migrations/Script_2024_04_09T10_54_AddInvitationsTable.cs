using AYI.Core.DataAccess.Abstractions;
using AYI.Core.DatabaseMaintenance.Util;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T10_54_AddInvitationsTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "invitations"))
			return;

		await connection.Execute("""
			create table invitations
		(
		    invitation_id       TEXT    not null
		        constraint PK_invitations_invitation_id
		            primary key,
		    person_id           INTEGER not null
		        constraint FK_invitations_person_id_people_person_id
		            references people,
		    can_view_guest_list INTEGER not null,
		    created_at          TEXT    not null
		);
		""");
	}
}
