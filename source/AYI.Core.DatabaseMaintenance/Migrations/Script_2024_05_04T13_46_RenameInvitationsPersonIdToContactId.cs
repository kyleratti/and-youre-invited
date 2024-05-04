using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_05_04T13_46_RenameInvitationsPersonIdToContactId : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (!await DbSchemaUtil.CheckIfColumnExists(connection, "invitations", "person_id"))
			return;

		await connection.Execute(@"create table invitations_dg_tmp
(
    invitation_id       TEXT    not null
        constraint PK_invitations_invitation_id
            primary key,
    contact_id          INTEGER not null
        constraint FK_invitations_contact_id_contacts_contact_id
            references contacts,
    can_view_guest_list INTEGER not null,
    created_at          TEXT    not null,
    scheduled_event_id  TEXT    not null
        constraint FK_invitations_scheduled_event_id_scheduled_events_event_id
            references scheduled_events
);

insert into invitations_dg_tmp(invitation_id, contact_id, can_view_guest_list, created_at, scheduled_event_id)
select invitation_id, person_id, can_view_guest_list, created_at, scheduled_event_id
from invitations;

drop table invitations;

alter table invitations_dg_tmp
    rename to invitations;

");
	}
}
