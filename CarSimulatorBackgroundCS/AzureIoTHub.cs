using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CarSimulatorBackgroundCS;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

class AzureIoTHub
{
    static List<string> VinList = new List<string>();
    static Random random = new Random();
    static Windows.ApplicationModel.Resources.ResourceLoader resources = new Windows.ApplicationModel.Resources.ResourceLoader("azureResources");

    static string HighEngineTempProbabilityPower = resources.GetString("HighEngineTempProbabilityPower");
    static string HighOilProbabilityPower = resources.GetString("HighOilProbabilityPower");
    static string HighOutsideTempProbabilityPower = resources.GetString("HighOutsideTempProbabilityPower");
    static string HighSpeedProbabilityPower = resources.GetString("HighSpeedProbabilityPower");
    static string HighTyrePressureProbabilityPower = resources.GetString("HighTyrePressureProbabilityPower");
    static string LowEngineTempProbabilityPower = resources.GetString("LowEngineTempProbabilityPower");
    static string LowOilProbabilityPower = resources.GetString("LowOilProbabilityPower");
    static string LowOutsideTempProbabilityPower = resources.GetString("LowOutsideTempProbabilityPower");
    static string LowSpeedProbabilityPower = resources.GetString("LowSpeedProbabilityPower");
    static string LowTyrePressureProbabilityPower = resources.GetString("LowTyrePressureProbabilityPower");

    private static void CreateClient()
    {
        if (deviceClient == null)
        {
            // create Azure IoT Hub client from embedded connection string
            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
        }
    }

    static DeviceClient deviceClient = null;

    //
    // Note: this connection string is specific to the device "RaspberryMinWinPC". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=ahmedmostelemetryhub.azure-devices.net;DeviceId=RaspberryMinWinPC;SharedAccessKey=JelgmGjvea2xD/1Hxc3UmSwi2xgOlLob34PBGy7gG1c=";


    //
    // To monitor messages sent to device "kraaa" use iothub-explorer as follows:
    //    iothub-explorer monitor-events --login HostName=ahmedmostelemetryhub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=wfaT7dpbFEpq/S9R9aHcYAKojkj+4wpKYX8jO+tx+SA= "RaspberryMinWinPC"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-2017-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync()
    {
        CreateClient();
        
        GetVINMasterList();

        //var str = "{\"deviceId\":\"RaspberryMinWinPC\",\"messageId\":1,\"text\":\"Hello, Cloud from a UWP C# app!\"}";

        while (true)
        {
            var str = GetCarTelemetryString();

            var message = new Message(Encoding.ASCII.GetBytes(str));

            message.Properties.Add("Type", "Telemetry_" + DateTime.Now.ToString());
            
            await deviceClient.SendEventAsync(message);

        await Task.Delay(200);
    }
}

    private static string GetCarTelemetryString()
    {
        try
        {
            var city = GetLocation();

            var info = new CarEvent()
            {

                vin = GetRandomVIN(),
                city = city,
                outsideTemperature = GetOutsideTemp(),
                engineTemperature = GetEngineTemp(),
                speed = GetSpeed(),
                fuel = random.Next(10, 100),
                engineoil = GetOil(),
                tirepressure = GetTirePressure(),
                odometer = random.Next(1000, 200000),
                accelerator_pedal_position = random.Next(1, 100),
                parking_brake_status = GetRandomBoolean(),
                headlamp_status = GetRandomBoolean(),
                brake_pedal_status = GetRandomBoolean(),
                transmission_gear_position = GetGearPos(),
                ignition_status = GetRandomBoolean(),
                windshield_wiper_status = GetRandomBoolean(),
                abs = GetRandomBoolean(),
                timestamp = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(info);
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
            Console.ResetColor();
            return "ERROR => " + exception.Message;
        }
    }

    private static void GetVINMasterList()
    {
      
            var reader = new StreamReader(File.OpenRead(@"VINMasterList.csv"));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                VinList.Add(values[0]);

            }
       
    }

