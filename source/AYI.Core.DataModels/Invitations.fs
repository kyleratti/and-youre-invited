module AYI.Core.DataModels.Invitations

open System

type SpringHasSprungAuxiliaryData = {
    Allergies : string option
    FoodBeingBrought : string option
}

type AuxiliaryRsvpData =
    | SpringHasSprung of SpringHasSprungAuxiliaryData
    member this.TrySpringHasSprung () =
        match this with
        | SpringHasSprung data -> Some data
        | _ -> None
    member this.Match (onSpringHasSprung : Func<SpringHasSprungAuxiliaryData, 'a>) =
        match this with
        | SpringHasSprung data -> onSpringHasSprung.Invoke data
