namespace AYI.Core.DataModels

type Location = {
    LocationId : int
    Street1 : string
    Street2 : string option
    City : string
    State : string
    ZipCode : string
}
