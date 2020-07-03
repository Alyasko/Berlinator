using Berlinator.Console.Utils;
using OpenQA.Selenium;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Berlinator.Console.Core
{
    public class BerMonitor
    {
        private readonly IWebDriver _driver;
        private readonly Timer _timer;
        private readonly IMessageAlarmSender _messageAlarm;

        public BerMonitor(IWebDriver driver, IMessageAlarmSender messageAlarm)
        {
            _driver = driver;
            _messageAlarm = messageAlarm;
            //_timer = new Timer(1 * 60 * 1000) {AutoReset = true, Enabled = false};
            _timer = new Timer(10000) {AutoReset = false, Enabled = false};
            _timer.Elapsed += TimerOnElapsed;
        }

        public event EventHandler<FoundTerminEventArgs>? TerminsFoundEventHandler;

        public event EventHandler<EventArgs>? TerminCalendarPageCorruptedEventHandler;

        private void TimerOnElapsed(object sender, ElapsedEventArgs? e)
        {
            Log.Information("Refreshing page.");
            _driver.Navigate().Refresh();

            var head = _driver.FindElements(By.CssSelector(".calendar-head .title"));

            if (head == null || head.Count == 0 || !head.First().Text
                    .Contains("Bitte wählen Sie ein Datum", StringComparison.OrdinalIgnoreCase))
            {
                TerminCalendarPageCorruptedEventHandler?.Invoke(this, EventArgs.Empty);
                Log.Fatal("Page is corrupted");
                return;
            }

            var calendars = _driver.FindElements(By.CssSelector("div.calendar-month-table")).ToList();

            var times = new List<DateTime>();

            foreach (var calendar in calendars)
            {
                var month = "[null]";
                var thead = calendar.FindElements(By.CssSelector("thead .month")).FirstOrDefault();
                if (thead != null)
                {
                    month = thead.Text;
                }

                var buchbars = calendar.FindElements(By.CssSelector("td.nichtbuchbar")).ToList();
                buchbars.ForEach(x =>
                {
                    var dt = DateTime.MinValue;
                    var dtString = $"{x.Text} {month}";
                    if (DateTime.TryParse(dtString, CultureInfo.GetCultureInfo("de"), DateTimeStyles.None, out var dtP))
                        dt = dtP;

                    times.Add(dt);
                });
            }

            if (times.Count > 0)
            {
                TerminsFoundEventHandler?.Invoke(this, new FoundTerminEventArgs()
                {
                    TerminTimes = times,
                });
                _messageAlarm.SendMessage("");

                Log.Fatal($"Found termin(s) at {string.Join(", ", times)}");
            }

            _timer.Start();
        }

        public void StartMonitoring()
        {
            var urlOriginal = _driver.Url;
            var urlBegin = string.Empty;
            var urlNexPage = string.Empty;

            Log.Information("Monitoring started. Url is " + urlOriginal);

            var dtNowMonth = DateTime.UtcNow.StartOfMonth();

            var dtBegin = dtNowMonth.AddDays(-1);
            var dtNextPage = dtNowMonth.AddMonths(2).AddDays(-1);

            var tsBegin = dtBegin.ToUnixTimestamp();
            var tsNextPage = dtNextPage.ToUnixTimestamp();

            var urlTrimmed = urlOriginal.TrimEnd('/', '\\').Trim();
            if (urlTrimmed.EndsWith("termin/day", StringComparison.OrdinalIgnoreCase))
            {
                urlBegin = $"{urlTrimmed}/{tsBegin}";
                urlNexPage = $"{urlTrimmed}/{tsNextPage}";
            }
            else
            {
                Log.Warning(
                    "Please make sure that you went to this page 'https://service.berlin.de/terminvereinbarung/termin/day/', then try again.");
                return;
            }

            TimerOnElapsed(this, null);
        }

        public void StopMonitoring()
        {
            _timer.Stop();
        }
    }
}
