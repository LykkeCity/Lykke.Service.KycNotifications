using JetBrains.Annotations;

namespace Lykke.Service.KycNotifications.Client
{
    /// <summary>
    /// KycNotifications client interface.
    /// </summary>
    [PublicAPI]
    public interface IKycNotificationsClient
    {
        /// <summary>Application Api interface</summary>
        IKycNotificationsApi Api { get; }
    }
}
