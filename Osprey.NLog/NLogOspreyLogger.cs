﻿using NLog;
using Osprey.Logging;

namespace Osprey.NLog
{
    public class NLogOspreyLogger : IOspreyLogger
    {
        private readonly ILogger _logger;

        public NLogOspreyLogger()
        {
            _logger = LogManager.GetLogger("Osprey");
        }

        public void Trace(string message) => _logger.Trace(message);

        public void Debug(string message) => _logger.Debug(message);

        public void Info(string message) => _logger.Info(message);

        public void Warn(string message) => _logger.Warn(message);

        public void Error(string message) => _logger.Error(message);
    }
}
