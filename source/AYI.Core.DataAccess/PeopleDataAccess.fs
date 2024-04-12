module AYI.Core.DataAccess.PeopleDataAccess

open System.Collections.Generic
open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FruityFoundation.Db.Db

let findPeopleById  (db : IDatabaseConnection<ReadOnly>)
                    (cancellationToken : CancellationToken)
                    (personIds : IReadOnlyCollection<int>) =
    (@"SELECT
            p.person_id
            ,p.first_name
            ,p.last_name
            ,p.phone_number_e164
            ,p.email_address
        FROM people p WHERE p.person_id IN @personIds",
        [|"@personIds", box personIds|])
    |> executeReader db cancellationToken
    |> mapReader cancellationToken (fun reader ->
        {
            PersonId = reader |> getInt32 0
            FirstName = reader |> getString 1
            LastName = reader |> tryGetString 2
            PhoneNumberE164 = reader |> tryGetString 3
            EmailAddress = reader |> tryGetString 4
        } : Person)
