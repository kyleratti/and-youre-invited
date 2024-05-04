using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_27T16_08_AddEventHostsTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "event_hosts"))
			return;

		await connection.Execute(@"
			create table event_hosts
(
    event_id  TEXT    not null
        constraint FK_event_hosts_scheduled_events_event_id
            references scheduled_events,
    person_id INTEGER not null
        constraint FK_event_hosts_person_id_people_person_id
            references people,
    constraint PK_event_hosts
        primary key (event_id, person_id)
);
");
	}
}
