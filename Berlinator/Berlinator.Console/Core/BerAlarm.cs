using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Berlinator.Console.Core
{
    public class BerAlarm : IMessageAlarmSender
    {
        private readonly TelegramBotClient? _botClient;
        private readonly BerConfig _berConfig;

        private bool _isBotEnabled = false;

        public BerAlarm(BerConfig berConfig)
        {
            _berConfig = berConfig;

            if (!string.IsNullOrWhiteSpace(berConfig.TelegramToken))
            {
                try
                {
                    _botClient = new TelegramBotClient(berConfig.TelegramToken);
                    _isBotEnabled = true;
                }
                catch
                {
                    _isBotEnabled = false;
                }
            }
        }

        public async Task Alarm(string urlStart, List<DateTime> terminTimes, string payload)
        {
            if(!_isBotEnabled || _botClient == null)
                return;

            var r = await _botClient.SendTextMessageAsync(_berConfig.ChatId, $"Found free termin(s) on:\n{string.Join("\n", terminTimes.Select(x => x.ToShortDateString()))}. \nPlease follow the url: {urlStart}");
        }

        public async Task PageCorrupted()
        {
            if (!_isBotEnabled || _botClient == null)
                return;

            await _botClient.SendTextMessageAsync(_berConfig.ChatId, $"Page has been corrupted.");
        }

        public async Task SendMessage(string text)
        {
            if (!_isBotEnabled || _botClient == null)
                return;

            //var updates = await _botClient.GetUpdatesAsync();
            var r = await _botClient.SendTextMessageAsync(_berConfig.ChatId, text);
        }
    }
}
