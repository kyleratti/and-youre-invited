namespace AYI.Core.DataModels

type Contact = {
    ContactId : int
    FirstName : string
    LastName : string option
    PhoneNumberE164 : string option
    EmailAddress : string option
}
