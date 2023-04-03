
using System.ComponentModel;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
   // public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
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

        //ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync );
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
            string[] message = BluetoothLEService.Message.Split(";");
            CurrentTime = TimeSpan.Parse(message[1]);
            LedStatusButtonSource = message[1].Contains("led on") ? "led_on.png" : "led_off.png";

        }
    }

    [ObservableProperty]
    ushort heartRateValue;

    public async Task ConnectToDeviceAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

                            
}
