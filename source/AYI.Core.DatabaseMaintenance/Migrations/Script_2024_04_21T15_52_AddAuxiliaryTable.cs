using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_21T15_52_AddAuxiliaryTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "invitation_auxiliary_responses"))
			return;

		await connection.Execute(@"
			create table invitation_auxiliary_responses
(
    invitation_id TEXT
        constraint FK_invitation_responses_invitation_id_invitations_invitation_id
            references invitations,
    json_blob     TEXT not null,
    constraint PK_invitation_auxiliary_responses_invitation_id
        primary key (invitation_id)
);

");
	}
}
