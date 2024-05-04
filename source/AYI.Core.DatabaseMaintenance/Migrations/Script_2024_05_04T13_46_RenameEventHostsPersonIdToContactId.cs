using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_05_04T13_46_RenameEventHostsPersonIdToContactId : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (!await DbSchemaUtil.CheckIfColumnExists(connection, "event_hosts", "person_id"))
			return;

		await connection.Execute(@"create table event_hosts_dg_tmp
(
    event_id   TEXT    not null
        constraint FK_event_hosts_scheduled_events_event_id
            references scheduled_events,
    contact_id INTEGER not null
        constraint FK_event_hosts_contact_id_contacts_contact_id
            references contacts,
    constraint PK_event_hosts
        primary key (event_id, contact_id)
);

insert into event_hosts_dg_tmp(event_id, contact_id)
select event_id, person_id
from event_hosts;

drop table event_hosts;

alter table event_hosts_dg_tmp
    rename to event_hosts;
");
	}
}
