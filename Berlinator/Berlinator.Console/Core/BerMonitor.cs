using Berlinator.Console.Utils;
using OpenQA.Selenium;
using Serilog;
using System;
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

        public BerMonitor(IWebDriver driver)
        {
            _driver = driver;
            _timer = new Timer(5 * 60 * 1000) {AutoReset = true, Enabled = false};
            _timer.Elapsed += TimerOnElapsed;
        }

        public event EventHandler<FoundTerminEventArgs>? TerminFoundEventHandler;

        public event EventHandler<EventArgs>? TerminCalendarPageCorruptedEventHandler;

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
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

            var buchbar = _driver.FindElements(By.CssSelector("td.buchbar")).FirstOrDefault();

            if (buchbar != null)
            {
                var utcFound = DateTime.MinValue;

                TerminFoundEventHandler?.Invoke(this, new FoundTerminEventArgs()
                {
                    UtcFreeTermin = utcFound,
                    Payload = buchbar.Text
                });

                Log.Fatal($"Found termin at {utcFound.ToShortDateString()}");
            }
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

            _timer.Start();
        }

        public void StopMonitoring()
        {
            _timer.Stop();
        }
    }
}
