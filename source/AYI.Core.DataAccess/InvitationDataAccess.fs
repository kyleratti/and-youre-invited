module AYI.Core.DataAccess.InvitationDataAccess

open System
open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FruityFoundation.Db.Db

let findInvitationsByEventId (db : IDatabaseConnection<ReadOnly>)
                            (cancellationToken : CancellationToken)
                            (eventId : string) =
    let parseInvitationResponse (responseType : byte option) (respondedAt : DateTimeOffset option) =
        match responseType, respondedAt with
        | Some respType, Some respAt when respType = 1uy -> Some (InvitationResponse.Attending respAt)
        | Some respType, Some respAt when respType = 2uy -> Some (InvitationResponse.NotAttending respAt)
        | None, None -> None
        | _ -> failwithf "Invalid invitation response data (ResponseTye=%A, RespondedAt=%A)" responseType respondedAt

    (@"SELECT
            i.invitation_id
            ,i.scheduled_event_id
            ,i.person_id
            ,i.can_view_guest_list
            ,i.created_at
            ,ir.response
            ,ir.created_at
        FROM invitations i
        LEFT JOIN invitation_responses ir ON ir.invitation_id = i.invitation_id
        WHERE i.scheduled_event_id = @eventId",
        [|"@eventId", box eventId|])
    |> executeReader db cancellationToken
    |> mapReader cancellationToken (fun reader ->
        {
            InvitationId = reader |> getString 0
            PersonId = reader |> getInt32 2
            CanViewGuestList = reader |> getBool 3
            CreatedAt = reader |> getDateTimeOffset 4
            Response = parseInvitationResponse (reader |> tryGetByte 5) (reader |> tryGetDateTimeOffset 6)
        } : Invitation)
