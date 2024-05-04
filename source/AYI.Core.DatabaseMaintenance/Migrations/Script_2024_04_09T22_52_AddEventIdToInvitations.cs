using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T22_52_AddEventIdToInvitations : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfColumnExists(connection, "invitations", "scheduled_event_id"))
			return;

		await connection.Execute(@"
			create table invitations_dg_tmp
(
    invitation_id       TEXT    not null
        constraint PK_invitations_invitation_id
            primary key,
    person_id           INTEGER not null
        constraint FK_invitations_person_id_people_person_id
            references people,
    can_view_guest_list INTEGER not null,
    created_at          TEXT    not null,
    scheduled_event_id  TEXT    not null
        constraint FK_invitations_scheduled_event_id_scheduled_events_event_id
            references scheduled_events
);

drop table invitations;

alter table invitations_dg_tmp
    rename to invitations;
    	");
	}
}
