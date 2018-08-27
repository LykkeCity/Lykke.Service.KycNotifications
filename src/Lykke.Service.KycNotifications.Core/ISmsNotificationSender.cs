using System.Threading.Tasks;

namespace Lykke.Service.KycNotifications.Core {
    public interface ISmsNotificationSender
    {
        Task SendSmsAsync(string partnerId, string phoneNumber, string message, bool useAlternativeProvider);
    }
}