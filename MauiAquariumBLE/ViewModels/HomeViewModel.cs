using Microsoft.Maui.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public IAsyncRelayCommand OnLedStatusButtonClicked { get; private set; }
    public IAsyncRelayCommand OnUpdateTimeButtonClicked { get; private set; }
    public IAsyncRelayCommand OnUpdateLightButtonClicked { get; private set; }
    public IAsyncRelayCommand OnReconnectButtonClicked { get; private set; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string bluetoothStatus = "Unknown";

    [ObservableProperty]
    TimeSpan currentTime = TimeSpan.Parse("00:00");
    [ObservableProperty]
    TimeSpan ledOnTime = TimeSpan.Parse("00:00");
    [ObservableProperty]
    TimeSpan ledOffTime = TimeSpan.Parse("00:00");

    [ObservableProperty]
    int ledBrightness = 0;
    [ObservableProperty]
    int ledDimmingMinutes = 0;
    // used for recognition if slider was modified by user or application
    public bool IsChangedByUser = false;

    [ObservableProperty]
    public List<int> numbers = Enumerable.Range(0, 101).ToList();

    [ObservableProperty]
    DateTime currentDate = DateTime.ParseExact("01.01.1990", "dd.MM.yyyy", null);

    [ObservableProperty]
    string ledStatusButtonSource = "led_q.png";

    [ObservableProperty]
    string bluetoothStatusImage = "bluetooth_disconected.png";

    [ObservableProperty]
    bool isDataReceived = false;

    [ObservableProperty]
    bool isReloadButtonEnabled = false;

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = "Home";

        BluetoothLEService = bluetoothLEService;
        BluetoothLEService.StatusChanged += ReadBluetoothStatus;
        BluetoothLEService.MessageReceived += ReadBluetoothMessage;

        OnLedStatusButtonClicked = new AsyncRelayCommand(ChangeLed);
        OnUpdateTimeButtonClicked = new AsyncRelayCommand(UpdateTime);
        OnUpdateLightButtonClicked = new AsyncRelayCommand(UpdateLight);
        OnReconnectButtonClicked = new AsyncRelayCommand(ConnectToDeviceAsync, () => IsReloadButtonEnabled);
    }

    private void ReadBluetoothStatus()
    {
        BluetoothStatus = BluetoothLEService.Status;
        BluetoothStatusImage = BluetoothLEService.Device?.State == DeviceState.Connected ? "bluetooth.png" : "bluetooth_disconected.png";

        if(BluetoothLEService.Device?.State != DeviceState.Connected)
        {
            IsDataReceived = false;
            if (!IsWorking)
            {
               // IsReloadButtonEnabled = true;
            }
            
        }
    }
    private void ReadBluetoothMessage()
    {
        IsChangedByUser = false;
        if (BluetoothLEService.Message.StartsWith("ArduinoOutputs"))
        {
            //ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
            string[] message = BluetoothLEService.Message.Split(";");
            CurrentTime = TimeSpan.Parse(message[1]);
            CurrentDate = DateTime.ParseExact(message[2], "dd.MM.yyyy", null);
            LedOnTime = TimeSpan.Parse(message[3]);
            LedOffTime = TimeSpan.Parse(message[4]);
            LedBrightness = int.TryParse(message[5], out var parsedValue) ? parsedValue : 0;
            LedDimmingMinutes = int.TryParse(message[6], out parsedValue) ? parsedValue : 0;
            LedStatusButtonSource = message[7].Contains("led on") ? "led_on.png" : "led_off.png";
            BluetoothStatus = "Data received";
        }
        else if (BluetoothLEService.Message.StartsWith("led on"))
        {
            LedStatusButtonSource = "led_on.png";
            BluetoothStatus = "Light turned on";
        }
        else if (BluetoothLEService.Message.StartsWith("led off"))
        {
            LedStatusButtonSource = "led_off.png";
            BluetoothStatus = "Light turned off";
        }
        IsDataReceived = true;
    }

    public async Task ConnectToDeviceAsync()
    {
        IsWorking = true; //TODO use custom event for IsWorking
        IsDataReceived = false;
        IsReloadButtonEnabled = false;
        await BluetoothLEService.ConnectToDeviceAsync();
        if(BluetoothLEService.Device.Name != null)
        {
            Title = BluetoothLEService.Device.Name;
        }
        if (BluetoothLEService.Device != null)
        {
            await BluetoothLEService.SendData("inputs");
        }

        IsWorking = false;
    }

    private async Task ChangeLed()
    {
        IsDataReceived = false;
        var command = LedStatusButtonSource.Contains("led_on") ? "led off" : "led on";
        LedStatusButtonSource = "led_q.png";
        await BluetoothLEService.SendData($"turn {command}");
    }

    private async Task UpdateTime()
    {
        IsDataReceived = false;
        //bluetoothConnection.send("timedate;"+time+";"+String.join(".", date));
        await BluetoothLEService.SendData($"timedate;{CurrentTime:hh\\:mm};{CurrentDate:dd.MM.yyyy}");
    }

    private async Task UpdateLight()
    {
        IsDataReceived = false;
        //bluetoothConnection.send("light;"+ledOn+";"+ledOff+";"+brightness+";"+dim.replace(" min.",""));
        await BluetoothLEService.SendData($"light;{LedOnTime:hh\\:mm};{LedOffTime:hh\\:mm};{LedBrightness};{LedDimmingMinutes}");
    }

    public async Task LedDimmingValueChange(double value)
    {
        if(ledStatusButtonSource.Contains("led_on") && IsDataReceived)
        {
            await BluetoothLEService.SendData($"dimming;{(long)value}");
        }
    }
}