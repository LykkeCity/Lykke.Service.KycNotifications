using JetBrains.Annotations;

namespace Lykke.Service.KycNotifications.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KycNotificationsSettings
    {
        public DbSettings Db { get; set; }
		public string RabbitMQConnectionString { get; internal set; }
		public object RetryDelayInMilliseconds { get; internal set; }
		public string Environment { get; internal set; }
	}
}
