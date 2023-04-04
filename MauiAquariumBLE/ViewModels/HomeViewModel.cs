
using System.ComponentModel;
using System.Windows.Input;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public IAsyncRelayCommand OnLedStatusButtonClicked { get; private set; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string bluetoothStatus = "Unknown";

    [ObservableProperty]
    TimeSpan currentTime = TimeSpan.Parse("00:00");

    [ObservableProperty]
    string ledStatusButtonSource = "led_q.png";

    [ObservableProperty]
    string bluetoothStatusImage = "bluetooth_disconected.png";

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = $"Home";

        BluetoothLEService = bluetoothLEService;
        BluetoothLEService.StatusChanged += ReadBluetoothStatus;
        BluetoothLEService.MessageReceived += ReadBluetoothMessage;

        OnLedStatusButtonClicked = new AsyncRelayCommand(ChangeLed);
    }

    private void ReadBluetoothStatus()
    {
        BluetoothStatus = BluetoothLEService.Status;
        BluetoothStatusImage = BluetoothLEService.Device?.State == DeviceState.Connected ? "bluetooth.png" : "bluetooth_disconected.png";
    }
    private void ReadBluetoothMessage()
    {
        if (BluetoothLEService.Message.StartsWith("ArduinoOutputs"))
        {
            //ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
            string[] message = BluetoothLEService.Message.Split(";");
            CurrentTime = TimeSpan.Parse(message[1]);
            LedStatusButtonSource = message[7].Contains("led on") ? "led_on.png" : "led_off.png";
            var a = LedStatusButtonSource;
        }
        else if (BluetoothLEService.Message.StartsWith("led on"))
        {
            LedStatusButtonSource = "led_on.png";
            BluetoothStatus = "Light changed succesfully";
        }
        else if (BluetoothLEService.Message.StartsWith("led off"))
        {
            LedStatusButtonSource = "led_off.png";
            BluetoothStatus = "Light changed succesfully";
        }
    }

    public async Task ConnectToDeviceAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

    private async Task ChangeLed()
    {
        //TODO enable button only when receive data
        var command = LedStatusButtonSource.Contains("led_on") ? "led off" : "led on";
        LedStatusButtonSource = "led_q.png";
        await BluetoothLEService.Send($"turn {command}");
    }

                            
}
