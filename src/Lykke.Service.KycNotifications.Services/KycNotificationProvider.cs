using System.Linq;
using System.Threading.Tasks;
using Lykke.Messages.Email;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Domain.Documents;
using Lykke.Service.Kyc.Abstractions.Domain.Documents.Types;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.KycNotifications.Core;
using Lykke.Service.KycNotifications.Core.Services;
using Lykke.Service.KycNotifications.Core.Settings;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.TemplateFormatter.Client;
using Lykke.Service.TemplateFormatter.TemplateModels;

namespace Lykke.Service.KycNotifications.Services
{
    public class KycNotificationProvider : IKycNotificationProvider<KycStatusChangedNotification>
    {
        private readonly IClientAccountClient _clientAccountService;
        private readonly IPersonalDataService _personalDataService;
        private readonly IKycDocumentsServiceV2 _kycDocumentsService;
        private readonly IEmailSender _emailSender;
        private readonly LykkeKycWebsiteUrlSettings _lykkeKycWebsiteUrlSettings;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ISmsNotificationSender _smsSender;
        private readonly IPushNotificationSender _pushNotificationSender;

        public KycNotificationProvider(
            IClientAccountClient clientAccountService,
            IPersonalDataService personalDataService,
            IKycDocumentsServiceV2 kycDocumentsService,
            IEmailSender emailSender,
            LykkeKycWebsiteUrlSettings lykkeKycWebsiteUrlSettings,
            ITemplateFormatter templateFormatter,
            ISmsNotificationSender smsSender,
            IPushNotificationSender pushNotificationSender)
        {
            _clientAccountService = clientAccountService;
            _personalDataService = personalDataService;
            _kycDocumentsService = kycDocumentsService;
            _emailSender = emailSender;
            _lykkeKycWebsiteUrlSettings = lykkeKycWebsiteUrlSettings;
            _templateFormatter = templateFormatter;
            _smsSender = smsSender;
            _pushNotificationSender = pushNotificationSender;
        }

        public async Task Send(KycStatusChangedNotification notification)
        {
            var clientAcc = await _clientAccountService.GetByIdAsync(notification.ClientId);
            var personalData = await _personalDataService.GetAsync(notification.ClientId);
            var pushSettings = await _clientAccountService.GetPushNotificationAsync(notification.ClientId);
            var useAlternativeProvider = (await _clientAccountService.GetSmsAsync(notification.ClientId)).UseAlternativeProvider;

            Task smsTask = null;
            Task emailTask = null;
            Task notificationTask = null;
            try
            {
                switch (notification.CurrentStatus)
                {
                    case nameof(KycStatus.Ok):
                        emailTask = notification.ProfileType == nameof(KycProfile.LykkeCyprus) 
                            ? _emailSender.SendWelcomeFxCypEmail(clientAcc.PartnerId, clientAcc.Email, notification.ClientId) 
                            : _emailSender.SendWelcomeFxEmail(clientAcc.PartnerId, clientAcc.Email, notification.ClientId);

                        smsTask = Task.Run(async () => {
                            var message = await _templateFormatter.FormatAsync(nameof(SmsKycApprovedTemplate), clientAcc.PartnerId, "EN", new SmsKycApprovedTemplate());
                            await _smsSender.SendSmsAsync(clientAcc.PartnerId, clientAcc.Phone, message.Subject, useAlternativeProvider);
                        });

                        if (pushSettings.Enabled)
                            notificationTask = _pushNotificationSender.SendNotificationAsync(clientAcc.NotificationsId, NotificationType.KycSucceess,
                                "You are approved to trade FX."
                            );
                        break;

                    case nameof(KycStatus.NeedToFillData):
                        var documents = await _kycDocumentsService.GetCurrentDocumentsAsync(notification.ClientId);
                        var declinedDocuments = documents
                            .Where(item => item.Status.Name == CheckDocumentPorcess.DeclinedState.Name && item.Type.Name != OtherDocument.ApiType)
                            .ToArray();

                        if (declinedDocuments.Length > 0)
                            emailTask = _emailSender.SendDocumentsDeclined(clientAcc.Id, clientAcc.PartnerId, clientAcc.Email, personalData.FullName, _lykkeKycWebsiteUrlSettings.Url, declinedDocuments);

                        smsTask = Task.Run(async () => {
                            var message = await _templateFormatter.FormatAsync(nameof(SmsKycAttentionNeededTemplate), clientAcc.PartnerId, "EN", new SmsKycAttentionNeededTemplate());
                            await _smsSender.SendSmsAsync(clientAcc.PartnerId, clientAcc.Phone, message.Subject, useAlternativeProvider);
                        });

                        if (pushSettings.Enabled)
                            notificationTask = _pushNotificationSender.SendNotificationAsync(clientAcc.NotificationsId, NotificationType.KycNeedToFillDocuments,
                                "Some of your photos have failed verification, tap to re-upload."
                            );
                        break;

                    case nameof(KycStatus.Rejected):
                        emailTask = notification.ProfileType == nameof(KycProfile.LykkeCyprus) 
                            ? _emailSender.SendRejectedCypEmail(clientAcc.PartnerId, clientAcc.Email) 
                            : _emailSender.SendRejectedEmail(clientAcc.PartnerId, clientAcc.Email);
                        break;

                    case nameof(KycStatus.RestrictedArea):
                        emailTask = _emailSender.SendRestrictedAreaMessage(clientAcc.PartnerId, clientAcc.Email, personalData.FirstName, personalData.LastName);
                        if (pushSettings.Enabled)
                            notificationTask = _pushNotificationSender.SendNotificationAsync(clientAcc.NotificationsId, NotificationType.KycRestrictedArea,
                                "Lykke is not allowed to onboard clients from your region at the moment. We apologise for the inconvenience."
                            );
                        break;
                }
            }
            finally
            {
                if (smsTask != null) await smsTask;
                if (emailTask != null) await emailTask;
                if (notificationTask != null) await notificationTask;
            }

        }
    }
}
