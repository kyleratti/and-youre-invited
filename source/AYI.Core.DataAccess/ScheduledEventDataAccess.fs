module AYI.Core.DataAccess.ScheduledEventDataAccess

open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FSharp.Control
open FruityFoundation.Db.Db

let findEventByInviteId (connection : IDatabaseConnection<ReadOnly>) (cancellationToken : CancellationToken) (inviteId : string) = task {
    use! reader =
        (
            "SELECT
                se.event_id
                ,se.url_slug
                ,se.location_id
                ,se.title
                ,se.starts_at
                ,se.ends_at
            FROM scheduled_events se
            WHERE se.event_id = (
                SELECT i.scheduled_event_id FROM invitations i WHERE i.invitation_id = @inviteId
            )",
            [|("inviteId", inviteId :> obj)|]
        )
        |> executeReader connection cancellationToken

    return reader
            |> mapReaderSync cancellationToken (fun reader ->
                {
                    EventId = reader |> getString 0
                    UrlSlug = reader |> getString 1
                    LocationId = reader |> getInt32 2
                    Title = reader |> getString 3
                    StartsAt = reader |> getDateTimeOffset 4
                    EndsAt = reader |> tryGetDateTimeOffset 5
                } : ScheduledEvent)
            |> Seq.tryHead
            //|> TaskSeq.tryHead
}

type EventHost = {
    PersonId : int
    EventId : string
    EmailAddress : string option
}

let findHostsByEventId  (db : IDatabaseConnection<ReadOnly>)
                        (cancellationToken : CancellationToken)
                        (eventId : string) =
    (@"SELECT
                eh.event_id
                ,p.person_id
                ,p.email_address
             FROM event_hosts eh
             INNER JOIN people p ON p.person_id = eh.person_id
             WHERE
                eh.event_id = @eventId",
             [|("eventId", box eventId)|])
        |> executeReader db cancellationToken
        |> mapReader cancellationToken (fun reader ->
            {
                EventId = reader |> getString 0
                PersonId = reader |> getInt32 1
                EmailAddress = reader |> tryGetString 2
            } : EventHost)
