using System;
using System.Windows.Controls;
using InfluxDb.Client.Write;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
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

        public UserControl GetControl() => _config.GetOrCreateUserControl(ref _userControl);

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.ListenOnGameUnloading(() => OnGameUnloading());

            var configFilePath = this.MakeConfigFilePath();
            _config = Persistent<TorchInfluxDbConfig>.Load(configFilePath);
            var config = _config.Data;

            _endpoints = new InfluxDbWriteEndpoints(config);
            _writeClient = new InfluxDbWriteClient(_endpoints, config);

            var interval = TimeSpan.FromSeconds(config.WriteIntervalSecs);
            _throttledWriteClient = new ThrottledInfluxDbWriteClient(_writeClient, interval);

            TorchInfluxDbWriter.WriteEndpoints = _endpoints;
            TorchInfluxDbWriter.WriteClient = _throttledWriteClient;
            TorchInfluxDbWriter.Enabled = config.Enable;

            config.PropertyChanged += (_, __) => OnConfigUpdated();

            if (config.Enable)
            {
                TestIntegrity();
            }

            _throttledWriteClient.SetRunning(config.Enable);
        }

        void OnConfigUpdated()
        {
            var config = _config.Data;

            TorchInfluxDbWriter.Enabled = config.Enable;

            _throttledWriteClient.SetRunning(config.Enable);

            Log.Info($"Update config; database writing enabled: {config.Enable}");
        }

        void TestIntegrity()
        {
            var point = new InfluxDbPoint("plugin_init").Field("message", "successfully initialized");
            _writeClient.WriteAsync(point).Wait();

            Log.Info("InfluxDB integrity tested");
        }

        void OnGameUnloading()
        {
            _config.Dispose();
            _throttledWriteClient?.StopWriting();
            _throttledWriteClient?.Flush();
            _endpoints?.Dispose();
        }
    }
}