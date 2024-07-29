using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace WinUiHomeAudio.logger {
    //[UnsupportedOSPlatform("browser")]
    //[ProviderAlias("ConGuiConsole")]
    public sealed class WinUiLoggerProvider : ILoggerProvider {
        private readonly IDisposable? _onChangeToken;
        private WinUiLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, WinUiLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public WinUiLoggerProvider(
            Microsoft.Extensions.Options.IOptionsMonitor<WinUiLoggerConfiguration> config) {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange((con, x) => _currentConfig = con);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new WinUiLogger(name, GetCurrentConfig));

        private WinUiLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose() {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
