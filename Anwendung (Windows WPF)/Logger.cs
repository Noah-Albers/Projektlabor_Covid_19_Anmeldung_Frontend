using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pl_Covid_19_Anmeldung
{
    class Logger
    {
		// All possible log levels
		// Shall be concatenated using | to log both levels
		public const int
		NONE = 0,               // Nothing will be logged
		DEBUG = 0b1,            // General debug messages shall be logged
		DEBUG_SPECIAL = 0b10,   // Debug message for the current code that gets debugged, will be logged
		INFO = 0b100,           // User-info will get logged
		WARNING = 0b1000,       // Warnings will get logged
		ERROR = 0b10000,        // Critical errors will be logged
		CRITICAL = 0b100000,	// Critical informations that can contain keys and other secrets. ONLY USED WHEN LIVE-DEBUGGING.
		ALL = ~0;               // All of the above will be logged

		// The level that the logger has (Use the above levels o concat them using |)
		private static int LOG_LEVEL_WRITE, LOG_LEVEL_OUTPUT;

		// The output for the file-writer
		private static StreamWriter OPEN_FILE;

		// Source that will be used as a prefix for the logging
		public readonly object Source;

		public Logger(object source)
		{
			this.Source = source;
		}

		/// <summary>
		/// Starts the logger service (Opens the file etc)
		/// </summary>
		/// <param name="logDirectory">The directory where the log-files shall be saved</param>
		/// <param name="logLevelWrite">The log level for all logs that will be written to the log file</param>
		/// <param name="logLevelOutput">The log level for all logs that will be displayed on the console</param>
		/// <exception cref="Exception">See StreamWriter and Directory.CreateDirectory for reference</exception>
		public static void init(string logDirectory, int logLevelWrite, int logLevelOutput)
		{
			LOG_LEVEL_WRITE = logLevelWrite;
			LOG_LEVEL_OUTPUT = logLevelOutput;

			// Ensures that the directory exists
			Directory.CreateDirectory(logDirectory);
		
			// Gets the file-formatter
			string dt = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

			// Opens the file stream
			OPEN_FILE = new StreamWriter(logDirectory + $"Log-{dt}.log",false, Encoding.UTF8);
		}

		/// <summary>
		/// Loges the given message in the log file and in the console
		/// </summary>
		/// <param name="logger">The logger itself to return. Used for convinience</param>
		/// <param name="level">The priority level of the message. Determines if the message will be logged</param>
		/// <param name="prefix">The prefix of the level of the message</param>
		/// <param name="msg">The message that shall be logged</param>
		private static Logger Log(Logger logger,int level, object prefix, object msg)
		{
			// Generates the final message
			string finMsg = $"{prefix}{(logger.Source == null ? "" : ($" [{logger.Source}] "))}{(msg ?? "")}\n";

			// Checks if the log-level for output matches
			if ((LOG_LEVEL_OUTPUT & level) != 0)
			{
				// Outputs the info
				Console.Write(finMsg);
			}

			// Checks if the log-level for file-writing matches
			if ((LOG_LEVEL_WRITE & level) != 0)
			{
				try
				{
					// Outputs the message
					OPEN_FILE.Write(finMsg);
					OPEN_FILE.Flush();
				}
				catch (Exception e)
				{
					// Error, can likely not be handled
					Console.WriteLine("ERROR");
					Console.WriteLine(e);
				}
			}

			return logger;
		}

		public Logger Debug(object msg) => Log(this,DEBUG, "\t[DEBUG] ", msg);
		public Logger Debug_special(object msg) =>Log(this, DEBUG_SPECIAL, "[DEBUG++] ", msg);
		public Logger Info(object msg) =>Log(this, INFO, "[INFO] ", msg);
		public Logger Warn(object msg) =>Log(this, WARNING, "[WARNING] ", msg);
		public Logger Error(object msg) =>Log(this, ERROR, "[ERROR] ", msg);
		public Logger Critical(object msg) => Log(this, CRITICAL, "[CRITICAL] ", msg);

	}
}
