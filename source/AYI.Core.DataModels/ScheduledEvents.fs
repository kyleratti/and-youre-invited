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

type Invitation = {
    InvitationId : string
    PersonId : int
    CanViewGuestList : bool
    CreatedAt : DateTimeOffset
    Response : InvitationResponse option
}

type EventInfo = {
    Event : ScheduledEvent
    Location : Location
    AllInvitations : IReadOnlyCollection<Invitation>
    AllInvitedPeople : IReadOnlyCollection<Person>
}
