﻿using Microsoft.Extensions.Logging;
using System;

namespace WinUiHomeAudio.logger {
    public class WinUiLogger : ILogger {
        private readonly string _name;
        private readonly Func<WinUiLoggerConfiguration> _getCurrentConfig;

        public WinUiLogger(
            string name,
            Func<WinUiLoggerConfiguration> getCurrentConfig) =>
            (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

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

            WinUiLoggerConfiguration config = _getCurrentConfig();

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
            //string threadTxt = Thread.CurrentThread.Name;
            ////if (Thread.CurrentThread.IsThreadPoolThread) {
            ////    threadTxt = "TP";
            ////} 
            ////int unmanagedId = AppDomain.GetCurrentThreadId();
            ////ProcessThread? myThread = (from ProcessThread entry in Process.GetCurrentProcess().Threads
            ////                          where entry.Id == unmanagedId
            ////                          select entry).FirstOrDefault();
            //// threadTxt += "-" + Environment.CurrentManagedThreadId.ToString() + " " + unmanagedId.ToString() + ((myThread?.ToString())??"u")  + " ";

            //threadTxt += "-" + Environment.CurrentManagedThreadId.ToString() + " ";

            // threadTxt += Thread.CurrentThread.ExecutionContext?.GetType();
            // threadTxt += "xx";
            string eventTxt = $"[{logLevel}]";

            if (!String.IsNullOrWhiteSpace(eventId.Name)) {
                eventTxt += $", [{eventId.Name}]";
            }
            var mymessage = formatter(state, exception);
            config.LoggerVm?.Add($"{eventTxt}: {_name} - {mymessage}");

            //}
        }
    }
}
