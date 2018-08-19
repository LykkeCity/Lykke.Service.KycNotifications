using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycNotifications.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
