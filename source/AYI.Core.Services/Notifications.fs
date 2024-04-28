module AYI.Core.Services.Notifications

open System
open System.Diagnostics.CodeAnalysis
open System.Threading
open AYI.Core.Contracts
open AYI.Core.Contracts.Options
open AYI.Core.DataAccess
open AYI.Core.DataAccess.InvitationDataAccess
open AYI.Core.DataModels
open AYI.Core.DataModels.Invitations
open DbAccess.Abstractions
open FSharp.Control
open FruityFoundation.FsBase
open Microsoft.Extensions.Options

let private generateAuxDataPlainText : AuxiliaryRsvpData -> string = function
    | SpringHasSprung x ->
        let allergies = x.Allergies |> Option.defaultValue "(none)"
        let foodBeingBrought = x.FoodBeingBrought |> Option.defaultValue "(none)"
        sprintf @"Food Allergies: %s
Food Being Brought: %s" allergies foodBeingBrought

let private getEventData    (db : IDatabaseConnection<ReadOnly>)
                            (inviteService : IInvitationService)
                            (cancellationToken : CancellationToken)
                            (inviteId : string) = task {
    let! event =
        inviteId
        |> ScheduledEventDataAccess.findEventByInviteId db cancellationToken
        |> Task.map (Option.defaultWith (fun () -> failwithf "No event found for invitation id: %s" inviteId))
    let! personResponding =
        inviteId
        |> PeopleDataAccess.findByInviteId db cancellationToken
        |> Task.map (Option.defaultWith (fun () -> failwithf "No person found for invitation id: %s" inviteId))
    let! auxData =
        (inviteId, cancellationToken)
        |> inviteService.GetAuxiliaryData
        |> Task.map Option.fromMaybe
    let! rsvps =
        event.EventId
        |> InvitationDataAccess.getRsvpStatusForEvent db cancellationToken
        |> TaskSeq.toArrayAsync
    let! hosts =
        event.EventId
        |> ScheduledEventDataAccess.findHostsByEventId db cancellationToken
        |> TaskSeq.toArrayAsync

    return {|
             Event = event
             Person = personResponding
             AuxData = auxData
             Hosts = hosts
             Rsvps = rsvps
            |}
}

[<ExcludeFromCodeCoverage>]
let private stringToOption = function
    | x when String.IsNullOrEmpty x -> None
    | x -> Some x
    
let private formatName (firstName : string) (lastName : string option) =
    match firstName, lastName with
    | firstName, Some lastName -> $"{firstName} {lastName}"
    | firstName, None -> firstName

[<ExcludeFromCodeCoverage>]
let inline private uncurry f (a, b) = f a b

let private groupNamesByRsvpStatus (status: EventRsvp -> bool) (otherRsvps : EventRsvp array) =
    otherRsvps
    |> Seq.filter status
    |> Seq.map (function
        | EventRsvp.Attending (_, firstName, lastName)
        | EventRsvp.NotAttending (_, firstName, lastName)
        | EventRsvp.NoResponse (_, firstName, lastName) -> (firstName, lastName))
    |> Seq.map (uncurry formatName)
    |> Seq.sort
    |> Array.ofSeq

let private groupOtherRsvps (otherRsvps : EventRsvp array) =
    let attending = otherRsvps |> groupNamesByRsvpStatus (function EventRsvp.Attending _ -> true | _ -> false)
    let notAttending = otherRsvps |> groupNamesByRsvpStatus (function EventRsvp.NotAttending _ -> true | _ -> false)
    let noResponse = otherRsvps |> groupNamesByRsvpStatus (function EventRsvp.NoResponse _ -> true | _ -> false)

    (attending, notAttending, noResponse)

let private toPlainTextListWithDefault (defaultVal : string option) (input : string seq) =
    let items = input |> Seq.map (sprintf "- %s") |> Array.ofSeq
    if Seq.isEmpty items then defaultVal |> Option.defaultValue ""
    else  items |> String.concat Environment.NewLine

let private toPlainTextList (defaultVal : string) (input : string seq) =
    input |> toPlainTextListWithDefault (Some defaultVal)

let private generatePlainTextMessageBody (personResponding : Person)
                                (response : InvitationResponseDto)
                                (auxData : AuxiliaryRsvpData option)
                                (otherRsvps : EventRsvp array) =
    let name = formatName personResponding.FirstName personResponding.LastName
    let intro =
        match response with
        | InvitationResponseDto.Attending -> $"Yay! {name} is attending!"
        | InvitationResponseDto.NotAttending -> $"Womp. {name} can't make it."
        | _ -> failwithf "Unhandled response: %A" response
    let attending, notAttending, noResponse =
        otherRsvps
        |> groupOtherRsvps
        |> fun (attending, notAttending, noResponse) ->
            (attending |> toPlainTextList "(none)", notAttending |> toPlainTextList "(none)", noResponse |> toPlainTextList "(none)")
    let auxDataText =
        auxData
        |> Option.map generateAuxDataPlainText
        |> Option.map (fun s -> Environment.NewLine + Environment.NewLine + s)
        |> Option.defaultValue ""

    sprintf """%s%s

==== Attending ====
%s

==== Not Attending ====
%s

==== No Response ====
%s""" intro auxDataText attending notAttending noResponse

type NotificationService (dbConnectionFactory : IDbConnectionFactory
                          ,inviteService : IInvitationService
                          ,emailSender : IEmailSender) =
    interface INotificationService with
        member _.SendNewRsvpRecordedNotification (invitationId : string,
                                                  response : InvitationResponseDto,
                                                  cancellationToken : CancellationToken) = task {
            use! db = dbConnectionFactory.CreateReadOnlyConnection ()
            let! data = invitationId |> getEventData db inviteService cancellationToken
            let name = formatName data.Person.FirstName data.Person.LastName
            let rsvpStatus =
                match response with
                | InvitationResponseDto.Attending -> "attending"
                | InvitationResponseDto.NotAttending -> "not attending"
                | _ -> failwithf "Unhandled response: %A" response
            let subject = $"RSVP: {name} is {rsvpStatus}"
            let body = generatePlainTextMessageBody data.Person response data.AuxData data.Rsvps

            let recipients =
                data.Hosts
                |> Seq.choose (fun host ->
                    match host.EmailAddress with
                    | Some emailAddress when String.IsNullOrEmpty emailAddress -> None
                    | Some x -> Some x
                    | None -> None)
                |> Array.ofSeq

            if recipients.Length = 0 then return ()
            else
                for emailAddress in recipients do
                    do! emailSender.SendMessage(
                        recipient = emailAddress,
                        subject = subject,
                        message = MessageType.PlainText body,
                        cancellationToken = cancellationToken)
        }
