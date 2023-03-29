﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConGui.Logger {
    public class ConGuiLogger :ILogger {
        private readonly string _name;
        private readonly Func<ConGuiLoggerConfiguration> _getCurrentConfig;

        public ConGuiLogger(
            string name,
            Func<ConGuiLoggerConfiguration> getCurrentConfig) =>
            (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public IDisposable? BeginScope<TState>(TState? state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            true;
 
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

            //LogLevel configured = Microsoft.Extensions.Logging.LogLevel.Information;

            //if (config.LogLevel.ContainsKey("Default")) {
            //    configured = config.LogLevel["Default"];
            //}
            //foreach (var key in config.LogLevel.Keys) {
            //    if (key.StartsWith(_name)) {                // TODO: make correct hirachy!!!!
            //        configured = config.LogLevel[key];
            //    }
            //}    

            //if (configured <= logLevel) { 
            string? threadTxt = Thread.CurrentThread.Name;
            if (Thread.CurrentThread.IsThreadPoolThread) {
                threadTxt = "TP";
            } 
            threadTxt += "-" + Thread.CurrentThread.ManagedThreadId.ToString() + " ";
            //threadTxt += Thread.CurrentThread.ExecutionContext?.GetType();

            string eventTxt = $"[{threadTxt}, {logLevel.ToString().Substring(0,1)}]";
            
            if (!String.IsNullOrWhiteSpace(eventId.Name)) {
                eventTxt += $", [{eventId.Name}]";
            }
            config.LogPanel?.Add($"{eventTxt}: {_name} - {formatter(state, exception)}");

            //}
        }
    }
}
