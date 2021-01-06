using NLog;
using NLog.Config;
using NLog.Targets;
using Utils.Torch;

namespace InfluxDb.Torch
{
    public class TorchInfluxDbLoggingConfigurator
    {
        public interface IConfig
        {
            bool SuppressWpfOutput { get; }
            bool EnableLoggingTrace { get; }
            string LogFilePath { get; }
        }

        const string TargetName = "InfluxDbLogFile";
        const string NamePattern = "InfluxDb.*";
        public const string DefaultLogFilePath = "Logs/InfluxDb-${shortdate}.log";

        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly FileTarget _target;
        readonly LoggingRule _rule;

        public TorchInfluxDbLoggingConfigurator()
        {
            _target = new FileTarget
            {
                Name = TargetName,
            };

            _rule = new LoggingRule
            {
                LoggerNamePattern = NamePattern,
                Final = true,
            };

            // default config
            _target.FileName = DefaultLogFilePath;
            _rule.Targets.Add(_target);
            _rule.Targets.Add(TorchUtils.GetWpfTarget());
            _rule.EnableLoggingForLevels(LogLevel.Info, LogLevel.Off);
        }

        public void Initialize()
        {
            LogManager.Configuration.AddTarget(_target);
            LogManager.Configuration.LoggingRules.Insert(0, _rule);
            LogManager.Configuration.Reload();
        }

        public void Reconfigure(IConfig config)
        {
            _target.FileName = config.LogFilePath;

            _rule.Targets.Clear();
            _rule.Targets.Add(_target);

            if (!config.SuppressWpfOutput)
            {
                _rule.Targets.Add(TorchUtils.GetWpfTarget());
            }

            var minLevel = config.EnableLoggingTrace ? LogLevel.Trace : LogLevel.Info;
            _rule.DisableLoggingForLevel(LogLevel.Trace);
            _rule.DisableLoggingForLevel(LogLevel.Debug);
            _rule.EnableLoggingForLevels(minLevel, LogLevel.Off);

            LogManager.ReconfigExistingLoggers();
            
            Log.Info($"Reconfigured; wpf={!config.SuppressWpfOutput}, minlevel={minLevel}");
        }
    }
}