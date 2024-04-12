module AYI.Core.Services.ScheduledEvents

open System.Threading
open AYI.Core.Contracts
open AYI.Core.DataAccess
open AYI.Core.DataModels
open DbAccess.Abstractions
open FSharp.Control
open FruityFoundation.FsBase

let private orFailWith (errorMessage : string) = function
    | Some x -> x
    | None -> failwith errorMessage

let private emptySeqFailWith (errorMessage : string) = function
    | x when Seq.isEmpty x -> failwith errorMessage
    | x -> x

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

                let! allInvites =
                    scheduledEvent.EventId
                    |> InvitationDataAccess.findInvitationsByEventId db cancellationToken
                    |> TaskSeq.toArrayAsync
                    |> Task.map (emptySeqFailWith (sprintf "Unable to find invitations for event with id %s" scheduledEvent.EventId))

                let! invitedPeople =
                    allInvites
                    |> Seq.map (fun x -> x.PersonId)
                    |> Array.ofSeq
                    |> PeopleDataAccess.findPeopleById db cancellationToken
                    |> TaskSeq.toArrayAsync
                    |> Task.map (emptySeqFailWith (sprintf "Unable to find people for event with id %s" scheduledEvent.EventId))

                return Some ({
                        Event = scheduledEvent
                        Location = location
                        AllInvitations = allInvites
                        AllInvitedPeople = invitedPeople
                    }: EventInfo)
                    |> Option.toMaybe
        }
