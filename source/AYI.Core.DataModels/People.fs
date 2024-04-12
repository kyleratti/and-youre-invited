namespace AYI.Core.DataModels

type Person = {
    PersonId : int
    FirstName : string
    LastName : string option
    PhoneNumberE164 : string option
    EmailAddress : string option
}
