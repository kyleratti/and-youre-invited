module AYI.Core.DataAccess.DbHelpers

open System
open System.Collections.Generic
open System.Data.Common
open System.Threading
open System.Threading.Tasks
open DbAccess.Abstractions
open FSharp.Control

let mapTask f (t:Task<_>) = task { 
    let! r = t
    return f r
}

let createReadOnlyConnection (factory : IDbConnectionFactory) = task {
    let! connection = factory.CreateReadOnlyConnection ()
    return connection
}

let query<'a> (connection : IDatabaseConnection<_>) (cancellationToken : CancellationToken) (cmd : string * (string * obj) array) : Task<IEnumerable<'a>> = task {
    let sql, parameters = cmd |> (fun (sql, parms) -> sql, dict parms)
    return! connection.Query<'a>(sql, parameters, cancellationToken)
}

let executeReader (connection : IDatabaseConnection<_>) (cancellationToken : CancellationToken) (cmd : string * (string * obj) array) : Task<DbDataReader> = task {
    let sql, parameters = cmd |> (fun (sql, parms) -> sql, dict parms)
    return! connection.ExecuteReader(sql, parameters, cancellationToken)
}

let mapReader<'a>   (cancellationToken : CancellationToken)
                    (mapper : DbDataReader -> 'a)
                    (reader : Task<DbDataReader>) = taskSeq {
    use! reader = reader
    while! reader.ReadAsync (cancellationToken) do
        yield mapper reader
}

let mapReaderSync<'a>   (cancellationToken : CancellationToken)
                    (mapper : DbDataReader -> 'a)
                    (reader : DbDataReader) = seq {
    use reader = reader
    while reader.Read () do
        yield mapper reader
}

let tryParseDateTimeOffset (str : string) =
    match DateTimeOffset.TryParse str with
    | true, x -> Some x
    | false, _ -> None

let tryGetDateTimeOffset (ordinal : int) (reader : DbDataReader) =
    if reader.IsDBNull ordinal then None
    else
        reader.GetString ordinal
        |> tryParseDateTimeOffset

let getDateTimeOffset (ordinal : int) (reader : DbDataReader) =
    reader
    |> tryGetDateTimeOffset ordinal
    |> Option.defaultWith (fun () -> failwithf "Unable to get DateTimeOffset at ordinal %d" ordinal)

let tryGetByte (ordinal : int) (reader : DbDataReader) =
    if reader.IsDBNull ordinal then None
    else Some (reader.GetByte ordinal)
