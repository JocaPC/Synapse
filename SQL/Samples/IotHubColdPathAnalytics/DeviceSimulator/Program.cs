// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//This is the code that sends messages to the IoT Hub for testing the routing as defined
//  in this article: https://docs.microsoft.com/en-us/azure/iot-hub/tutorial-routing
//The scripts for creating the resources are included in the resources folder in this
//  Visual Studio solution. 
//
// This program encodes the message body so it can be queried against by the Iot hub.
//

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    class Program
    {
        private static DeviceClient s_deviceClient;

        private static string s_iotHubUri = /*{your device key}*/ ".azure-devices.net";
        
        // Create device using portal https://docs.microsoft.com/en-us/azure/iot-hub/tutorial-routing-config-message-routing-rm-template#create-simulated-device
        private static string s_myDeviceId = "{your device key}";
        
        // This is the primary key for the device. This is in the portal. 
        // Find your IoT hub in the portal > IoT devices > select your device > copy the key. 
        private static string s_deviceKey = "{your device key}";

        private static void Main(string[] args)
        {
            // Send messages to the simulated device. Each message will contain a randomly generated 
            //   Temperature and Humidity.
            // The "level" of each message is set randomly to "storage", "critical", or "normal".
            // The messages are routed to different endpoints depending on the level, temperature, and humidity.
            //  This is set in the tutorial that goes with this sample: 
            //  http://docs.microsoft.com/azure/iot-hub/tutorial-routing

            Console.WriteLine("Routing Tutorial: Simulated device\n");

            switch(args.Length)
            {
                case 0:
                    Console.WriteLine("First argument must be IoT Hub name where the messages should be sent");
                    Console.WriteLine("Example:\n>SimulatedDevice <IotHubName> <IotHubKey>");
                    return;
                case 1:
                    Console.WriteLine("Second argument must be device in IoT Hub .\n\t*Find your IoT hub in the portal > IoT devices.");
                    Console.WriteLine("Example:\n>SimulatedDevice <IotHubName> <IotHubKey>");
                    return;
                case 2:
                    Console.WriteLine("Third argument must be IoT Hub key that should be used for authentication.\n\t*Find your IoT hub in the portal > IoT devices > select your device > copy the key.");
                    Console.WriteLine("Example:\n>SimulatedDevice <IotHubName> <IotHubKey>");
                    return;
            }

            s_iotHubUri = args[0] + s_iotHubUri;
            s_myDeviceId = args[1];
            s_deviceKey = args[2];
            
            s_deviceClient = DeviceClient.Create(s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(s_myDeviceId, s_deviceKey), TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.WriteLine("Press the Enter key to stop.");
            Console.ReadLine();
        }

        /// <summary>
        /// Send message to the Iot hub. This generates the object to be sent to the hub in the message.
        /// </summary>
        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                string infoString;
                string levelValue;

                if (rand.NextDouble() > 0.7)
                {
                    if (rand.NextDouble() > 0.5)
                    {
                        levelValue = "storage";
                        infoString = "This is a critical message.";
                    }
                    else
                    {
                        levelValue = "storage";
                        infoString = "This is a storage message.";
                    }
                }
                else
                {
                    levelValue = "storage";
                    infoString = "This is a normal message.";
                }

                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity,
                    info = infoString
                };
                // serialize the telemetry data and convert it to JSON.
                var telemetryDataString = JsonConvert.SerializeObject(telemetryDataPoint);

                var message = new Message(Encoding.Unicode.GetBytes(telemetryDataString));

                //Add one property to the message.
                message.Properties.Add("level", levelValue);

                // Submit the message to the hub.
                await s_deviceClient.SendEventAsync(message);

                // Print out the message.
                Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, telemetryDataString);

                //await Task.Delay(1000);
            }
        }
    } 
}
