The HiveMQtt package uses [NLog](https://github.com/NLog/NLog) and can be configured with a configuration
file (`NLog.config`).  Having this file in the same directory of your executable will configure the
HiveMQtt logger to output as configured.

Use this file if you want to see the inner workings (Trace level to Error) of the HiveMQtt package in your application.

See [this section](https://github.com/hivemq/hivemq-mqtt-client-dotnet#logging) in the repository README for more details.

