module AYI.Core.Services.Contacts

open System.Collections.Generic
open System.Threading
open AYI.Core.Contracts
open AYI.Core.DataAccess
open DbAccess.Abstractions
open FSharp.Control

type ContactService (dbConnectionFactory : IDbConnectionFactory) =
    interface IContactService with
        member this.GetContactsById (contactIds : IReadOnlyCollection<int>, cancellationToken : CancellationToken) = task {
            use! dbConnection = dbConnectionFactory.CreateReadOnlyConnection ()

            return contactIds
                    |> ContactDataAccess.findContactsById dbConnection cancellationToken
        }
