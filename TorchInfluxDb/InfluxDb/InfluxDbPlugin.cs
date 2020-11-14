using System;
using System.Windows.Controls;
using InfluxDb.Client;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Views;
using TorchUtils;

namespace InfluxDb
{
    public sealed class InfluxDbPlugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Persistent<InfluxDbConfig> _config;
        UserControl _userControl;

        InfluxDbWriteEndpoints _endpoints;
        ThrottledInfluxDbWriteClient _throttledWriteClient;

        public UserControl GetControl() => _userControl ?? (_userControl = CreateUserControl());

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.ListenOnGameUnloading(() => OnGameUnloading());

            var configFilePath = this.MakeConfigFilePath();
            _config = Persistent<InfluxDbConfig>.Load(configFilePath);
            var config = _config.Data;

            _endpoints = new InfluxDbWriteEndpoints(config);
            var writeClient = new InfluxDbWriteClient(_endpoints, config);

            var interval = TimeSpan.FromSeconds(config.WriteIntervalSecs);
            _throttledWriteClient = new ThrottledInfluxDbWriteClient(writeClient, interval);

            InfluxDbPointFactory.WriteEndpoints = _endpoints;
            InfluxDbPointFactory.WriteClient = _throttledWriteClient;
            InfluxDbPointFactory.Enabled = config.Enable;

            config.PropertyChanged += (_, __) =>
            {
                _throttledWriteClient.SetRunning(config.Enable);
                InfluxDbPointFactory.Enabled = config.Enable;
            };

            if (config.Enable)
            {
                // test integrity
                var point = new InfluxDbPoint("plugin_init").Field("message", "successfully initialized");
                writeClient.WriteAsync(point).Wait();

                Log.Info("InfluxDB integrity tested");
            }
            
            _throttledWriteClient.SetRunning(config.Enable);
        }

        UserControl CreateUserControl()
        {
            return new PropertyGrid
            {
                DataContext = _config.Data,
            };
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