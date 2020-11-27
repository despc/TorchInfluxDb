using System.Xml.Serialization;
using InfluxDb.Client;
using InfluxDb.Client.Write;
using Torch;
using Torch.Views;

namespace TorchInfluxDb
{
    public sealed class InfluxDbConfig :
        ViewModel,
        IInfluxDbEndpointConfig,
        InfluxDbWriteClient.IConfig
    {
        const string GroupName = "InfluxDB";

        bool _enable;
        string _hostUrl = "http://localhost:8086";
        string _bucket = "test";
        string _username = "";
        string _password = "";
        float _writeIntervalSecs = 10;
        bool _suppressResponseError;

        [XmlElement("Enable")]
        [Display(Order = 0, Name = "Enable", GroupName = GroupName)]
        public bool Enable
        {
            get => _enable;
            set => SetProperty(ref _enable, value);
        }

        [XmlElement("HostUrl")]
        [Display(Order = 2, Name = "Host URL", GroupName = GroupName)]
        public string HostUrl
        {
            get => _hostUrl;
            set => SetProperty(ref _hostUrl, value);
        }

        [XmlElement("Bucket")]
        [Display(Order = 3, Name = "Bucket Name", GroupName = GroupName)]
        public string Bucket
        {
            get => _bucket;
            set => SetProperty(ref _bucket, value);
        }

        [XmlElement("Username")]
        [Display(Order = 4, Name = "Username (Optional)", GroupName = GroupName)]
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        [XmlElement("Password")]
        [Display(Order = 5, Name = "Password (Optional)", GroupName = GroupName)]
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        [XmlElement("SuppressResponseError")]
        [Display(Order = 8, Name = "Suppress Response Errors", GroupName = GroupName)]
        public bool SuppressResponseError
        {
            get => _suppressResponseError;
            set => SetProperty(ref _suppressResponseError, value);
        }

        [XmlElement("WriteIntervalSecs")]
        [Display(Order = 7, Name = "Throttle Interval (Seconds)", GroupName = GroupName)]
        public float WriteIntervalSecs
        {
            get => _writeIntervalSecs;
            set => SetProperty(ref _writeIntervalSecs, value);
        }

        // ReSharper disable once RedundantAssignment
        void SetProperty<T>(ref T field, T value)
        {
            field = value;
            OnPropertyChanged();
        }
    }
}