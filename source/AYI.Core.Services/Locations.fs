module AYI.Core.Services.Locations

open System.Threading
open AYI.Core.Contracts
open AYI.Core.DataAccess
open DbAccess.Abstractions
open FruityFoundation.FsBase

type LocationService (dbConnectionFactory : IDbConnectionFactory) =
    interface ILocationService with
        member _.GetLocationById (locationId : int, cancellationToken : CancellationToken) = task {
            use! dbConnection = dbConnectionFactory.CreateReadOnlyConnection ()
            return! locationId
                    |> LocationDataAccess.findLocationById dbConnection cancellationToken
                    |> Task.map Option.toMaybe
        }
