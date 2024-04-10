using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T10_37_AddScheduledEventsTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "scheduled_events"))
			return;

		await connection.Execute("""
			create table scheduled_events
		(
		    event_id    TEXT    not null
		        constraint PK_scheduled_events_event_id
		            primary key,
		    url_slug    TEXT    not null
		        constraint UQ_scheduled_events_url_slug
		            unique,
		    location_id INTEGER not null
		        constraint FK_scheduled_events_location_id_locations_location_id
		            references locations,
		    title       TEXT    not null,
		    starts_at   TEXT    not null,
		    ends_at     TEXT
		);
		""");
	}
}
