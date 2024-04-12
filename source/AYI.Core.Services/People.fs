module AYI.Core.Services.People

open System.Collections.Generic
open System.Threading
open AYI.Core.Contracts
open AYI.Core.DataAccess
open DbAccess.Abstractions
open FSharp.Control

type PeopleService (dbConnectionFactory : IDbConnectionFactory) =
    interface IPeopleService with
        member this.GetPeopleById (personIds : IReadOnlyCollection<int>, cancellationToken : CancellationToken) = task {
            use! dbConnection = dbConnectionFactory.CreateReadOnlyConnection ()

            return personIds
                    |> PeopleDataAccess.findPeopleById dbConnection cancellationToken
        }
