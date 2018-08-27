using System.Threading.Tasks;

namespace Lykke.Service.KycNotifications.Core.Services {

    public interface IKycNotification { }

    public interface IKycNotificationProvider<in T> where T: IKycNotification {
        Task Send(T notification);
    }

    public interface IKycNotificationService {
        Task Send<T>(T notification) where T: IKycNotification;
    }
}