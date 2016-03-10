using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

static class AzureIoTHub
{
    static bool blinkCommand;
    //
    // Note: this connection string is specific to the device "DragonBoard01". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=iSmartGreenhouse.azure-devices.net;DeviceId=DragonBoard01;SharedAccessKey=425ZbrVGuCC7ybJcBwBI4Q==";

    // const string deviceConnectionString = "HostName=iSmartGreenhouse.azure-devices.net;DeviceId=DragonBoard_Jeff;SharedAccessKey=pbTr6QoQRCr23BSMMs/xjw==";    

    //
    // To monitor messages sent to device "DragonBoard01" use iothub-explorer as follows:
    //    iothub-explorer "HostName=iSmartGreenhouse.azure-devices.net;DeviceId=DragonBoard_Jeff;SharedAccessKey=pbTr6QoQRCr23BSMMs/xjw==" monitor-events "DragonBoard_Jeff"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-cpp for more information on Microsoft Azure IoT Connected Service

    static AzureIoTHub()
    {
        bool blinkCommand = true;
    }

    public static bool GetBlinkCommand()
    {
        return blinkCommand;
    }

    public static void SetBlinkCommand()
    {
        blinkCommand = true;
    }

    public static async Task SendDeviceToCloudMessageAsync(double temp, double humidity)
    {
        try
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Http1);

            var telemetry = new
            {
                DeviceId = "DragonBoard01",
                Temperature = temp,
                Humidity = humidity,
                ExternalTemperature = 67
            };

            var str = JsonConvert.SerializeObject(telemetry);

            var message = new Message(Encoding.ASCII.GetBytes(str));

            await deviceClient.SendEventAsync(message);
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.Write("I suck:" + e);
        }

    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Http1);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                if (messageData.Contains("StopBlink"))
                {
                    blinkCommand = false;
                }
                else
                {
                    blinkCommand = true;
                }

                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            //  Note: In this sample, the polling interval is set to 
            //  10 seconds to enable you to see messages as they are sent.
            //  To enable an IoT solution to scale, you should extend this 
            //  interval. For example, to scale to 1 million devices, set 
            //  the polling interval to 25 minutes.
            //  For further information, see
            //  https://azure.microsoft.com/documentation/articles/iot-hub-devguide/#messaging

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

}
