using AYI.Core.DatabaseMaintenance.Util;
using DbAccess.Abstractions;

namespace AYI.Core.DatabaseMaintenance.Migrations;

public class Script_2024_05_04T13_40_RenameContactsPersonIdToContactId : IDbScript
{
	/// <inheritdoc />
	public async Task Execute(IDatabaseConnection<ReadWrite> connection)
	{
		if (!await DbSchemaUtil.CheckIfColumnExists(connection, "contacts", "person_id"))
			return;

		await connection.Execute(@"create table contacts_dg_tmp
(
    contact_id        INTEGER
        constraint PK_contacts_contact_id
            primary key autoincrement,
    first_name        TEXT not null,
    last_name         TEXT,
    phone_number_e164 TEXT,
    email_address     TEXT,
    constraint UQ_contacts_first_name_last_name
        unique (first_name, last_name)
);

insert into contacts_dg_tmp(contact_id, first_name, last_name, phone_number_e164, email_address)
select person_id, first_name, last_name, phone_number_e164, email_address
from contacts;

drop table contacts;

alter table contacts_dg_tmp
    rename to contacts;
");
	}
}
