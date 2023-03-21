# The Example HiveMQTT CLI

This example is a demonstration on how to use the HiveMQtt NuGet package with a .NET console application.  It was built using [Microsoft's console application template](https://aka.ms/new-console-template).  See those instructions if you would like to expand the example further.

The console application is ready to run after you specify the broker to connect to.  See below for instructions.

# Configure the broker to connect to

Edit the `Program.cs` file and edit the `HiveMQClientOptions` to point to your MQTT broker.  Bundled into that file are a few variations to potentially use.

```c#
var options = new HiveMQClientOptions
{
    Host = "b8212ae75b11f4y2abs254bdea608173b.s1.eu.hivemq.cloud",
    Port = 8883,
    UseTLS = true,
    UserName = 'myusername',
    Password = "mypassword',
}
```

# How to build and run the application

Change into the directory of the console application (where the `Program.cs` file is) and run `dotnet build` and `dotnet run`:

```bash
cd Examples/HiveMQtt-CLI/HiveMQtt-CLI
dotnet build
dotnet run
```

# What does it do?

The simple console application will connect to the specified broker, publish a quality of service level 2 message and disconnect.


