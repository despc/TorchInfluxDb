TorchInfluxDb
===

[InfluxDB](https://www.influxdata.com/) integration & configuration UI.

Code Sample
---

```C#
    using InfluxDb;
    
    InfluxDbPointFactory
      .Measurement("profiler_block_types")
      .Tag("block_type", blockType.Name)
      .Field("main_ms", deltaTime)
      .Write();
```

Configuration UI
---

* `Enable` -- Sends data to the InfluxDB instance.
* `Host URL` -- URL to the the InfluxDB instance.
* `Organization Name` -- Organization name to use when sending data to the InfluxDB instance.
* `Authentication Token` -- Authentication token for the write access to the InfluxDB instance.
* `Throttle Interval (Seconds)` -- Minimizes the number of HTTP calls using a queue.
* `Suppress Response Errors` -- Ignores network/authentication errors.

You don't need to restart Torch to apply changes.

Specification
---

Based on [the official Line Protocol (v1.8)](https://docs.influxdata.com/influxdb/v1.8/write_protocols/line_protocol_reference/).

License
---

MIT License.

How To Set Up InfluxDB Instance
---

This plugin alone won't store data. You will have to set up an InfluxDB instance to write data to.

* Follow [the official instruction (v1.8)](https://docs.influxdata.com/influxdb/v1.8/introduction/)
* Choose either a self-hosted or cloud-hosted instance (both work).
* `plugin_init.message` will receive test data on plugin init. Make sure to `Enable` and not `Suppress Response Errors`.

How to Set Up Grafana
---

[Grafana](https://grafana.com/) is a web-based observatory tool with a highly customizable graphical dashboard.

[Example dashboard](http://play.se.hnz.asia:3000/d/9UUUl7pGk/hnz-gaalsien?orgId=1&refresh=30s) (if still alive).

* Follow [the official instruction](https://grafana.com/docs/grafana/latest/installation/windows/).
* Add [InfluxDB data source](https://grafana.com/docs/grafana/latest/datasources/influxdb/)
* Wire up your data on the dashboard.
* Optional: Enable [anonymous authentication](https://grafana.com/docs/grafana/latest/auth/overview/#anonymous-authentication) to allow everyone to the dashboard with readonly access.
