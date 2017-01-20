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
		public enum logType { dev = 0, debug, info, warm, error, fatal }

		public enum logDisplay { normal, show, hide }

		public struct Log
		{
			public string text;
			public logType type;
			public logDisplay display;

			public Log(string p1, logType p2, logDisplay p3 = logDisplay.normal)
			{
				text = p1;
				type = p2;
				display = p3;
			}
		}

		List<Log> toWriteLogs = new List<Log>();
		string logPath;
		ConsoleColor[] logBackColor;
		ConsoleColor[] logForeColor;
		Thread Updater;
		logType logLevel = logType.info;
		bool _run = true;
		public bool run { get { return _run; } }
		static bool _debug = false;
		static bool _dev = false;

		List<string> _outList;
		public List<string> outList { get { return _outList; } }

		string _outText;
		public string outText { get { return _outText; } }

		public struct Outputs
		{
			public bool File;
			public bool Console;
			public bool ConsoleIO;
			public bool List;
			public bool Text;

			public Outputs(bool file = true, bool console = false, bool consoleIO = true, bool stringList = false, bool text = false )
			{
				File = file;
				Console = console;
				ConsoleIO = consoleIO;
				List = stringList;
				Text = text;
			}
		}

		Outputs outputs;

		/// <summary>
		/// Create log file and start logger thread
		/// </summary>
		/// <param name="LogPath">Absolute path to logs directory</param>
		public void Initialise(string LogPath, ConsoleColor[] backColor, ConsoleColor[] foreColor, logType LogLevel = logType.info, bool debug = false, bool dev = false, Outputs output = new Outputs())
		{
			outputs = output;
			logPath = LogPath;
			logBackColor = backColor;
			logForeColor = foreColor;
			logLevel = LogLevel;
			_debug = debug;
			_dev = dev;

			if (outputs.File)
			{
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
			}

			if (outputs.Console || outputs.ConsoleIO)
			{
				Console.BackgroundColor = logBackColor[0];
				Console.ForegroundColor = logForeColor[0];
				Console.Clear();
			}

			if (outputs.Text)
			{
				_outText = "";
			}

			if (outputs.List)
			{
				_outList = new List<string>();
			}

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
			Write("Change to " + logLevel, logType.info, logDisplay.show);
		}

		/// <summary>
		/// Add log to log pile
		/// </summary>
		/// <param name="text">Log text</param>
		/// <param name="type">Log status</param>
		/// <param name="console">Server display modifier</param>
		public void Write(string text, logType type, logDisplay console = logDisplay.normal)
		{
			Write(new Log(text, type, console));
		}

		/// <summary>
		/// Add log to log pile
		/// </summary>
		/// <param name="log">Log struct</param>
		void Write(Log log)
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

					if (log.type >= logLevel || log.display == logDisplay.show)
					{
						string datetime = DateTime.UtcNow.ToString("[yyyy-MM-dd]", CultureInfo.InvariantCulture);
						string text = datetime + " [" + log.type.ToString().ToUpper() + "]: " + log.text;
						string textfull = datetime + ": " + log.text;

						if (outputs.File && log.type >= logLevel)
						{
							File.AppendAllText(logPath, textfull + Environment.NewLine);
						}

						if (outputs.Console)
						{
							Console.ResetColor();
							Console.ForegroundColor = logForeColor[(int)log.type];
							Console.BackgroundColor = logBackColor[(int)log.type];
							Console.WriteLine(text);
						}

						if (outputs.ConsoleIO)
						{
							ConsoleIO.Write(new ColorStrings(new ColorString(text, logForeColor[(int)log.type], logBackColor[(int)log.type])));
						}

						if (outputs.List)
						{
							_outList.Add(textfull);
						}

						if (outputs.Text)
						{
							_outText += textfull + Environment.NewLine;
						}
					}
					
					toWriteLogs.Remove(log);
				}
			}
		}
	}
}