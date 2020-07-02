using System;
using System.Linq;
using System.Threading;
using System.Timers;
using Berlinator.Console.Utils;
using OpenQA.Selenium;
using Serilog;
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
            _timer = new Timer(10000) {AutoReset = true, Enabled = false};
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Log.Information("Refreshing page.");
            _driver.Navigate().Refresh();

            var buched = _driver.FindElements(By.CssSelector("td.buchbar")).FirstOrDefault();
            if (buched != null)
            {
                Log.Fatal("FOUND");
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

            //Log.Information($"Pages to navigate:{Environment.NewLine}{urlBegin}{Environment.NewLine}{urlNexPage}");

            //_driver.FindElements(By.CssSelector(".controll .next")).FirstOrDefault();

            //_driver.FindElements(By.CssSelector(".control>.next")).FirstOrDefault()?.Click();

            _timer.Start();
        }

        public void StopMonitoring()
        {
            _timer.Stop();
        }
    }
}
