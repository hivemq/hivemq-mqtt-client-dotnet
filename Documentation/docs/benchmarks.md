---
sidebar_position: 90
---

# Benchmarks

The HiveMQtt GitHub repository provides benchmarks built using [BenchmarkDotNet](https://benchmarkdotnet.org), a .NET library for benchmarking. These benchmarks measure the performance of various messaging operations against any MQTT broker.

## Running Benchmarks

To run the benchmarks yourself, execute:

```bash
cd Benchmarks/ClientBenchmarkApp
dotnet run ClientBenchmarkApp.csproj -c Release
```

## Results

The benchmarks provide insights into the performance of different messaging methods under various scenarios. Results below are from a local MacBook Pro against a HiveMQ v4 broker running in a Docker container over localhost.

## Legend

| Term | Description |
|------|-------------|
| **Mean** | Arithmetic mean of all measurements |
| **Error** | Half of 99.9% confidence interval |
| **StdDev** | Standard deviation of all measurements |
| **Median** | Value separating the higher half of all measurements (50th percentile) |
| **1 μs** | 1 Microsecond (0.000001 sec) |
| **1 ms** | 1,000 Microseconds |

## November 26, 2025 (Latest)

### Performance Improvements

Significant improvements in single message publishing and smaller payload batch operations:

| Benchmark | Previous | Current | Improvement |
|-----------|----------|---------|-------------|
| QoS 1 single message | 6,094.69 μs | 551.43 μs | **~91% faster** |
| QoS 2 single message | 8,083.58 μs | 1,004.75 μs | **~88% faster** |
| 100x 256b QoS 0 | 151.09 μs | 97.12 μs | ~36% faster |
| 100x 256b QoS 1 | 26,178.26 μs | 19,516.85 μs | ~25% faster |
| 100x 256b QoS 2 | 43,196.69 μs | 36,133.97 μs | ~16% faster |
| 100x 256k QoS 0 | 139.16 μs | 100.22 μs | ~28% faster |
| 1k QoS 2 messages | 325,461.87 μs | 306,406.35 μs | ~6% faster |

The most dramatic improvements were seen in single message QoS 1 and QoS 2 operations, indicating better handling of acknowledgment flows and reduced connection overhead.

### Raw Benchmark Data

| Method                                           | Mean          | Error        | StdDev       | Median         |
|------------------------------------------------- |--------------:|-------------:|-------------:|---------------:|
| 'Publish a QoS 0 message'                        |      26.00 μs |     75.28 μs |    221.97 μs |       3.667 μs |
| 'Publish a QoS 1 message'                        |     551.43 μs |    225.28 μs |    664.23 μs |     395.938 μs |
| 'Publish a QoS 2 message'                        |   1,004.75 μs |    295.34 μs |    870.80 μs |     782.188 μs |
| 'Publish 100 256b length payload QoS 0 messages' |      97.12 μs |     66.68 μs |    196.61 μs |      73.959 μs |
| 'Publish 100 256b length payload QoS 1 messages' |  19,516.85 μs |    811.32 μs |  2,392.21 μs |  19,155.875 μs |
| 'Publish 100 256b length payload QoS 2 messages' |  36,133.97 μs |  1,572.75 μs |  4,637.29 μs |  36,361.791 μs |
| 'Publish 100 256k length payload QoS 0 messages' |     100.22 μs |     69.78 μs |    205.74 μs |      77.270 μs |
| 'Publish 100 256k length payload QoS 1 messages' | 206,129.00 μs | 23,529.04 μs | 69,375.91 μs | 194,267.416 μs |
| 'Publish 100 256k length payload QoS 2 messages' | 222,555.86 μs | 23,062.91 μs | 68,001.51 μs | 211,001.312 μs |
| 'Publish 1k QoS 1 messages'                      | 173,926.43 μs |  3,766.79 μs | 11,106.45 μs | 176,484.750 μs |
| 'Publish 1k QoS 2 messages'                      | 306,406.35 μs | 10,326.42 μs | 30,447.67 μs | 298,261.895 μs |

## October 28, 2025 (v0.31.0)

### Performance Improvements from v0.30.0

Significant performance improvements across all benchmarks:

| Benchmark | Previous | Current | Improvement |
|-----------|----------|---------|-------------|
| 1k QoS 1 messages | 287,802.82 μs | 163,134.95 μs | **~43% faster** |
| 1k QoS 2 messages | 533,574.86 μs | 325,461.87 μs | **~39% faster** |

The most dramatic improvements were seen in bulk message publishing scenarios, particularly for QoS 1 and QoS 2 messages.

### Raw Benchmark Data

| Method                                           | Mean          | Error        | StdDev        | Median         |
|------------------------------------------------- |--------------:|-------------:|--------------:|---------------:|
| 'Publish a QoS 0 message'                        |      24.20 μs |     70.88 μs |     208.99 μs |       2.646 μs |
| 'Publish a QoS 1 message'                        |   6,094.69 μs | 16,414.36 μs |  48,398.12 μs |   1,026.708 μs |
| 'Publish a QoS 2 message'                        |   8,083.58 μs | 17,437.59 μs |  51,415.13 μs |   1,353.751 μs |
| 'Publish 100 256b length payload QoS 0 messages' |     151.09 μs |    116.79 μs |     344.37 μs |     107.188 μs |
| 'Publish 100 256b length payload QoS 1 messages' |  26,178.26 μs | 18,001.87 μs |  53,078.92 μs |  20,828.730 μs |
| 'Publish 100 256b length payload QoS 2 messages' |  43,196.69 μs | 19,064.73 μs |  56,212.80 μs |  36,206.188 μs |
| 'Publish 100 256k length payload QoS 0 messages' |     139.16 μs |     94.26 μs |     277.94 μs |     101.584 μs |
| 'Publish 100 256k length payload QoS 1 messages' | 157,189.65 μs | 41,857.20 μs | 123,416.91 μs | 139,608.958 μs |
| 'Publish 100 256k length payload QoS 2 messages' | 180,717.82 μs | 49,526.73 μs | 146,030.67 μs | 154,019.188 μs |
| 'Publish 1k QoS 1 messages'                      | 163,134.95 μs | 19,365.29 μs |  57,099.01 μs | 160,983.167 μs |
| 'Publish 1k QoS 2 messages'                      | 325,461.87 μs | 26,343.40 μs |  77,674.12 μs | 314,379.667 μs |

## October 28, 2025 (v0.30.0)

### Performance Improvements from March 2024

Major performance improvements across all benchmarks:

| Benchmark | Previous | Current | Improvement |
|-----------|----------|---------|-------------|
| 100x 256b QoS 1 | 45,813.98 μs | 26,283.55 μs | **~43% faster** |
| 100x 256b QoS 2 | 88,589.38 μs | 44,557.05 μs | **~50% faster** |
| 100x 256k QoS 1 | 270,043.05 μs | 155,177.99 μs | **~43% faster** |
| 100x 256k QoS 2 | 300,923.38 μs | 172,109.05 μs | **~43% faster** |

**New benchmarks introduced**: Added 1k message bulk publishing tests for QoS 1 and QoS 2.

### Raw Benchmark Data

| Method                                           | Mean          | Error        | StdDev        | Median         |
|------------------------------------------------- |--------------:|-------------:|--------------:|---------------:|
| 'Publish a QoS 0 message'                        |      39.67 μs |     89.35 μs |     263.45 μs |       8.521 μs |
| 'Publish a QoS 1 message'                        |   5,892.86 μs | 16,632.25 μs |  49,040.57 μs |     911.083 μs |
| 'Publish a QoS 2 message'                        |   7,369.47 μs | 16,676.10 μs |  49,169.85 μs |   1,457.687 μs |
| 'Publish 100 256b length payload QoS 0 messages' |     137.26 μs |     78.49 μs |     231.44 μs |     103.145 μs |
| 'Publish 100 256b length payload QoS 1 messages' |  26,283.55 μs | 19,855.80 μs |  58,545.26 μs |  19,256.166 μs |
| 'Publish 100 256b length payload QoS 2 messages' |  44,557.05 μs | 21,810.68 μs |  64,309.29 μs |  36,249.938 μs |
| 'Publish 100 256k length payload QoS 0 messages' |     141.71 μs |     96.62 μs |     284.88 μs |     102.645 μs |
| 'Publish 100 256k length payload QoS 1 messages' | 155,177.99 μs | 39,395.25 μs | 116,157.79 μs | 138,491.062 μs |
| 'Publish 100 256k length payload QoS 2 messages' | 172,109.05 μs | 44,912.98 μs | 132,426.93 μs | 149,029.541 μs |
| 'Publish 1k QoS 1 messages'                      | 287,802.82 μs | 90,041.54 μs | 265,489.51 μs | 248,344.521 μs |
| 'Publish 1k QoS 2 messages'                      | 533,574.86 μs | 62,147.67 μs | 183,243.82 μs | 475,369.771 μs |

## Historical Data

### March 2024

| Method                                           | Mean          | Error       | StdDev       | Median         |
|------------------------------------------------- |--------------:|------------:|-------------:|---------------:|
| 'Publish a QoS 0 message'                        |      57.27 μs |   158.55 μs |    467.50 μs |       9.084 μs |
| 'Publish a QoS 1 message'                        |   2,291.28 μs |   903.01 μs |  2,662.56 μs |   1,357.063 μs |
| 'Publish a QoS 2 message'                        |   2,058.05 μs | 1,048.91 μs |  3,092.73 μs |   1,292.396 μs |
| 'Publish 100 256b length payload QoS 0 messages' |     138.29 μs |   183.38 μs |    540.69 μs |      79.604 μs |
| 'Publish 100 256b length payload QoS 1 messages' |  45,813.98 μs | 4,838.62 μs | 14,266.78 μs |  42,482.520 μs |
| 'Publish 100 256b length payload QoS 2 messages' |  88,589.38 μs | 3,877.02 μs | 11,431.48 μs |  85,640.167 μs |
| 'Publish 100 256k length payload QoS 0 messages' |     124.92 μs |   173.22 μs |    510.74 μs |      69.709 μs |
| 'Publish 100 256k length payload QoS 1 messages' | 270,043.05 μs | 8,850.72 μs | 26,096.56 μs | 266,506.583 μs |
| 'Publish 100 256k length payload QoS 2 messages' | 300,923.38 μs | 5,704.22 μs | 16,819.03 μs | 296,254.688 μs |

### v0.11.0 Performance Journey (March 2024)

With release [v0.11.0](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases/tag/v0.11.0) there was a major performance improvement. Publishing a **QoS level 2 message went from ~206ms down to ~1.6ms**.

| Method | Original | After Refactor | Improvement |
|--------|----------|----------------|-------------|
| QoS 0 Message | 5.646 μs | 4.875 μs | 14% faster |
| QoS 1 Message | 103,536 μs | 790 μs | **99% faster** |
| QoS 2 Message | 206,959 μs | 1,653 μs | **99% faster** |

## See Also

- [What is MQTT Quality of Service (QoS) 0, 1, & 2? – MQTT Essentials](https://www.hivemq.com/blog/mqtt-essentials-part-6-mqtt-quality-of-service-levels/)
- [Benchmark source code](https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Benchmarks/ClientBenchmarkApp)
