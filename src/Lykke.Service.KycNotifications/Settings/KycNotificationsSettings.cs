using JetBrains.Annotations;

namespace Lykke.Service.KycNotifications.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KycNotificationsSettings
    {
        public DbSettings Db { get; set; }
    }
}
