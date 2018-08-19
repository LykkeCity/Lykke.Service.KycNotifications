using Lykke.HttpClientGenerator;

namespace Lykke.Service.KycNotifications.Client
{
    /// <summary>
    /// KycNotifications API aggregating interface.
    /// </summary>
    public class KycNotificationsClient : IKycNotificationsClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to KycNotifications Api.</summary>
        public IKycNotificationsApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public KycNotificationsClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IKycNotificationsApi>();
        }
    }
}
