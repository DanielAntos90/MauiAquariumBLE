
using System.ComponentModel;
using System.Windows.Input;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{

    public ICommand OnLedStatusButtonClicked { get; private set; }
   // public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }

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
       // BluetoothStatus = "Unknown";
       // CurrentTime = TimeSpan.Parse("00:00");

        BluetoothLEService = bluetoothLEService;
        BluetoothLEService.PropertyChanged += ViewModel_PropertyChanged;

        OnLedStatusButtonClicked = new Command(ChangeLed);
        //DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BluetoothLEService.Status))
        {
            BluetoothStatus = BluetoothLEService.Status;
            BluetoothStatusImage = BluetoothLEService.Device?.State == DeviceState.Connected ? "bluetooth.png" : "bluetooth_disconected.png";

        } else if (e.PropertyName == nameof(BluetoothLEService.Message))
        {
            //    ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
      
            if (BluetoothLEService.Message.StartsWith("ArduinoOutputs")) {
                string[] message = BluetoothLEService.Message.Split(";");
                CurrentTime = TimeSpan.Parse(message[1]);
                LedStatusButtonSource = message[1].Contains("led on") ? "led_on.png" : "led_off.png";
            } else if (BluetoothLEService.Message.StartsWith("led on"))
            {
                LedStatusButtonSource = "led_on.png";
                BluetoothStatus = "Light changed succesfully";
            }
            else if (BluetoothLEService.Message.StartsWith("led off"))
            {
                LedStatusButtonSource =  "led_off.png";
                BluetoothStatus = "Light changed succesfully";
            }
           // "Time changed"
           // "Light changed"


        }
    }

    [ObservableProperty]
    ushort heartRateValue;

    public async Task ConnectToDeviceAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

    private async void ChangeLed()
    {
        //TODO enable button only when receive data
        var command = LedStatusButtonSource.Contains("led_on") ? "led off" : "led on";
        LedStatusButtonSource = "led_q.png";
        await BluetoothLEService.Send($"turn {command}");
    }

                            
}
