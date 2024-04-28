namespace AYI.Core.DataModels

open System
open System.Collections.Generic

type ScheduledEvent = {
    EventId : string
    UrlSlug : string
    LocationId : int
    Title : string
    StartsAt : DateTimeOffset
    EndsAt : DateTimeOffset option
}

type InvitationResponse =
    | Attending of DateTimeOffset
    | NotAttending of DateTimeOffset
    member this.Merge   (onAttending : Func<DateTimeOffset, 'a>)
                        (onNotAttending : Func<DateTimeOffset, 'a>) =
        match this with
        | Attending respondedAt -> onAttending.Invoke(respondedAt)
        | NotAttending respondedAt -> onNotAttending.Invoke(respondedAt)

type InvitationResponseDto =
    | Attending = 1uy
    | NotAttending = 2uy

type InvitationDto = {
    InvitationId : string
    PersonId : int
    CanViewGuestList : bool
    CreatedAt : DateTimeOffset
    Response : (InvitationResponseDto * DateTimeOffset) option
}

type Invitation = {
    InvitationId : string
    Person : Person
    CanViewGuestList : bool
    CreatedAt : DateTimeOffset
    Response : InvitationResponse option
}

type EventInfo = {
    Event : ScheduledEvent
    Location : Location
    ThisInvite : Invitation
    AllInvitations : IReadOnlyCollection<Invitation>
    AllInvitedPeople : IReadOnlyCollection<Person>
}
