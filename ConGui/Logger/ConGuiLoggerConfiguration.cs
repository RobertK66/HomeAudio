﻿using ConGui.Controls;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConGui.Logger {
    public class ConGuiLoggerConfiguration {

        public LogPanel? LogPanel { get; set; } = null;
        //public int EventId { get; set; }

        // This dictionary is only used if nothing is configured at all somewhere else (e.g. in appsettings.json)
        public Dictionary<string, LogLevel> LogLevel { get; set; } = new() {
            ["Default"] = Microsoft.Extensions.Logging.LogLevel.Information,
        };
    }
}
