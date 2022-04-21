using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui.Logger {
    public class ConGuiLogger :ILogger {
        private readonly string _name;
        private readonly Func<ConGuiLoggerConfiguration> _getCurrentConfig;

        public ConGuiLogger(
            string name,
            Func<ConGuiLoggerConfiguration> getCurrentConfig) =>
            (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            _getCurrentConfig().LogLevels.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }

            ConGuiLoggerConfiguration config = _getCurrentConfig();
            //if (config.EventId == 0 || config.EventId == eventId.Id) {
                config.LogPanel?.Add($"[{eventId.Id,2}: {logLevel,-12}] {_name} - {formatter(state, exception)}");
                //ConsoleColor originalColor = Console.ForegroundColor;

                //Console.ForegroundColor = config.LogLevels[logLevel];
                //Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

                //Console.ForegroundColor = originalColor;
                //Console.Write($"     {_name} - ");

                //Console.ForegroundColor = config.LogLevels[logLevel];
                //Console.Write($"{formatter(state, exception)}");

                //Console.ForegroundColor = originalColor;
                //Console.WriteLine();
            //}
        }
    }
}
