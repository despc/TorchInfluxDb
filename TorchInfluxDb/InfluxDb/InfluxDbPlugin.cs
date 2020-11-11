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

        InfluxDbWriteEndpoints _influxDbWriteEndpoints;
        ThrottledInfluxDbWriteClient _influxDbWriteClient;

        public UserControl GetControl() => _userControl ?? (_userControl = CreateUserControl());

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.ListenOnGameUnloading(() => OnGameUnloading());

            var configFilePath = this.MakeConfigFilePath();
            _config = Persistent<InfluxDbConfig>.Load(configFilePath);
            var config = _config.Data;

            _influxDbWriteEndpoints = new InfluxDbWriteEndpoints(config);
            var influxDbWriteClient = new InfluxDbWriteClient(_influxDbWriteEndpoints, config);

            var interval = TimeSpan.FromSeconds(config.WriteIntervalSecs);
            _influxDbWriteClient = new ThrottledInfluxDbWriteClient(influxDbWriteClient, interval);
            _influxDbWriteClient.StartWriting();

            InfluxDbPointFactory.WriteEndpoints = _influxDbWriteEndpoints;
            InfluxDbPointFactory.WriteClient = _influxDbWriteClient;
            InfluxDbPointFactory.Enabled = config.Enable;

            config.PropertyChanged += (_, __) =>
            {
                InfluxDbPointFactory.Enabled = config.Enable;
            };

            if (config.Enable)
            {
                // test integrity
                InfluxDbPointFactory
                    .Measurement("plugin_init")
                    .Field("message", "successfully initialized")
                    .WriteAsync()
                    .Wait();

                Log.Info("InfluxDB integrity tested");
            }
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
            _influxDbWriteClient?.StopWriting();
            _influxDbWriteEndpoints?.Dispose();
        }
    }
}