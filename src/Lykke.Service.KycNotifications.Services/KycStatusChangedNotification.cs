using Lykke.Service.KycNotifications.Core.Services;

namespace Lykke.Service.KycNotifications.Services
{
    public class KycStatusChangedNotification : IKycNotification
    {
        public KycStatusChangedNotification(string clientId, string profileType, string currentStatus, string previousStatus)
        {
            ClientId = clientId;
            ProfileType = profileType;
            CurrentStatus = currentStatus;
            PreviousStatus = previousStatus;
        }

        public string ClientId { get; }
        public string ProfileType { get; }
        public string CurrentStatus { get; }
        public string PreviousStatus { get; }
    }
}