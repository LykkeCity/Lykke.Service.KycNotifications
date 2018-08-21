using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.KycNotifications.Projections;
using Lykke.Service.KycNotifications.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.KycNotifications.Modules
{
    public class CqrsModule : Module
    {
        private readonly AppSettings _settings;
		private readonly ILogFactory _log;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager, ILogFactory log)
        {
            _settings = settingsManager.CurrentValue;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

			var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.KycNotificationsService.RabbitMQConnectionString };
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory());

            builder.RegisterType<NotificationProjection>();

			var defaultRetryDelay = _settings.KycNotificationsService.RetryDelayInMilliseconds;
            builder.Register(ctx =>
            {
				var projection = ctx.Resolve<NotificationProjection>();

                return new CqrsEngine(
                    _log,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngine,
                    new DefaultEndpointProvider(),
                    true,
					Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq", SerializationFormat.ProtoBuf, environment: _settings.KycNotificationsService.Environment)),


                    Register.BoundedContext("kycstatuschange")
					    .ListeningCommands(typeof(ChangeStatusCommand))
					    .On("kyc-profile-status-changes-commands")
                        .WithProjection(projection, "kyc-profile-status-changes")
                );
            }).As<ICqrsEngine>().SingleInstance().AutoActivate();
            
        }
    }
}
