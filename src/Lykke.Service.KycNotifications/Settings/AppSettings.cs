using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.KycNotifications.Core.Settings;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Service.KycNotifications.Settings
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KycNotificationsSettings KycNotificationsService { get; set; }
		public LykkeKycWebsiteUrlSettings LykkeKycWebsiteUrlSettings { get; set; }
		public SmsNotificationsSettings SmsNotifications { get; set; }
        public PushNotificationsServiceSettings PushNotificationsService { get; set; }
		public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
    }
}
