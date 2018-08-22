using Autofac;
using AzureStorage.Queue;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.KycNotifications.Core;
using Lykke.Service.KycNotifications.Core.Services;
using Lykke.Service.KycNotifications.Services;
using Lykke.Service.KycNotifications.Settings;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
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
			builder.Register(ctx =>
			{
				return new PersonalDataService(_appSettings.CurrentValue.PersonalDataServiceClient, ctx.Resolve<ILogFactory>().CreateLog(this));
			}).As<IPersonalDataService>().SingleInstance();

			builder.Register(ctx =>
            {
				return new KycDocumentsServiceV2Client(_appSettings.CurrentValue.DocumentsServiceV2Client, ctx.Resolve<ILogFactory>());
			}).As<IKycDocumentsServiceV2>().SingleInstance();
            
			builder.RegisterInstance<IClientAccountClient>(new ClientAccountClient(_appSettings.CurrentValue.ClientAccountServiceClient.ServiceUrl));

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
