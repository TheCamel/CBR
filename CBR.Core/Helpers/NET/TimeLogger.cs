using System;
using System.Diagnostics;

namespace CBR.Core.Helpers
{
	public class TimeLogger : IDisposable
	{
		Stopwatch _stopWatch;
		string _from;
		object _parameters;

		public TimeLogger(string from)
		{
			_from = from;

			if (LogHelper.CanInfo()) 
				_stopWatch = Stopwatch.StartNew();
		}

		public TimeLogger(string from, object pParameters)
		{
			_from = from;
			_parameters = pParameters;

			if (LogHelper.CanInfo()) 
				_stopWatch = Stopwatch.StartNew();
		}
        
        public void Dispose()
        {
			if (LogHelper.CanInfo())
			{
				_stopWatch.Stop();

				LogHelper.Trace(string.Format("{0} execution time {1} milli-seconds = {2} (s)",
					_from, _stopWatch.ElapsedMilliseconds, _stopWatch.ElapsedMilliseconds / 1000));
			}
        }
	}
}
