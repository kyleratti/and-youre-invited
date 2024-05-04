module AYI.Core.DataAccess.InvitationDataAccess

open System
open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FSharp.Control
open FruityFoundation.Db.Db

let private parseInvitationResponse (responseType : byte option)
                                    (respondedAt : DateTimeOffset option) =
    match responseType, respondedAt with
    | Some respType, Some respAt when respType = (byte InvitationResponseDto.Attending) ->
        Some (InvitationResponseDto.Attending, respAt)
    | Some respType, Some respAt when respType = (byte InvitationResponseDto.NotAttending) ->
        Some (InvitationResponseDto.NotAttending, respAt)
    | None, None -> None
    | _ -> failwithf "Invalid invitation response data (ResponseTye=%A, RespondedAt=%A)" responseType respondedAt

[<RequireQualifiedAccess>]
type InvitationResponse =
    | Attending of respondedAt : DateTimeOffset
    | NotAttending of respondedAt : DateTimeOffset

let private parseInviteResponse (responseType : byte option)
                                (respondedAt : DateTimeOffset option) =
    match responseType, respondedAt with
    | Some respType, Some respAt when respType = (byte InvitationResponseDto.Attending) ->
        (InvitationResponse.Attending respAt)
        |> Some
    | Some respType, Some respAt when respType = (byte InvitationResponseDto.NotAttending) ->
        (InvitationResponse.NotAttending respAt)
        |> Some
    | None, None -> None
    | _ -> failwithf "Invalid invitation response data (ResponseTye=%A, RespondedAt=%A)" responseType respondedAt

let findInvitationsByEventId (db : IDatabaseConnection<ReadOnly>)
                            (cancellationToken : CancellationToken)
                            (eventId : string) =

    (@"SELECT
            i.invitation_id
            ,i.scheduled_event_id
            ,i.contact_id
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
            ContactId = reader |> getInt32 2
            CanViewGuestList = reader |> getBool 3
            CreatedAt = reader |> getDateTimeOffset 4
            Response = parseInvitationResponse (reader |> tryGetByte 5) (reader |> tryGetDateTimeOffset 6)
        } : InvitationDto)

let addInvitationResponse (db : IDatabaseConnection<ReadWrite>)
                          (cancellationToken : CancellationToken)
                          (invitationId : string)
                          (response : InvitationResponseDto) = task {
    return! (@"INSERT INTO invitation_responses (invitation_id, response, created_at)
        VALUES (@invitationId, @response, @createdAt)",
        [|"@invitationId", box invitationId
         ;"@response", box (byte response)
         ;"@createdAt", box DateTimeOffset.UtcNow|])
    |> execute db cancellationToken
}

let private asNullable = function
    | None -> null
    | Some x -> box x

let addAuxiliaryResponse (db : IDatabaseConnection<ReadWrite>)
                         (cancellationToken : CancellationToken)
                         (invitationId : string)
                         (jsonBlob : string option) = task {
    if Option.isNone jsonBlob then
        return! (@"DELETE FROM invitation_auxiliary_responses WHERE invitation_id = @invitationId",
                 [|"@invitationId", box invitationId|])
                |> execute db cancellationToken
    else
        return! (@"INSERT INTO invitation_auxiliary_responses (invitation_id, json_blob)
            VALUES (@invitationId, @jsonBlob)
            ON CONFLICT(invitation_id) DO UPDATE SET json_blob = @jsonBlob",
            [|"@invitationId", box invitationId
             ;"@jsonBlob", jsonBlob |> asNullable|])
        |> execute db cancellationToken
}

let getAuxiliaryData (db : IDatabaseConnection<ReadOnly>)
                     (cancellationToken : CancellationToken)
                     (invitationId : string) =
    (@"SELECT json_blob
        FROM invitation_auxiliary_responses
        WHERE invitation_id = @invitationId",
        [|"@invitationId", box invitationId|])
    |> executeScalar<string> db cancellationToken

[<RequireQualifiedAccess>]
type EventRsvp =
    | Attending of personId : int * firstName : string * lastName : string option
    | NotAttending of personId : int * firstName : string * lastName : string option
    | NoResponse of personId : int * firstName : string * lastName : string option

let getRsvpStatusForEvent   (db : IDatabaseConnection<ReadOnly>)
                            (cancellationToken: CancellationToken)
                            (eventId : string) =
    (@"SELECT
            i.contact_id
            ,C.first_name
            ,c.last_name
            ,ir.response
            ,ir.created_at
         FROM invitations i
         INNER JOIN contacts c ON c.contact_id = i.contact_id
         LEFT JOIN invitation_responses ir ON ir.invitation_id = i.invitation_id
         WHERE i.scheduled_event_id = @eventId",
        [|"@eventId", box eventId|])
    |> executeReader db cancellationToken
    |> mapReader cancellationToken (fun reader ->
        let contactId = reader |> getInt32 0
        let firstName = reader |> getString 1
        let lastName = reader |> tryGetString 2
        let response = parseInviteResponse (reader |> tryGetByte 3) (reader |> tryGetDateTimeOffset 4)

        match response with
        | Some (InvitationResponse.Attending _) ->
            EventRsvp.Attending (contactId, firstName, lastName)
        | Some (InvitationResponse.NotAttending _) ->
            EventRsvp.NotAttending (contactId, firstName, lastName)
        | None ->
            EventRsvp.NoResponse (contactId, firstName, lastName))
