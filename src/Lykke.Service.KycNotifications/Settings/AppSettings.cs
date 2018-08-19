using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.KycNotifications.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KycNotificationsSettings KycNotificationsService { get; set; }
    }
}
