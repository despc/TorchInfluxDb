TorchInfluxDb
===

[InfluxDB v2.0](https://www.influxdata.com/) integration.

Installation
---

[TorchAPI plugin page](https://torchapi.net/plugins/item/5af3a335-0e25-4ddd-9fc7-6084d7e42e79)

Code Sample
---

```C#
    using InfluxDb.Torch;
    
    TorchInfluxDbWriter
      .Measurement("profiler_block_types")
      .Tag("block_type", blockType.Name)
      .Field("main_ms", deltaTime)
      .Write();
```

Plugin Configuration
---

### Credentials
* `Host URL`
* `Organization Name`
* `Bucket Name`
* `Authentication Token`

### Operation
* `Enable` -- Enables writing.
* `Throttle Interval (Seconds)` -- Sets frequency to write points in queue.
* `Log File Path` -- Sets log file path for `InfluxDb.*` namespace.
* `Suppress Console Output` -- Hides logs from Torch console UI.
* `Enable Logging Trace` -- Enables logging "trace" entries.

Note you don't need to restart Torch.

License
---

MIT License.

See Also
---

* [Torch Monitor Plugin](https://github.com/HnZGaming/TorchMonitor)