---
sidebar_position: 90
---

# Benchmarks

The HiveMQtt GitHub repository provides benchmarks built using BenchmarkDotNet, a .NET library for benchmarking. These benchmarks measure the performance of various messaging operations against any MQTT broker.



## Running Benchmarks

To run the benchmarks, execute the following commands:

```bash
cd Benchmarks/ClientBenchmarkApp
dotnet run ClientBenchmarkApp.csproj -c Release
```

## Results

The benchmarks provide insights into the performance of different messaging methods under various scenarios. Below are the results obtained from running the benchmarks on a local MacBook Pro against a HiveMQ v4 broker running in a Docker container over localhost.

## Legend

* **Mean**: Arithmetic mean of all measurements
* **Error**: Half of 99.9% confidence interval
* **StdDev**: Standard deviation of all measurements
* **Median**: Value separating the higher half of all measurements (50th percentile)
* **1 us**: 1 Microsecond (0.000001 sec)
* **1 ms**: 1,000 Microseconds
* <font color="green">Green</font>: Improvement
* <font color="red">Red</font>: Regression

## July 25, 2024

### Summary

The client has had many feature additions and improvements from March 2024 including:

* New Back-pressure support
* Improved QoS reliability
* Improved Async await strategies
* Re-architected internal queues
* Improved event & trigger handling
* All around better error handling

The following table shows the benchmark results compared against previous benchmark runs.

| Method | Alpha | March 2024 | July 2024 |
| ------ | ----- | -------- | -------------- |
| QoS 0 Message    | <font color="">4.875 us</font>  | <font color="red">9.084 us</font>  | <font color="green">5.500 us</font>  |
| QoS 1 Message    | <font color="">790.645 us</font>  | <font color="red">1,357.063 us</font>  | <font color="green">982.208 us</font>  |
| QoS 2 Message    | <font color="">1,653.083 us</font>  | <font color="green">1,292.396 us</font>  | <font color="green">1,004.854 us</font>  |
| 100 QoS 0 @ 256b | No Data | <font color="">79.604 us</font>  | <font color="red">100.375 us</font>  |
| 100 QoS 1 @ 256b | No Data | <font color="">42,482.520 us</font>  | <font color="green">29,661.605 us</font>  |
| 100 QoS 2 @ 256b | No Data | <font color="">85,640.167 us</font>  | <font color="green">55,474.250 us</font>  |
| 100 QoS 0 @ 256k | No Data | <font color="">69.709 us</font>  | <font color="red">99.354 us</font>  |
| 100 QoS 1 @ 256k | No Data | <font color="">266,506.583 us</font>  | <font color="green">215,158.104 us</font>  |
| 100 QoS 2 @ 256k | No Data | <font color="">296,254.688 us</font>  | <font color="green">227,255.729 us</font>  |

### July Benchmark Data

The raw benchmark data from July.

| Method                                           | Mean          | Error        | StdDev        | Median         |
|------------------------------------------------- |--------------:|-------------:|--------------:|---------------:|
| 'Publish a QoS 0 message'                        |      32.59 us |     85.64 us |     252.53 us |       5.500 us |
| 'Publish a QoS 1 message'                        |   6,323.30 us | 16,671.57 us |  49,156.51 us |     982.208 us |
| 'Publish a QoS 2 message'                        |   6,554.90 us | 16,998.38 us |  50,120.10 us |   1,004.854 us |
| 'Publish 100 256b length payload QoS 0 messages' |     131.73 us |     92.15 us |     271.71 us |     100.375 us |
| 'Publish 100 256b length payload QoS 1 messages' |  35,405.71 us | 17,183.26 us |  50,665.23 us |  29,661.605 us |
| 'Publish 100 256b length payload QoS 2 messages' |  61,955.04 us | 17,559.19 us |  51,773.66 us |  55,474.250 us |
| 'Publish 100 256k length payload QoS 0 messages' |     129.00 us |     83.24 us |     245.42 us |      99.354 us |
| 'Publish 100 256k length payload QoS 1 messages' | 226,498.28 us | 42,050.59 us | 123,987.13 us | 215,158.104 us |
| 'Publish 100 256k length payload QoS 2 messages' | 244,227.03 us | 47,932.40 us | 141,329.77 us | 227,255.729 us |

## Mar 22, 2024

__Publishing a QoS 2 message__ with the full round-trip of confirmation packets __is now timed at ~1.2ms__.

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

See also: [What is MQTT Quality of Service (QoS) 0,1, & 2? – MQTT Essentials: Part 6](https://www.hivemq.com/blog/mqtt-essentials-part-6-mqtt-quality-of-service-levels/).

## Mar 21, 2024

With release [v0.11.0](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases/tag/v0.11.0) there was a big performance improvement.  All messaging performance was improved but particularly publishing a **QoS level 2 message went from ~206ms down to ~1.6ms**.

### Summary

From the early version through two refactors, this table summarizes the performance improvements.

| Method | Original | First Pass Refactor | Final Refactor |
| ------ | -------- | ------------------- | -------------- |
| QoS 0 Message | <font color="red">5.646 us</font>  | 9.250 us  | <font color="green">4.875 us</font>  |
| QoS 1 Message | <font color="red">103,536.375 us</font>  | 1,324.251 us  | <font color="green">790.645 us</font>  |
| QoS 2 Message | <font color="red">206,959.834 us</font>  | 2,569.166 us  | <font color="green">1,653.083 us</font>  |

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
