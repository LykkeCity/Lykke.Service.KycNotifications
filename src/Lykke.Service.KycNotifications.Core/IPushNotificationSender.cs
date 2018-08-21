using System;
using System.Threading.Tasks;

namespace Lykke.Service.KycNotifications.Core {
    public interface IPushNotificationSender {
        Task SendNotificationAsync(string notificationsId, NotificationType type, string message);
    }

    public enum NotificationType
    {
        KycSucceess = 1,
        KycRestrictedArea = 2,
        KycNeedToFillDocuments = 3,
    }

    public static class EventsAndEntities
    {
        public const string KYC = "KYC";

        public const string Ok = "Ok";
        public const string RestrictedArea = "RestrictedArea";
        public const string NeedToFillData = "NeedToFillData";

        public static string GetEvent(NotificationType notification)
        {
            switch (notification)
            {
                case NotificationType.KycSucceess:
                    return Ok;
                case NotificationType.KycRestrictedArea:
                    return RestrictedArea;
                case NotificationType.KycNeedToFillDocuments:
                    return NeedToFillData;
                default:
                    throw new ArgumentException("Unknown notification");
            }
        }
    }
}