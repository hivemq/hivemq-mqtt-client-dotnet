# Benchmarks

The benchmarks are built with [BenchmarkDotNet](https://benchmarkdotnet.org) and can be run with:

`dotnet run ClientBenchmarkApp.csproj -c Release`

# Results - Mar 21, 2024

With release [v0.11.0](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases/tag/v0.11.0) there was a big performance improvement.  All messaging performance was improved but particularly publishing a QoS level 2 message went from ~206ms down to ~1.6ms.

##  Previous Performance

| Method                                    | Mean         | Error       | StdDev      | Median         |
|------------------------------------------ |-------------:|------------:|------------:|---------------:|
| 'Publish a QoS 0 message' |     390.8 us |  1,842.5 us |  1,218.7 us |       5.646 us |
| 'Publish a QoS 1 message' | 103,722.8 us |  4,330.0 us |  2,864.1 us | 103,536.375 us |
| 'Publish a QoS 2 message' | 202,367.9 us | 26,562.9 us | 17,569.7 us | 206,959.834 us |

## First Pass Refactor Performance

| Method                                    | Mean       | Error      | StdDev     | Median       |
|------------------------------------------ |-----------:|-----------:|-----------:|-------------:|
| 'Publish a QoS 0 message' |   401.9 us | 1,876.3 us | 1,241.0 us |     9.250 us |
| 'Publish a QoS 1 message' | 2,140.0 us | 3,568.2 us | 2,360.1 us | 1,324.251 us |
| 'Publish a QoS 2 message' | 4,217.2 us | 5,803.7 us | 3,838.8 us | 2,569.166 us |

## Final Refactor Performance Results (for now 👻)

| Method                                    | Mean        | Error     | StdDev      | Median       |
|------------------------------------------ |------------:|----------:|------------:|-------------:|
| 'Publish a QoS 0 message' |    47.11 us | 139.47 us |   411.23 us |     4.875 us |
| 'Publish a QoS 1 message' | 1,210.71 us | 508.64 us | 1,499.75 us |   790.645 us |
| 'Publish a QoS 2 message' | 2,080.46 us | 591.38 us | 1,743.71 us | 1,653.083 us |
