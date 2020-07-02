﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Berlinator.Console.Core
{
    public class BerApp
    {
        private readonly IWebDriver _driver;
        private readonly BerMonitor _berMonitor;
        private readonly BerConfig _berConfig;

        public BerApp(BerConfig berConfig)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Berlinator started!");

            if (berConfig == BerConfig.Empty)
                Log.Warning("Configuration is empty.");

            _berConfig = berConfig;

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = false;
            service.HideCommandPromptWindow = true;
            service.EnableVerboseLogging = false;
            _driver = new ChromeDriver(service);

            _berMonitor = new BerMonitor(_driver);
        }

        public Task Run()
        {
            //_driver.Url = "https://service.berlin.de/";
            _driver.Url = "https://service.berlin.de/dienstleistung/327537/";
            _driver.Navigate();

            return Task.CompletedTask;
        }

        public void OnExit(object? sender, EventArgs e)
        {
            Log.Information("Closing web browser...");

            _driver.Quit();

            Log.Information("Web browser closed.");
        }

        public void HandleCommand(string command)
        {
            if (command.Equals("start", StringComparison.OrdinalIgnoreCase))
                _berMonitor.StartMonitoring();
            if (command.Equals("stop", StringComparison.OrdinalIgnoreCase))
                _berMonitor.StopMonitoring();
        }
    }
}
