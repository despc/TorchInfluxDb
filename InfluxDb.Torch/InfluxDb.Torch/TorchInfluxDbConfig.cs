using System.Xml.Serialization;
using InfluxDb.Client;
using InfluxDb.Client.Write;
using Torch;
using Torch.Views;

namespace InfluxDb.Torch
{
    public sealed class TorchInfluxDbConfig :
        ViewModel,
        IInfluxDbAuthConfig,
        InfluxDbWriteClient.IConfig
    {
        const string GroupName = "InfluxDB";

        bool _enable;
        string _hostUrl = "http://localhost:8086";
        string _bucket = "test";
        string _organization = "";
        string _authenticationToken = "";
        float _writeIntervalSecs = 10;
        bool _suppressResponseError;

        [XmlElement("Enable")]
        [Display(Order = 0, Name = "Enable", GroupName = GroupName)]
        public bool Enable
        {
            get => _enable;
            set => SetValue(ref _enable, value);
        }

        [XmlElement("HostUrl")]
        [Display(Order = 2, Name = "Host URL", GroupName = GroupName)]
        public string HostUrl
        {
            get => _hostUrl;
            set => SetValue(ref _hostUrl, value);
        }

        [XmlElement("Organization")]
        [Display(Order = 3, Name = "Organization", GroupName = GroupName)]
        public string Organization
        {
            get => _organization;
            set => SetValue(ref _organization, value);
        }

        [XmlElement("Bucket")]
        [Display(Order = 4, Name = "Bucket Name", GroupName = GroupName)]
        public string Bucket
        {
            get => _bucket;
            set => SetValue(ref _bucket, value);
        }

        [XmlElement("AuthenticationToken")]
        [Display(Order = 5, Name = "Authentication Token (Optional)", GroupName = GroupName)]
        public string AuthenticationToken
        {
            get => _authenticationToken;
            set => SetValue(ref _authenticationToken, value);
        }

        [XmlElement("SuppressResponseError")]
        [Display(Order = 8, Name = "Suppress Response Errors", GroupName = GroupName)]
        public bool SuppressResponseError
        {
            get => _suppressResponseError;
            set => SetValue(ref _suppressResponseError, value);
        }

        [XmlElement("WriteIntervalSecs")]
        [Display(Order = 7, Name = "Throttle Interval (Seconds)", GroupName = GroupName)]
        public float WriteIntervalSecs
        {
            get => _writeIntervalSecs;
            set => SetValue(ref _writeIntervalSecs, value);
        }
    }
}