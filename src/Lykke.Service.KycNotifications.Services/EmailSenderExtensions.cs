using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Messages.Email;
using Lykke.Messages.Email.MessageData;
using Lykke.Service.Kyc.Abstractions.Domain.Documents;
using Lykke.Service.Kyc.Abstractions.Domain.Process;
using Lykke.Service.Kyc.Abstractions.Services.Models;


namespace Lykke.Service.KycNotifications.Services
{
    public static class EmailSenderExtensions {

        public static async Task SendWelcomeFxEmail(this IEmailSender sender, string partnerId, string email, string clientId) {
            var msgData = new KycOkData {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await sender.SendEmailAsync(partnerId, email, msgData);
        }

        public static async Task SendWelcomeFxCypEmail(this IEmailSender sender, string partnerId, string email, string clientId) {
            var msgData = new KycOkCypData {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await sender.SendEmailAsync(partnerId, email, msgData);
        }

        public static async Task SendDocumentsDeclined(this IEmailSender sender, string clientId, string partnerId, string email, string fullName, string lykkeKycWebsiteUrl, IKycDocumentV2[] documents) {
            var msgData = new DeclinedDocumentsData {
                FullName = fullName,
                LykkeKycWebsiteUrl = lykkeKycWebsiteUrl,
                Documents = DocumentModel.Create(documents)?.Select(doc => new KycDocumentData {
                    ClientId = clientId,
                    DocumentId = doc.DocumentId,
                    Type = doc.Type.Title,
                    KycComment = doc.Status.ToRejectReason() ?? String.Empty,
                    State = doc.Status.Name,
                    DateTime = doc.Status.WasTransferred.DateTime
                }).ToArray()
            };

            await sender.SendEmailAsync(partnerId, email, msgData);
        }

        public static async Task SendRejectedEmail(this IEmailSender sender, string partnerId, string email) {
            var msgData = new RejectedData();
            await sender.SendEmailAsync(partnerId, email, msgData);
        }
        public static async Task SendRejectedCypEmail(this IEmailSender sender, string partnerId, string email)
        {
            var msgData = new RejectedCypData();
            await sender.SendEmailAsync(partnerId, email, msgData);
        }

        public static async Task SendRestrictedAreaMessage(this IEmailSender sender, string partnerId, string email, string firstName, string lastName) {
            var msgData = new RestrictedAreaData {
                FirstName = firstName,
                LastName = lastName
            };

            await sender.SendEmailAsync(partnerId, email, msgData);
        }

        public static string ToRejectReason(this IStatus<IProcess<IKycDocumentV2>> status) {
            return status.Properties?["Reason"]?.ToObject<string>();
        }
    }
}
