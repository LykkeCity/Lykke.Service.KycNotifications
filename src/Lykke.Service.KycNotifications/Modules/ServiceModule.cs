using Autofac;
using AzureStorage.Queue;
using Lykke.Service.KycNotifications.Core;
using Lykke.Service.KycNotifications.Core.Services;
using Lykke.Service.KycNotifications.Services;
using Lykke.Service.KycNotifications.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.KycNotifications.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
			builder.RegisterType<KycNotificationService>().As<IKycNotificationService>().SingleInstance();
            builder.RegisterType<KycNotificationProvider>().As<IKycNotificationProvider<KycStatusChangedNotification>>().SingleInstance();

			IQueueExt smsQueue = AzureQueueExt.Create(_appSettings.ConnectionString(x => x.SmsNotifications.AzureQueue.ConnectionString), _appSettings.CurrentValue.SmsNotifications.AzureQueue.QueueName);
            builder.RegisterInstance<ISmsNotificationSender>(
                new SmsNotificationSender(smsQueue)
                );

            builder.RegisterInstance<IPushNotificationSender>(
				new PushNotificationSender(_appSettings.CurrentValue.PushNotificationsService.HubConnectionString, _appSettings.CurrentValue.PushNotificationsService.HubName)
                );
        }
    }
}
