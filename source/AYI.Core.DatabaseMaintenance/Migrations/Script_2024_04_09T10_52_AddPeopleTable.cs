using AYI.Core.DataAccess.Abstractions;
using AYI.Core.DatabaseMaintenance.Util;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_04_09T10_52_AddPeopleTable : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (await DbSchemaUtil.CheckIfTableExists(connection, "people"))
			return;

		await connection.Execute("""
			create table people
		(
		    person_id         INTEGER
		        constraint PK_people_person_id
		            primary key autoincrement,
		    first_name        TEXT not null,
		    last_name         TEXT,
		    phone_number_e164 TEXT,
		    email_address     TEXT,
		    constraint UQ_people_first_name_last_name
		        unique (first_name, last_name)
		);
		""");
	}
}
