module AYI.Core.Services.Task

open System.Threading.Tasks

let map f (t : Task<_>) = task { 
    let! r = t
    return f r
}
