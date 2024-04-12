module AYI.Core.Services.Invitations

open System.Collections.Generic
open AYI.Core.Contracts
open AYI.Core.DataAccess
open DbAccess.Abstractions
open FSharp.Control

type InvitationService (dbConnectionFactory : IDbConnectionFactory) =
    interface IInvitationService with
        member _.GetInvitationsForEvent (eventId, cancellationToken) = task {
            use! dbConnection = dbConnectionFactory.CreateReadOnlyConnection ()
            return eventId
                    |> InvitationDataAccess.findInvitationsByEventId dbConnection cancellationToken
        }
