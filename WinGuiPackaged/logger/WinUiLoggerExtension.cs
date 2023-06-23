using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WinGuiPackaged.logger {
 
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
