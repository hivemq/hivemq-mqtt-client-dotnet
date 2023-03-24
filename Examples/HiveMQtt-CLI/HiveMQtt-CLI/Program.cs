/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

// Built from the Microsoft CLI Template.
// See https://aka.ms/new-console-template for more information

// Example Options to connect to HiveMQ cloud
//
// See here for more information to get your own free instance:
// https://www.hivemq.com/mqtt-cloud-broker/
//
// These values could instead be set with:
//    Environment.GetEnvironmentVariable("HIVEMQTTCLI_HOST")
//  if you set your environment variables externally.
//
var options = new HiveMQClientOptions
{
    Host = "91a9688e01054.s2.eu.hivemq.cloud",
    Port = 8883,
    UseTLS = true,
    UserName = "myusername",
    Password = "mypassword",
};

// Example Options to connect to HiveMQ Public Broker (insecure public testing broker)
//
// These values could instead be set with:
//    Environment.GetEnvironmentVariable("HIVEMQTTCLI_HOST")
//  if you set your environment variables externally.
//
/*
var options = new HiveMQClientOptions
{
    Host = "broker.hivemq.com",
    Port = 8883,
    UseTLS = true,
};
*/

// Example HiveMQClientOptions to connect to a local MQTT broker without authentication
/*
var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
};
*/

var client = new HiveMQClient(options);

if (options.UseTLS)
{
    Console.WriteLine($"Connecting to {options.Host} on port {options.Port} using TLS...");
}
else
{
    Console.WriteLine($"Connecting to {options.Host} on port {options.Port} without TLS...");
}

// Connect
HiveMQtt.Client.Results.ConnectResult connectResult;
try
{
    connectResult = await client.ConnectAsync().ConfigureAwait(false);
    if (connectResult.ReasonCode == ConnAckReasonCode.Success)
    {
        Console.WriteLine($"Connect successful: {connectResult}");
    }
    else
    {
        // FIXME: Add ToString
        Console.WriteLine($"Connect failed: {connectResult}");
        Environment.Exit(-1);
    }
}
catch (System.Net.Sockets.SocketException e)
{
    Console.WriteLine($"Error connecting to the MQTT Broker with the following socket error: {e.Message}");
    Environment.Exit(-1);
}
catch (Exception e)
{
    Console.WriteLine($"Error connecting to the MQTT Broker with the following message: {e.Message}");
    Environment.Exit(-1);
}

Console.WriteLine("Publishing a QoS level 2 message...");

var msg = new string(/*lang=json,strict*/
"{\"interference\": \"1029384\"}");
var result = await client.PublishAsync("tests/MostBasicPublishWithQoS2Async", msg, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

Console.WriteLine(result);

Console.WriteLine("Disconnecting from broker...");
var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
