module AYI.Core.Services.EmailSender

open System.Threading
open AYI.Core.Contracts
open AYI.Core.Contracts.Options
open Azure.Communication.Email
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Options

type EmailSenderService (config : IConfiguration
                         ,emailNotificationOptions : IOptions<EmailNotificationOptions>) =
    let connectionString = config.GetConnectionString "AzureCommunicationServices"

    interface IEmailSender with
        member this.SendMessage (recipient : string,
                                 subject : string,
                                 message : MessageType,
                                 cancellationToken : CancellationToken) = task {
            let client = EmailClient connectionString
            let messageContent = EmailContent subject
            messageContent.PlainText <- message.Merge(
                onPlainText = (_.Text),
                onHtml = (fun x -> failwith "HTML type is not supported"))
            let message = EmailMessage(
                senderAddress = emailNotificationOptions.Value.SenderAddress,
                recipientAddress = recipient,
                content = messageContent)

            let! sendOperation = client.SendAsync (
                Azure.WaitUntil.Completed,
                message = message,
                cancellationToken = cancellationToken)

            return ()
        }
