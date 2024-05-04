module AYI.Core.Services.Invitations

open System.Text.Json
open System.Threading
open AYI.Core.Contracts
open AYI.Core.DTO
open AYI.Core.DataAccess
open AYI.Core.DataModels.Invitations
open DbAccess.Abstractions
open FSharp.Control
open FruityFoundation.FsBase

let private stringToOption = function
    | x when System.String.IsNullOrEmpty x -> None
    | x -> Some x

let parseAuxiliaryRsvpDataFromDto : AuxiliaryRsvpDataDto -> AuxiliaryRsvpData = function
    | :? AuxiliaryRsvpDataDto.SpringHasSprung as x ->
        ({
            Allergies = x.Allergies |> stringToOption
            FoodBeingBrought = x.FoodBeingBrought |> stringToOption 
        } : SpringHasSprungAuxiliaryData)
        |> AuxiliaryRsvpData.SpringHasSprung
    | dto -> failwith $"Unhandled DTO type: {dto.GetType().FullName}"

let private asNullable = function
    | None -> null
    | Some x -> x

let parseAuxiliaryRsvpDataToDto : AuxiliaryRsvpData -> AuxiliaryRsvpDataDto = function
    | AuxiliaryRsvpData.SpringHasSprung x ->
        let dto = AuxiliaryRsvpDataDto.SpringHasSprung ()
        dto.Allergies <- (x.Allergies |> asNullable)
        dto.FoodBeingBrought <- (x.FoodBeingBrought |> asNullable)
        dto

type InvitationService (dbConnectionFactory : IDbConnectionFactory) =
    interface IInvitationService with
        member _.GetAuxiliaryData (inviteId : string, cancellationToken : CancellationToken) = task {
            use! db = dbConnectionFactory.CreateReadOnlyConnection ()
            let! json = inviteId |> InvitationDataAccess.getAuxiliaryData db cancellationToken

            return json
                |> Option.map JsonSerializer.Deserialize<AuxiliaryRsvpDataDto>
                |> Option.map parseAuxiliaryRsvpDataFromDto
                |> Option.toMaybe
        }
