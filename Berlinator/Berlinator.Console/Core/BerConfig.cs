using System.IO;
using Newtonsoft.Json;

namespace Berlinator.Console.Core
{
    public class BerConfig
    {
        public string TelegramToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string StartUrl { get; set; } = string.Empty;

        public static readonly BerConfig Empty = new BerConfig();

        public static BerConfig Load(string fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return Empty;

            var json = File.ReadAllText(fi.FullName);
            return JsonConvert.DeserializeObject<BerConfig>(json);
        }
    }
}
