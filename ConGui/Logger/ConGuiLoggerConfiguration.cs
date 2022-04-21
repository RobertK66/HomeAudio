using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui.Logger {
    public class ConGuiLoggerConfiguration {

        public LogPanel? LogPanel { get; set; } = null;
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new() {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }
}
