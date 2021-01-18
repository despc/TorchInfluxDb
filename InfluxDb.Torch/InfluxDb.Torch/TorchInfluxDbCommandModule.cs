using System;
using Torch.Commands;
using Torch.Commands.Permissions;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;
using VRageMath;

namespace InfluxDb.Torch
{
    [Category("influxdb")]
    public sealed class TorchInfluxDbCommandModule : CommandModule
    {
        TorchInfluxDbPlugin Plugin => (TorchInfluxDbPlugin) Context.Plugin;

        [Command("w", "Tries to write a raw line to the integrated InfluxDB instance.")]
        [Permission(MyPromoteLevel.Admin)]
        public void WriteLine() => this.CatchAndReport(async () =>
        {
            var line = Context.RawArgs;
            line.ThrowIfNullOrEmpty(nameof(line));

            Context.Respond($"Writing line: '{line}'");

            try
            {
                await TorchInfluxDbWriter.WriteLineAsync(line);
            }
            catch (Exception e)
            {
                Context.Respond($"{e.Message}", Color.Red);
                return;
            }

            Context.Respond("Done writing line");
        });

        [Command("enable")]
        [Permission(MyPromoteLevel.Admin)]
        public void Enable() => this.CatchAndReport(() =>
        {
            Plugin.Config.Enable = true;
        });

        [Command("disable")]
        [Permission(MyPromoteLevel.Admin)]
        public void Disable() => this.CatchAndReport(() =>
        {
            Plugin.Config.Enable = false;
        });

        [Command("host")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetHostUrl(string hostUrl) => this.CatchAndReport(() =>
        {
            Plugin.Config.HostUrl = hostUrl;
        });

        [Command("organization")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetOrganization(string organization) => this.CatchAndReport(() =>
        {
            Plugin.Config.Organization = organization;
        });

        [Command("bucket")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetBucket(string bucket) => this.CatchAndReport(() =>
        {
            Plugin.Config.Bucket = bucket;
        });

        [Command("token")]
        [Permission(MyPromoteLevel.Admin)]
        public void SetToken(string token) => this.CatchAndReport(() =>
        {
            Plugin.Config.AuthenticationToken = token;
        });
    }
}