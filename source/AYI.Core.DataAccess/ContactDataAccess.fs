module AYI.Core.DataAccess.ContactDataAccess

open System.Collections.Generic
open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FSharp.Control
open FruityFoundation.Db.Db

let findContactsById  (db : IDatabaseConnection<ReadOnly>)
                    (cancellationToken : CancellationToken)
                    (contactIds : IReadOnlyCollection<int>) =
    (@"SELECT
            c.contact_id
            ,c.first_name
            ,c.last_name
            ,c.phone_number_e164
            ,c.email_address
        FROM contacts c WHERE c.contact_id IN @contactIds",
        [|"@contactIds", box contactIds|])
    |> executeReader db cancellationToken
    |> mapReader cancellationToken (fun reader ->
        {
            ContactId = reader |> getInt32 0
            FirstName = reader |> getString 1
            LastName = reader |> tryGetString 2
            PhoneNumberE164 = reader |> tryGetString 3
            EmailAddress = reader |> tryGetString 4
        } : Contact)

let findByInviteId  (db : IDatabaseConnection<ReadOnly>)
                    (cancellationToken : CancellationToken)
                    (inviteId : string) = task {
    return! (@"SELECT
                c.contact_id
                ,c.first_name
                ,c.last_name
                ,c.phone_number_e164
                ,c.email_address
            FROM contacts c
            INNER JOIN invitations i ON i.invitation_id = @inviteId AND i.contact_id = c.contact_id",
            [|"@inviteId", box inviteId|])
            |> executeReader db cancellationToken
            |> mapReader cancellationToken (fun reader ->
                {
                    ContactId = reader |> getInt32 0
                    FirstName = reader |> getString 1
                    LastName = reader |> tryGetString 2
                    PhoneNumberE164 = reader |> tryGetString 3
                    EmailAddress = reader |> tryGetString 4
                } : Contact)
            |> TaskSeq.tryHead
}