    private static int GetRandomWeightedNumber(int max, int min, double probabilityPower)
    {
        var randomizer = new Random();
        var randomDouble = randomizer.NextDouble();

        var result = Math.Floor(min + (max + 1 - min) * (Math.Pow(randomDouble, probabilityPower)));
        return (int)result;
    }

    private static bool GetRandomBoolean()
    {
        return new Random().Next(100) % 2 == 0;
    }

    private static int GetSpeed()
    {
        if (GetRandomBoolean())
        {
            return GetRandomWeightedNumber(100, 0, Convert.ToDouble(HighSpeedProbabilityPower));
        }
        return GetRandomWeightedNumber(100, 0, Convert.ToDouble(LowSpeedProbabilityPower));
    }

    private static int GetOil()
    {
        if (GetRandomBoolean())
        {
            return GetRandomWeightedNumber(50, 0, Convert.ToDouble(LowOilProbabilityPower));
        }
        return GetRandomWeightedNumber(50, 0, Convert.ToDouble(HighOilProbabilityPower));
    }

    private static int GetTirePressure()
    {
        if (GetRandomBoolean())
        {
            return GetRandomWeightedNumber(50, 0, Convert.ToDouble(LowTyrePressureProbabilityPower));
        }
        return GetRandomWeightedNumber(50, 0, Convert.ToDouble(HighTyrePressureProbabilityPower));
    }

    private static int GetEngineTemp()
    {
        if (GetRandomBoolean())
        {
            return GetRandomWeightedNumber(500, 0, Convert.ToDouble(HighEngineTempProbabilityPower));
        }
        return GetRandomWeightedNumber(500, 0, Convert.ToDouble(LowEngineTempProbabilityPower));
    }

    private static int GetOutsideTemp()
    {
        if (GetRandomBoolean())
        {
            return GetRandomWeightedNumber(100, 0, Convert.ToDouble(LowOutsideTempProbabilityPower));
        }
        return GetRandomWeightedNumber(100, 0, Convert.ToDouble(HighOutsideTempProbabilityPower));
    }
    private static string GetRandomVIN()
    {
        int RandomIndex = random.Next(1, VinList.Count - 1);
        return VinList[RandomIndex];
    }

    private static string GetLocation()
    {
        List<string> list = new List<string>() { "Doha", "Al Wakrah", "Mesaieed" };
        int l = list.Count;
        Random r = new Random();
        int num = r.Next(l);
        return list[num];
    }
    private static string GetGearPos()
    {
        List<string> list = new List<string>() { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eight" };
        int l = list.Count;
        Random r = new Random();
        int num = r.Next(l);
        return list[num];
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        CreateClient();

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private static async Task<MethodResponse> OnSampleMethod1Called(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("SampleMethod1 has been called");
        return new MethodResponse(200);
    }

    private static async Task<MethodResponse> OnSampleMethod2Called(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("SampleMethod2 has been called");
        return new MethodResponse(200);
    }

    public static async Task RegisterDirectMethodsAsync()
    {
        CreateClient();

        Console.WriteLine("Registering direct method callbacks");
        await deviceClient.SetMethodHandlerAsync("SampleMethod1", OnSampleMethod1Called, null);
        await deviceClient.SetMethodHandlerAsync("SampleMethod2", OnSampleMethod2Called, null);
    }

    public static async Task GetDeviceTwinAsync()
    {
        CreateClient();

        Console.WriteLine("Getting device twin");
        Twin twin = await deviceClient.GetTwinAsync();
        Console.WriteLine(twin.ToJson());
    }

    private static async Task OnDesiredPropertiesUpdated(TwinCollection desiredProperties, object userContext)
    {
        Console.WriteLine("Desired properties were updated");
        Console.WriteLine(desiredProperties.ToJson());
    }

    public static async Task RegisterTwinUpdateAsync()
    {
        CreateClient();

        Console.WriteLine("Registering Device Twin update callback");
        await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertiesUpdated, null);
    }

    public static async Task UpdateDeviceTwin()
    {
        CreateClient();

        TwinCollection tc = new TwinCollection();
        tc["SampleProperty1"] = "test value";

        Console.WriteLine("Updating Device Twin reported properties");
        await deviceClient.UpdateReportedPropertiesAsync(tc);
    }
}
