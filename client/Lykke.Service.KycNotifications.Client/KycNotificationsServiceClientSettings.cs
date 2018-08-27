using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycNotifications.Client 
{
    /// <summary>
    /// KycNotifications client settings.
    /// </summary>
    public class KycNotificationsServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
