using System.Threading.Tasks;
using AzureStorage.Queue;
using Lykke.Service.KycNotifications.Core;

namespace Lykke.Service.KycNotifications.Services {
    public class SmsNotificationSender : ISmsNotificationSender {
        private readonly IQueueExt _queueExt;

        public SmsNotificationSender(IQueueExt queueExt) {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(
                QueueType.Create("SimpleSmsMessage", typeof(SendSmsData<string>))
            );
        }

        public Task SendSmsAsync(string partnerId, string phoneNumber, string message, bool useAlternativeProvider) {
            var msg = new SendSmsData<string> {
                PartnerId = partnerId,
                MessageData = message,
                PhoneNumber = phoneNumber,
                UseAlternativeProvider = useAlternativeProvider
            };

            return _queueExt.PutMessageAsync(msg);
        }
    }
}