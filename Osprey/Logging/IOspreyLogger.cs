using System;
using System.Collections.Generic;
using System.Text;

namespace Osprey.Logging
{
    public interface IOspreyLogger
    {
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }

    public class ConsoleOspreyLogger : IOspreyLogger
    {
        public void Trace(string message) => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | TRACE | " + message);

        public void Debug(string message) => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | DEBUG | " + message);

        public void Info(string message) => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | INFO  | " + message);

        public void Warn(string message) => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | WARN  | " + message);

        public void Error(string message) => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ERROR | " + message);
    }
}
