namespace Lykke.Service.KycNotifications.Core.Domain.Templates {
    public interface IKycTemplate
    {
        string Id { get; }
        string Name { get; }
        string Type { get; }
        string Text { get; }
    }
}