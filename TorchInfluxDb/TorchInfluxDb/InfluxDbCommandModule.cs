using System;
using Torch.Commands;
using Torch.Commands.Permissions;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;
using VRageMath;

namespace TorchInfluxDb
{
    [Category(Category)]
    public sealed class InfluxDbCommandModule : CommandModule
    {
        const string Category = "idb";
        const string Cmd_Write = "w";

        [Command(Cmd_Write, "Tries to write a raw line to the integrated InfluxDB instance.")]
        [Permission(MyPromoteLevel.Admin)]
        public void WriteLine() => this.CatchAndReport(async () =>
        {
            var line = Context.RawArgs;
            line.ThrowIfNullOrEmpty(nameof(line));

            Context.Respond($"Writing line: '{line}'");

            try
            {
                await InfluxDbPointFactory.WriteLineAsync(line);
            }
            catch (Exception e)
            {
                Context.Respond($"{e.Message}", Color.Red);
                return;
            }

            Context.Respond("Done writing line");
        });
    }
}