module AYI.Core.Services.ScheduledEvents

open System.Collections.Generic
open System.Data
open System.Text.Json
open System.Threading
open AYI.Core.Contracts
open AYI.Core.DataAccess
open AYI.Core.DataModels
open AYI.Core.DataModels.Invitations
open DbAccess.Abstractions
open FSharp.Control
open FruityFoundation.Base.Structures
open FruityFoundation.FsBase

let private orFailWith (errorMessage : string) = function
    | Some x -> x
    | None -> failwith errorMessage

let private emptySeqFailWith (errorMessage : string) = function
    | x when Seq.isEmpty x -> failwith errorMessage
    | x -> x

let private toInvitationResponse = function
        | Some (InvitationResponseDto.Attending, respAt) ->
            Some (InvitationResponse.Attending respAt)
        | Some (InvitationResponseDto.NotAttending, respAt) ->
            Some (InvitationResponse.NotAttending respAt)
        | None -> None
        | resp -> failwithf "Invalid response (%A)" resp

let private getThisInvite   (allInvites : IReadOnlyCollection<InvitationDto>)
                            (invitedPeople : IReadOnlyCollection<Contact>)
                            (inviteId : string) =
    let thisInvite =
        allInvites
        |> Seq.tryFind (fun x -> x.InvitationId = inviteId)
        |> orFailWith (sprintf "Unable to find invitation with id %s" inviteId)

    let thisPerson =
        invitedPeople
        |> Seq.tryFind (fun x -> x.ContactId = thisInvite.ContactId)
        |> orFailWith (sprintf "Unable to find person with id %d" thisInvite.ContactId)

    {
        InvitationId = thisInvite.InvitationId
        Contact = thisPerson
        CanViewGuestList = thisInvite.CanViewGuestList
        CreatedAt = thisInvite.CreatedAt
        Response = thisInvite.Response |> toInvitationResponse
    } : Invitation

let private getPerson (invitedPeople : IReadOnlyCollection<Contact>) (personId : int) =
    invitedPeople
    |> Seq.tryFind (fun x -> x.ContactId = personId)
    |> orFailWith (sprintf "Unable to find person with id %d" personId)

let private toInvitation (invitedPeople : IReadOnlyCollection<Contact>) (dto : InvitationDto) =
    {
        InvitationId = dto.InvitationId
        Contact = dto.ContactId |> getPerson invitedPeople
        CanViewGuestList = dto.CanViewGuestList
        CreatedAt = dto.CreatedAt
        Response = dto.Response |> toInvitationResponse
    }: Invitation

type ScheduledEventService (dbConnectionFactory : IDbConnectionFactory) =
    interface IScheduledEventService with
        member _.GetScheduledEventByInviteId (inviteId : string, cancellationToken : CancellationToken) = task {
            use! db = dbConnectionFactory.CreateReadOnlyConnection ()
            let! result =
                inviteId
                |> ScheduledEventDataAccess.findEventByInviteId db cancellationToken
            return result |> Option.toMaybe
        }

        member this.GetEventInfoByInviteId(inviteId, cancellationToken) = task {
            use! db = dbConnectionFactory.CreateReadOnlyConnection ()
            let! scheduledEvent =
                inviteId
                |> ScheduledEventDataAccess.findEventByInviteId db cancellationToken

            if Option.isNone scheduledEvent then return None |> Option.toMaybe
            else
                let scheduledEvent = Option.get scheduledEvent
                let! location =
                    scheduledEvent.LocationId
                    |> LocationDataAccess.findLocationById db cancellationToken
                    |> Task.map (orFailWith (sprintf "Unable to find location with id %d" scheduledEvent.LocationId))

                let! allInviteDtos =
                    scheduledEvent.EventId
                    |> InvitationDataAccess.findInvitationsByEventId db cancellationToken
                    |> TaskSeq.toArrayAsync
                    |> Task.map (emptySeqFailWith (sprintf "Unable to find invitations for event with id %s" scheduledEvent.EventId))

                let! invitedPeople =
                    allInviteDtos
                    |> Seq.map (_.ContactId)
                    |> Array.ofSeq
                    |> PeopleDataAccess.findPeopleById db cancellationToken
                    |> TaskSeq.toArrayAsync
                    |> Task.map (emptySeqFailWith (sprintf "Unable to find people for event with id %s" scheduledEvent.EventId))

                let! allInvites =
                    scheduledEvent.EventId
                    |> InvitationDataAccess.findInvitationsByEventId db cancellationToken
                    |> TaskSeq.map (fun invitationDto -> invitationDto |> toInvitation invitedPeople)
                    |> TaskSeq.toArrayAsync
                    |> Task.map (emptySeqFailWith (sprintf "Unable to find invitations for event with id %s" scheduledEvent.EventId))

                let thisInvite = inviteId |> getThisInvite allInviteDtos invitedPeople

                return Some ({
                        Event = scheduledEvent
                        Location = location
                        ThisInvite = thisInvite
                        AllInvitations = allInvites
                        AllInvitedContacts = invitedPeople
                    }: EventInfo)
                    |> Option.toMaybe
        }

        member this.RecordRsvp (inviteId : string,
                                response : InvitationResponseDto,
                                auxiliaryData : AuxiliaryRsvpData Maybe,
                                cancellationToken : CancellationToken) = task {
            use! db = dbConnectionFactory.CreateConnection ()
            use! tx = db.CreateTransaction ()

            do! (inviteId, response)
                ||> InvitationDataAccess.addInvitationResponse tx cancellationToken

            if Option.isSome (Option.fromMaybe auxiliaryData) then
                let dataJson =
                    auxiliaryData
                    |> Option.fromMaybe
                    |> Option.map Invitations.parseAuxiliaryRsvpDataToDto
                    |> Option.map JsonSerializer.Serialize
                do! (inviteId, dataJson)
                    ||> InvitationDataAccess.addAuxiliaryResponse tx cancellationToken

            do! tx.Commit cancellationToken
        }
