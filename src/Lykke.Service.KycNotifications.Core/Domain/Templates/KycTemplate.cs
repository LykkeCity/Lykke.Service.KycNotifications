namespace Lykke.Service.KycNotifications.Core.Domain.Templates {
    public class KycTemplate : IKycTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }

        public static KycTemplate Create(IKycTemplate data) {
            return new KycTemplate {
                Id = data.Id,
                Name = data.Name,
                Type = data.Type,
                Text = data.Text
            };
        }
    }
}