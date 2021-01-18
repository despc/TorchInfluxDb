using System;
using System.Windows.Controls;
using InfluxDb.Client;
using InfluxDb.Client.Write;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Utils.General;
using Utils.Torch;

namespace InfluxDb.Torch
{
    public sealed class TorchInfluxDbPlugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Persistent<TorchInfluxDbConfig> _config;
        UserControl _userControl;

        InfluxDbWriteEndpoints _endpoints;
        InfluxDbWriteClient _writeClient;
        ThrottledInfluxDbWriteClient _throttledWriteClient;
        FileLoggingConfigurator _loggingConfigurator;

        public TorchInfluxDbConfig Config => _config.Data;
        public UserControl GetControl() => _config.GetOrCreateUserControl(ref _userControl);

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.ListenOnGameLoaded(OnGameLoaded);
            this.ListenOnGameUnloading(OnGameUnloading);

            var configFilePath = this.MakeConfigFilePath();
            _config = Persistent<TorchInfluxDbConfig>.Load(configFilePath);
            var config = _config.Data;

            _loggingConfigurator = new FileLoggingConfigurator("InfluxDbLogFile", new[] {"InfluxDb.*"}, TorchInfluxDbConfig.DefaultLogFilePath);
            _loggingConfigurator.Initialize();
            _loggingConfigurator.Configure(Config);

            var auth = new InfluxDbAuth(config);
            _endpoints = new InfluxDbWriteEndpoints(auth);
            _writeClient = new InfluxDbWriteClient(_endpoints);

            var interval = TimeSpan.FromSeconds(config.WriteIntervalSecs);
            _throttledWriteClient = new ThrottledInfluxDbWriteClient(_writeClient, interval);

            TorchInfluxDbWriter.WriteEndpoints = _endpoints;
            TorchInfluxDbWriter.WriteClient = _throttledWriteClient;

            if (config.Enable)
            {
                try
                {
                    Log.Info("Testing database connection...");

                    var point = new InfluxDbPoint("plugin_init").Field("message", "successfully initialized");
                    _writeClient.WriteAsync(point).Wait();

                    Log.Info("Done testing databse connection");
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        void OnGameLoaded()
        {
            Config.PropertyChanged += (_, __) =>
            {
                OnConfigUpdated(Config);
            };

            OnConfigUpdated(Config);
        }

        void OnConfigUpdated(TorchInfluxDbConfig config)
        {
            _loggingConfigurator.Configure(config);

            if (config.Enable != TorchInfluxDbWriter.Enabled)
            {
                TorchInfluxDbWriter.Enabled = config.Enable;
                _throttledWriteClient.SetRunning(config.Enable);

                Log.Info($"Writing enabled: {config.Enable}");
            }
        }

        void OnGameUnloading()
        {
            Log.Info("Unloading...");

            _config.Dispose();
            _throttledWriteClient?.StopWriting();
            _throttledWriteClient?.Flush();
            _endpoints?.Dispose();

            Log.Info("Unloaded");
        }
    }
}