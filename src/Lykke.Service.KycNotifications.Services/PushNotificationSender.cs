using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Lykke.Service.KycNotifications.Core;
using Newtonsoft.Json;

namespace Lykke.Service.KycNotifications.Services {
    public enum Device {
        Android,
        Ios
    }

    public interface IIosNotification { }

    public interface IAndroidNotification { }

    public class IosFields {
        [JsonProperty("alert")]
        public string Alert { get; set; }

        [JsonProperty("type")]
        public NotificationType Type { get; set; }

        [JsonProperty("sound")]
        public string Sound { get; set; } = "default";
    }

    public class AndroidPayloadFields {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class IosNotification : IIosNotification {
        [JsonProperty("aps")]
        public IosFields Aps { get; set; }
    }

    public class AndoridPayloadNotification : IAndroidNotification {
        [JsonProperty("data")]
        public AndroidPayloadFields Data { get; set; }
    }

    public class PushNotificationSender : IPushNotificationSender {
        private readonly string _connectionString;
        private readonly string _hubName;

        public PushNotificationSender(string connectionString, string hubName) {
            _connectionString = connectionString;
            _hubName = hubName;
        }

        public async Task SendNotificationAsync(string notificationsId, NotificationType type, string message) {
            var apnsMessage = new IosNotification {
                Aps = new IosFields {
                    Alert = message,
                    Type = type
                }
            };

            var gcmMessage = new AndoridPayloadNotification {
                Data = new AndroidPayloadFields {
                    Entity = "KYC",
                    Event = EventsAndEntities.GetEvent(type),
                    Message = message,
                }
            };

            await SendIosNotificationAsync(notificationsId, apnsMessage);
            await SendAndroidNotificationAsync(notificationsId, gcmMessage);
        }

        private async Task SendIosNotificationAsync(string notificationId, IIosNotification notification) {
            await SendRawNotificationAsync(Device.Ios, notificationId, notification.ToJson(ignoreNulls: true));
        }

        private async Task SendAndroidNotificationAsync(string notificationId, IAndroidNotification notification) {
            await SendRawNotificationAsync(Device.Android, notificationId, notification.ToJson(ignoreNulls: true));
        }

        private async Task SendRawNotificationAsync(Device device, string notificationId, string payload) {
            try {
                if (string.IsNullOrEmpty(notificationId)) {
                    return;
                }

                var hub = CustomNotificationHubClient.CreateClientFromConnectionString(_connectionString, _hubName);
                if (device == Device.Ios)
                    await hub.SendAppleNativeNotificationAsync(payload, new []{ notificationId });
                else
                    await hub.SendGcmNativeNotificationAsync(payload, new[] { notificationId });
            }
            catch (Exception) {
                //TODO: process exception
            }
        }

        public class CustomNotificationHubClient {
            private readonly string _sharedAccessKey;
            private readonly string _sharedAccessKeyName;
            private readonly string _url;

            public CustomNotificationHubClient(string sharedAccessKey, string sharedAccessKeyName, string baseUrl, string hubName) {
                _sharedAccessKey = sharedAccessKey;
                _sharedAccessKeyName = sharedAccessKeyName;
                _url = string.Format("https://{0}/{1}/messages/?api-version=2015-08", baseUrl, hubName);
            }

            public static CustomNotificationHubClient CreateClientFromConnectionString(
                string connectionString,
                string hubName) {
                var regexp = new Regex(@"sb://(?<url>[A-z\.\-]*)/;SharedAccessKeyName=(?<keyName>[A-z0-9]*);.*SharedAccessKey=(?<key>[A-z0-9+=/]*)");
                var match = regexp.Match(connectionString);
                var baseUrl = match.Groups["url"].Value;
                var accessKey = match.Groups["key"].Value;
                var accessKeyName = match.Groups["keyName"].Value;

                return new CustomNotificationHubClient(accessKey, accessKeyName, baseUrl, hubName);
            }

            public async Task SendGcmNativeNotificationAsync(string jsonPayload, string[] ids) {
                var headers = new Dictionary<string, string> {
                    { "ServiceBusNotification-Format", "gcm" },
                    { "ServiceBusNotification-Tags", string.Join("||", ids) }
                };

                await SendNotification(jsonPayload, headers);
            }

            public async Task SendAppleNativeNotificationAsync(string jsonPayload, string[] ids) {
                var headers = new Dictionary<string, string> {
                    { "ServiceBusNotification-Format", "apple" },
                    { "ServiceBusNotification-Tags", string.Join("||", ids) },
                    { "ServiceBusNotification-Apns-Expiry", DateTime.UtcNow.AddDays(10).ToString("s") }
                };

                await SendNotification(jsonPayload, headers);
            }

            public async Task SendNotification(string payload, Dictionary<string, string> headers) {
                var request = (HttpWebRequest)WebRequest.Create(_url);
                request.Method = "POST";
                request.ContentType = "application/json;charset=utf-8";

                var epochTime = (long)(DateTime.UtcNow-new DateTime(1970, 01, 01)).TotalSeconds;
                var expiry = epochTime+(long)TimeSpan.FromHours(1).TotalSeconds;

                var encodedUrl = WebUtility.UrlEncode(_url);
                var stringToSign = encodedUrl+"\n"+expiry;
                var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_sharedAccessKey));

                var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
                var sasToken = $"SharedAccessSignature sr={encodedUrl}&sig={WebUtility.UrlEncode(signature)}&se={expiry}&skn={_sharedAccessKeyName}";

                request.Headers[HttpRequestHeader.Authorization] = sasToken;

                foreach (var header in headers)
                    request.Headers[header.Key] = header.Value;

                using (var stream = await request.GetRequestStreamAsync())
                using (var streamWriter = new StreamWriter(stream)) {
                    streamWriter.Write(payload);
                    streamWriter.Flush();
                }

                await request.GetResponseAsync();
            }
        }
    }
}

