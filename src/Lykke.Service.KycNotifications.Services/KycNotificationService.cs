using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.KycNotifications.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.KycNotifications.Services
{
    public class KycNotificationService : IKycNotificationService
    {
        private readonly IServiceProvider _serviceProvider;

        public KycNotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Send<T>(T notification) where T : IKycNotification
        {
            var notificationProviders = _serviceProvider.GetServices<IKycNotificationProvider<T>>();

            await Task.WhenAll(notificationProviders.Select(x => x.Send(notification)));
        }
    }
}