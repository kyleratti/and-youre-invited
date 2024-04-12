module AYI.Core.DataAccess.LocationDataAccess

open System.Threading
open AYI.Core.DataModels
open DbAccess.Abstractions
open DbHelpers
open FSharp.Control
open FruityFoundation.Db.Db

let findLocationById (db : IDatabaseConnection<ReadOnly>) (cancellationToken : CancellationToken) (locationId : int) = task {
    return! (
            @"SELECT
                l.location_id
                ,l.street_1
                ,l.street_2
                ,l.city
                ,l.state
                ,l.zip_code
            FROM locations l WHERE l.location_id = @locationId",
            [|"@locationId", box locationId|]
        )
        |> executeReader db cancellationToken
        |> mapReader cancellationToken (fun reader ->
            {
                LocationId = reader |> getInt32 0
                Street1 = reader |> getString 1
                Street2 = reader |> tryGetString 2
                City = reader |> getString 3
                State = reader |> getString 4
                ZipCode = reader |> getString 5
            }: Location)
        |> TaskSeq.tryHead
}
