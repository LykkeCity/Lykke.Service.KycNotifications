using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.KycNotifications.Core.Services;
using Lykke.Service.KycNotifications.Services;

namespace Lykke.Service.KycNotifications.Projections
{
    public class NotificationProjection
    {
        private readonly ILogFactory _log;
		private readonly IKycNotificationProvider<KycStatusChangedNotification> _kycNotificationProvider;

        public NotificationProjection(
            [NotNull] ILogFactory log,
			[NotNull] IKycNotificationProvider<KycStatusChangedNotification> kycNotificationProvider)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
			_kycNotificationProvider = kycNotificationProvider ?? throw new ArgumentNullException(nameof(kycNotificationProvider));
        }
      
        public async Task Handle(ChangeStatusEvent cmd)
        {
			
            var message = new KycStatusChangedNotification(cmd.ClientId, cmd.ProfileType, cmd.NewStatus, cmd.OldStatus);
			await _kycNotificationProvider.Send(message);


        }
    }
}
