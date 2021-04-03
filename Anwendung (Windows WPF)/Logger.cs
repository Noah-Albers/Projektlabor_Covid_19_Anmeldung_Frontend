using System;
using System.Collections.Generic;
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

		// The level that the logger has (Used the above levels and concat them using | )
		private readonly int logLevel;

		public Logger(int logLevel)
		{
			this.logLevel = logLevel;
		}

		/// <summary>
		/// The actual method to log an occurrence.
		/// Used by all others to log.
		/// </summary>
		/// <param name="level">the level that gets logged (Used to verify, that the message should be outputted)</param>
		/// <param name="prefix">prefix the prefix that can be printed before the message (Visual distinction)</param>
		/// <param name="msg">msg the actual message that shall be logged</param>
		private void Log(int level, string prefix, object msg)
		{
			// Checks if the log-level does not match
			if ((this.logLevel & level) == 0)
				return;

			// Generates the final message
			string finalMessage = prefix + msg.ToString();

			// Outputs the info
			Console.WriteLine(finalMessage);
		}

		public void Debug(object msg) =>this.Log(DEBUG, "\t[DEBUG] ", msg);
		public void Debug_special(object msg) =>this.Log(DEBUG_SPECIAL, "[DEBUG++] ", msg);
		public void Info(object msg) =>this.Log(INFO, "[INFO] ", msg);
		public void Warn(object msg) =>this.Log(WARNING, "[WARNING] ", msg);
		public void Error(object msg) =>this.Log(ERROR, "[ERROR] ", msg);
		public void Critical(object msg) => this.Log(CRITICAL, "[CRITICAL] ", msg);

	}
}
