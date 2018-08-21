using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.KycNotifications.Core.Settings;

namespace Lykke.Service.KycNotifications.Settings
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KycNotificationsSettings KycNotificationsService { get; set; }
		public LykkeKycWebsiteUrlSettings LykkeKycWebsiteUrlSettings { get; set; }
		public SmsNotificationsSettings SmsNotifications { get; set; }
        public PushNotificationsServiceSettings PushNotificationsService { get; set; }
    }
}
