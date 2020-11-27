using System.Xml.Serialization;
using InfluxDb.Client;
using InfluxDb.Client.Write;
using Torch;
using Torch.Views;

namespace InfluxDb
{
    public sealed class InfluxDbConfig :
        ViewModel,
        InfluxDbWriteEndpoints.IConfig,
        InfluxDbWriteClient.IConfig
    {
        const string GroupName = "InfluxDB";

        bool _enable;
        float _writeIntervalSecs = 10;
        string _hostUrl = "http://localhost:8086";
        string _organization = "Some Organization";
        string _bucket = "test";
        string _authenticationToken = "";
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

        [XmlElement("Organization")]
        [Display(Order = 3, Name = "Organization Name", GroupName = GroupName)]
        public string Organization
        {
            get => _organization;
            set => SetProperty(ref _organization, value);
        }

        [XmlElement("Bucket")]
        [Display(Order = 4, Name = "Bucket Name", GroupName = GroupName)]
        public string Bucket
        {
            get => _bucket;
            set => SetProperty(ref _bucket, value);
        }

        [XmlElement("AuthenticationToken")]
        [Display(Order = 5, Name = "Authentication Token", GroupName = GroupName)]
        public string AuthenticationToken
        {
            get => _authenticationToken;
            set => SetProperty(ref _authenticationToken, value);
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