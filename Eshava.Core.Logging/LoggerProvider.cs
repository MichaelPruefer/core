﻿using System.Collections.Generic;
using Eshava.Core.Extensions;
using Eshava.Core.Logging.Interfaces;
using Eshava.Core.Logging.Models;
using Microsoft.Extensions.Logging;

namespace Eshava.Core.Logging
{
	public class LoggerProvider : ILoggerProvider
	{
		private readonly string _version;
		private readonly ILogWriter _logWriter;
		private readonly LogSettings _logSettings;
		private readonly Dictionary<string, LogEngine> _logEngines;
		private static readonly object _lock = new object();

		public LoggerProvider(LogSettings logSettings, string version, ILogWriter logWriter)
		{
			_logEngines = new Dictionary<string, LogEngine>();
			_logSettings = logSettings ?? new LogSettings();
			_version = version;
			_logWriter = logWriter;

			Initialize();
		}

		public ILogger CreateLogger(string categoryName)
		{
			categoryName = categoryName.IsNullOrEmpty() ? LogEngine.DEFAULT : categoryName;

			lock (_lock)
			{
				if (!_logEngines.ContainsKey(categoryName))
				{
					var logLevel = GetLogLevel(categoryName);
					_logEngines.Add(categoryName, GenerateLogEngine(categoryName, logLevel));
				}

				return _logEngines[categoryName];
			}
		}

		public void Dispose()
		{
			_logEngines.Clear();
		}

		private LogEngine GenerateLogEngine(string categoryName, LogLevel logLevel)
		{
			return new LogEngine(categoryName, _version, logLevel, _logWriter);
		}

		private LogLevel GetLogLevel(string categoryName)
		{
			return _logSettings.LogLevel.ContainsKey(categoryName) ? _logSettings.LogLevel[categoryName] : _logSettings.LogLevel[LogEngine.DEFAULT];
		}

		private void Initialize()
		{
			if (!_logSettings.LogLevel.ContainsKey(LogEngine.DEFAULT))
			{
				_logSettings.LogLevel.Add(LogEngine.DEFAULT, LogLevel.Debug);
			}
		}
	}
}