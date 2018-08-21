using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.KycNotifications.Core.Services;
using Lykke.Service.KycNotifications.Services;

namespace Lykke.Service.KycNotifications.Projections
{
	public class NotificationProjection
    {
        private readonly ILogFactory _log;
		private readonly IKycNotificationService _kycNotificationService;

		public NotificationProjection(
			[NotNull] ILogFactory log,
			[NotNull] IKycNotificationService kycNotificationService)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
			_kycNotificationService = kycNotificationService ?? throw new ArgumentNullException(nameof(kycNotificationService));
        }
        
		public async Task Handle(ChangeStatusCommand cmd)
        {
			var message = new KycStatusChangedNotification(cmd.ClientId, cmd.ProfileType, cmd.NewStatus, cmd.OldStatus);
			await _kycNotificationService.Send(message);

           
        }
    }
}
