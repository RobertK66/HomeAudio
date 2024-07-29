using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;


namespace WinUiHomeAudio.logger {
    public static class WinUiLoggerExtension {
        public static ILoggingBuilder AddWinUiLogger(
            this ILoggingBuilder builder) {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, WinUiLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <WinUiLoggerConfiguration, WinUiLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddWinUiLogger(
            this ILoggingBuilder builder,
            Action<WinUiLoggerConfiguration> configure) {
            builder.AddWinUiLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
