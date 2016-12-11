using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace MyCommon
{
    public class Logger
    {
        public enum logType { dev, debug, info, warm, error, fatal }

        public enum logConsole { normal, show, hide }

        public struct Log
        {
            public string text;
            public logType type;
            public logConsole console;

            public Log(string p1, logType p2, logConsole p3 = logConsole.normal)
            {
                text = p1;
                type = p2;
                console = p3;
            }
        }

        private List<Log> toWriteLogs = new List<Log>();
        private string logPath;
        private ConsoleColor[] logBackColor;
        private ConsoleColor[] logForeColor;
        private Thread Updater;
        private logType logLevel = logType.info;
        private bool _run = true;
        public bool run { get { return _run; } }
        private static bool _debug = false;
        private static bool _dev = false;
        private bool haveConsole = false;

        /// <summary>
        /// Create log file and start logger thread
        /// </summary>
        /// <param name="LogPath">Absolute path to logs directory</param>
        public void Initialise(string LogPath, ConsoleColor[] backColor, ConsoleColor[] foreColor, logType LogLevel, bool debug, bool dev, bool haveconsole = true)
        {
            haveConsole = haveconsole;
            logPath = LogPath;
            logBackColor = backColor;
            logForeColor = foreColor;
            logLevel = LogLevel;
            _debug = debug;
            _dev = dev;
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
                Write("Log Directory Created", logType.info);
            }
            else
            {
                //Sort old logs
                string[] files = Directory.GetFiles(logPath);

                foreach (string file in files)
                {
                    if (Path.GetExtension(file) == ".log")
                    {
                        string name = Path.GetFileName(file);
                        name = name.Substring(0, Math.Min(name.Length, 10));
                        if (name.Length == 10)
                        {
                            if (name != DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                            {
                                int y;
                                int m;
                                int d;

                                if (int.TryParse(new string(name.Take(4).ToArray()), out y) && int.TryParse(new string(name.Skip(5).Take(2).ToArray()), out m) && int.TryParse(new string(name.Skip(8).Take(2).ToArray()), out d))
                                {
                                    if (!Directory.Exists(logPath + "/" + y + "/" + m + "/" + d))
                                    {
                                        Directory.CreateDirectory(logPath + "/" + y + "/" + m + "/" + d);
                                    }
                                    File.Move(file, logPath + "/" + y + "/" + m + "/" + d + "/" + Path.GetFileName(file));
                                }
                            }
                        }
                    }
                }
            }

            int i = 0;
            while (File.Exists(logPath + "/" + DateTime.UtcNow.ToString("yyyy-MM-dd-", CultureInfo.InvariantCulture) + i + ".log")) { i++; }
            logPath = logPath + "/" + DateTime.UtcNow.ToString("yyyy-MM-dd-", CultureInfo.InvariantCulture) + i + ".log";
            Write("Log path:" + logPath, logType.debug);
            Updater = new Thread(new ThreadStart(UpdaterLoop));
            Updater.Start();
        }

        public void Join()
        {
            _run = false;
            Updater.Join();
        }

        public void ChangeLevel(logType level)
        {
            logLevel = level;
            Write("Change to " + logLevel.ToString(), logType.info, logConsole.show);
        }

        /// <summary>
        /// Add log to log pile
        /// </summary>
        /// <param name="text">Log text</param>
        /// <param name="type">Log status</param>
        /// <param name="console">Server display modifier</param>
        public void Write(string text, logType type, logConsole console = logConsole.normal)
        {
            Write(new Log(text, type, console));
        }

        /// <summary>
        /// Add log to log pile
        /// </summary>
        /// <param name="log">Log struct</param>
        private void Write(Log log)
        {
            if (_debug || _dev)
            {
                //Add Source Method
                log.text = "[" + new StackTrace().GetFrame(2).GetMethod().Name + "]: " + log.text;
            }
            toWriteLogs.Add(log);
        }

        /// <summary>
        /// Write log pile to logfile and console
        /// </summary>
        public void UpdaterLoop()
        {
            while (_run || toWriteLogs.Count > 0)
            {
                while (toWriteLogs.Count > 0)
                {
                    Log log = toWriteLogs[0]; //Saved log -> any lock need

                    if (log.type >= logLevel)
                    {
                        File.AppendAllText(logPath, DateTime.UtcNow.ToString("[yyyy-MM-dd]", CultureInfo.InvariantCulture) + " [" + log.type.ToString().ToUpper() + "]: " + log.text + Environment.NewLine);
                        if (log.console != logConsole.hide)
                        {
                            ConsoleWrite(log);
                        }
                    }
                    else
                    {
                        if (log.console == logConsole.show)
                        {
                            ConsoleWrite(log);
                        }
                    }
                    toWriteLogs.Remove(log);
                }
            }
        }

        private void ConsoleWrite(Log log)
        {
            if (haveConsole)
                Console.Write(new ColorStrings(new ColorString(DateTime.UtcNow.ToString("[yyyy-MM-dd]", CultureInfo.InvariantCulture) + ": " + log.text, logForeColor[(int)log.type], logBackColor[(int)log.type])));
        }
    }
}