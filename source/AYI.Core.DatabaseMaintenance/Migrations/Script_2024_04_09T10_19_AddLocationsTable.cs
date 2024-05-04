using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T10_19_AddLocationsTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "locations"))
			return;

		await connection.Execute("""
			create table locations
			(
				location_id INTEGER
					constraint PK_locations_location_id
				primary key autoincrement,
				street_1	TEXT not null,
				street_2	TEXT,
				city		TEXT not null,
				state	   TEXT not null,
				zip_code	TEXT not null
				);
		""");
	}
}
