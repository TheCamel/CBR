using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Helpers
{
	public class SpecialFolderPatternConverter : log4net.Util.PatternConverter
	{
		protected override void Convert(System.IO.TextWriter writer, object state)
		{
			Environment.SpecialFolder specialFolder = Environment.SpecialFolder.MyDocuments;
			Enum.TryParse<Environment.SpecialFolder>(base.Option, out specialFolder);
			writer.Write(Environment.GetFolderPath(specialFolder));
		}
	}

	public class LogHelper
	{
		#region ------------------logger------------------
		/// <summary>
		/// Logger of log4Net
		/// </summary>
		private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		static public void Manage(string from, Exception error)
		{
			if (_logger.IsErrorEnabled)
				_logger.Error(from, error);
		}

		static public bool CanDebug()
		{
			return _logger.IsDebugEnabled;
		}

		static public void Begin(string from)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug(string.Format("BEGIN FROM {0}", from));
		}

		static public void Begin(string from, string format, params object[] args)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug(string.Format("BEGIN FROM {0}", from) + string.Format(format, args));
		}

		static public void End(string from)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("END FROM " + from);
		}

		static public void End(string from, string format, params object[] args)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug(string.Format("END FROM {0}", from) + string.Format(format, args));
		}

		static public bool CanInfo()
		{
			return _logger.IsInfoEnabled;
		}

		static public void Trace(string data)
		{
			if (_logger.IsInfoEnabled)
				_logger.Info(data);
		}

		static public void Trace(string format, params object[] args)
		{
			if (_logger.IsInfoEnabled)
				_logger.InfoFormat(format, args);
		}
	}
}
