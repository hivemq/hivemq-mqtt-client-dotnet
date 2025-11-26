# Benchmarks

The benchmarks are built with [BenchmarkDotNet](https://benchmarkdotnet.org) and can be run with:

`dotnet run ClientBenchmarkApp.csproj -c Release`

# Results

The following results are from benchmarks run on my local MBP against a HiveMQ v4 broker running in a Docker container over localhost.

## Legend
```
  Mean   : Arithmetic mean of all measurements
  Error  : Half of 99.9% confidence interval
  StdDev : Standard deviation of all measurements
  Median : Value separating the higher half of all measurements (50th percentile)
  1 us   : 1 Microsecond (0.000001 sec)
  1 ms   : 1,000 Microseconds
```

## November 26, 2025

### Performance Improvements from October 28, 2025

Significant improvements in single message publishing and smaller payload batch operations:

- **QoS 1 single message**: ~91% faster (6,094.69μs → 551.43μs)
- **QoS 2 single message**: ~88% faster (8,083.58μs → 1,004.75μs)
- **100x 256b QoS 0 messages**: ~36% faster (151.09μs → 97.12μs)
- **100x 256b QoS 1 messages**: ~25% faster (26,178.26μs → 19,516.85μs)
- **100x 256b QoS 2 messages**: ~16% faster (43,196.69μs → 36,133.97μs)
- **100x 256k QoS 0 messages**: ~28% faster (139.16μs → 100.22μs)
- **1k QoS 2 messages**: ~6% faster (325,461.87μs → 306,406.35μs)

The most dramatic improvements were seen in single message QoS 1 and QoS 2 operations, indicating better handling of acknowledgment flows and reduced connection overhead.

| Method                                           | Mean          | Error        | StdDev       | Median         |
|------------------------------------------------- |--------------:|-------------:|-------------:|---------------:|
| 'Publish a QoS 0 message'                        |      26.00 us |     75.28 us |    221.97 us |       3.667 us |
| 'Publish a QoS 1 message'                        |     551.43 us |    225.28 us |    664.23 us |     395.938 us |
| 'Publish a QoS 2 message'                        |   1,004.75 us |    295.34 us |    870.80 us |     782.188 us |
| 'Publish 100 256b length payload QoS 0 messages' |      97.12 us |     66.68 us |    196.61 us |      73.959 us |
| 'Publish 100 256b length payload QoS 1 messages' |  19,516.85 us |    811.32 us |  2,392.21 us |  19,155.875 us |
| 'Publish 100 256b length payload QoS 2 messages' |  36,133.97 us |  1,572.75 us |  4,637.29 us |  36,361.791 us |
| 'Publish 100 256k length payload QoS 0 messages' |     100.22 us |     69.78 us |    205.74 us |      77.270 us |
| 'Publish 100 256k length payload QoS 1 messages' | 206,129.00 us | 23,529.04 us | 69,375.91 us | 194,267.416 us |
| 'Publish 100 256k length payload QoS 2 messages' | 222,555.86 us | 23,062.91 us | 68,001.51 us | 211,001.312 us |
| 'Publish 1k QoS 1 messages'                      | 173,926.43 us |  3,766.79 us | 11,106.45 us | 176,484.750 us |
| 'Publish 1k QoS 2 messages'                      | 306,406.35 us | 10,326.42 us | 30,447.67 us | 298,261.895 us |


## October 28, 2025 v0.31.0

### Performance Improvements from v0.30.0

Significant performance improvements across all benchmarks, with the most notable gains in:

- **1k QoS 1 messages**: ~43% faster (287,802.82μs → 163,134.95μs)
- **1k QoS 2 messages**: ~39% faster (533,574.86μs → 325,461.87μs)

The most dramatic improvements were seen in bulk message publishing scenarios, particularly for QoS 1 and QoS 2 messages, indicating better handling of acknowledgment flows and reduced overhead per message in batch operations.

| Method                                           | Mean          | Error        | StdDev        | Median         |
|------------------------------------------------- |--------------:|-------------:|--------------:|---------------:|
| 'Publish a QoS 0 message'                        |      24.20 us |     70.88 us |     208.99 us |       2.646 us |
| 'Publish a QoS 1 message'                        |   6,094.69 us | 16,414.36 us |  48,398.12 us |   1,026.708 us |
| 'Publish a QoS 2 message'                        |   8,083.58 us | 17,437.59 us |  51,415.13 us |   1,353.751 us |
| 'Publish 100 256b length payload QoS 0 messages' |     151.09 us |    116.79 us |     344.37 us |     107.188 us |
| 'Publish 100 256b length payload QoS 1 messages' |  26,178.26 us | 18,001.87 us |  53,078.92 us |  20,828.730 us |
| 'Publish 100 256b length payload QoS 2 messages' |  43,196.69 us | 19,064.73 us |  56,212.80 us |  36,206.188 us |
| 'Publish 100 256k length payload QoS 0 messages' |     139.16 us |     94.26 us |     277.94 us |     101.584 us |
| 'Publish 100 256k length payload QoS 1 messages' | 157,189.65 us | 41,857.20 us | 123,416.91 us | 139,608.958 us |
| 'Publish 100 256k length payload QoS 2 messages' | 180,717.82 us | 49,526.73 us | 146,030.67 us | 154,019.188 us |
| 'Publish 1k QoS 1 messages'                      | 163,134.95 us | 19,365.29 us |  57,099.01 us | 160,983.167 us |
| 'Publish 1k QoS 2 messages'                      | 325,461.87 us | 26,343.40 us |  77,674.12 us | 314,379.667 us |


## October 28, 2025 v0.30.0

### Performance Improvements from Mar 22, 2024

Major performance improvements across all benchmarks, with substantial gains in bulk message publishing:

- **100x 256b QoS 1 messages**: ~43% faster (45,813.98μs → 26,283.55μs)
- **100x 256b QoS 2 messages**: ~50% faster (88,589.38μs → 44,557.05μs)
- **100x 256k QoS 1 messages**: ~43% faster (270,043.05μs → 155,177.99μs)
- **100x 256k QoS 2 messages**: ~43% faster (300,923.38μs → 172,109.05μs)

**New benchmarks introduced**: Added 1k message bulk publishing tests for QoS 1 and QoS 2, providing better insights into high-volume scenarios.

The improvements demonstrate significant optimization in batch processing and acknowledgment handling, particularly for larger payloads and bulk operations.

| Method                                           | Mean          | Error        | StdDev        | Median         |
|------------------------------------------------- |--------------:|-------------:|--------------:|---------------:|
| 'Publish a QoS 0 message'                        |      39.67 us |     89.35 us |     263.45 us |       8.521 us |
| 'Publish a QoS 1 message'                        |   5,892.86 us | 16,632.25 us |  49,040.57 us |     911.083 us |
| 'Publish a QoS 2 message'                        |   7,369.47 us | 16,676.10 us |  49,169.85 us |   1,457.687 us |
| 'Publish 100 256b length payload QoS 0 messages' |     137.26 us |     78.49 us |     231.44 us |     103.145 us |
| 'Publish 100 256b length payload QoS 1 messages' |  26,283.55 us | 19,855.80 us |  58,545.26 us |  19,256.166 us |
| 'Publish 100 256b length payload QoS 2 messages' |  44,557.05 us | 21,810.68 us |  64,309.29 us |  36,249.938 us |
| 'Publish 100 256k length payload QoS 0 messages' |     141.71 us |     96.62 us |     284.88 us |     102.645 us |
| 'Publish 100 256k length payload QoS 1 messages' | 155,177.99 us | 39,395.25 us | 116,157.79 us | 138,491.062 us |
| 'Publish 100 256k length payload QoS 2 messages' | 172,109.05 us | 44,912.98 us | 132,426.93 us | 149,029.541 us |
| 'Publish 1k QoS 1 messages'                      | 287,802.82 us | 90,041.54 us | 265,489.51 us | 248,344.521 us |
| 'Publish 1k QoS 2 messages'                      | 533,574.86 us | 62,147.67 us | 183,243.82 us | 475,369.771 us |

## Mar 22, 2024

**New comprehensive benchmarks**: Introduced bulk message publishing tests with different payload sizes (256b and 256k), providing detailed performance insights for real-world scenarios.

The results demonstrate excellent scalability and efficiency in handling bulk operations, with particularly strong performance for larger payloads and QoS 2 message handling.

| Method                                           | Mean          | Error       | StdDev       | Median         |
|------------------------------------------------- |--------------:|------------:|-------------:|---------------:|
| 'Publish a QoS 0 message'                       |      57.27 us |   158.55 us |    467.50 us |       9.084 us |
| 'Publish a QoS 1 message'                       |   2,291.28 us |   903.01 us |  2,662.56 us |   1,357.063 us |
| 'Publish a QoS 2 message'                       |   2,058.05 us | 1,048.91 us |  3,092.73 us |   1,292.396 us |
| 'Publish 100 256b length payload QoS 0 messages' |     138.29 us |   183.38 us |    540.69 us |      79.604 us |
| 'Publish 100 256b length payload QoS 1 messages' |  45,813.98 us | 4,838.62 us | 14,266.78 us |  42,482.520 us |
| 'Publish 100 256b length payload QoS 2 messages' |  88,589.38 us | 3,877.02 us | 11,431.48 us |  85,640.167 us |
| 'Publish 100 256k length payload QoS 0 messages' |     124.92 us |   173.22 us |    510.74 us |      69.709 us |
| 'Publish 100 256k length payload QoS 1 messages' | 270,043.05 us | 8,850.72 us | 26,096.56 us | 266,506.583 us |
| 'Publish 100 256k length payload QoS 2 messages' | 300,923.38 us | 5,704.22 us | 16,819.03 us | 296,254.688 us |


## Mar 21, 2024

With release [v0.11.0](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases/tag/v0.11.0) there was a big performance improvement.  All messaging performance was improved but particularly publishing a QoS level 2 message went from ~206ms down to ~1.6ms.

###  Previous Performance

| Method                                   | Mean         | Error       | StdDev      | Median         |
|----------------------------------------- |-------------:|------------:|------------:|---------------:|
| 'Publish a QoS 0 message' |     390.8 us |  1,842.5 us |  1,218.7 us |       5.646 us |
| 'Publish a QoS 1 message' | 103,722.8 us |  4,330.0 us |  2,864.1 us | 103,536.375 us |
| 'Publish a QoS 2 message' | 202,367.9 us | 26,562.9 us | 17,569.7 us | 206,959.834 us |

### First Pass Refactor Performance

| Method                                 | Mean       | Error      | StdDev     | Median       |
|--------------------------------------- |-----------:|-----------:|-----------:|-------------:|
| 'Publish a QoS 0 message' |   401.9 us | 1,876.3 us | 1,241.0 us |     9.250 us |
| 'Publish a QoS 1 message' | 2,140.0 us | 3,568.2 us | 2,360.1 us | 1,324.251 us |
| 'Publish a QoS 2 message' | 4,217.2 us | 5,803.7 us | 3,838.8 us | 2,569.166 us |

### Final Refactor Performance Results (for now 👻)

| Method                                    | Mean        | Error     | StdDev      | Median       |
|------------------------------------------ |------------:|----------:|------------:|-------------:|
| 'Publish a QoS 0 message' |    47.11 us | 139.47 us |   411.23 us |     4.875 us |
| 'Publish a QoS 1 message' | 1,210.71 us | 508.64 us | 1,499.75 us |   790.645 us |
| 'Publish a QoS 2 message' | 2,080.46 us | 591.38 us | 1,743.71 us | 1,653.083 us |
