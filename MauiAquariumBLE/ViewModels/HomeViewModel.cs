
using System.ComponentModel;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
   // public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
   // public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string bluetoothStatus;

    [ObservableProperty]
    TimeSpan currentTime;

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = $"Home";
        BluetoothStatus = "Unknown";
        CurrentTime = TimeSpan.Parse("00:00");

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
        } else if (e.PropertyName == nameof(BluetoothLEService.Message))
        {
            //    ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
            string[] message = BluetoothLEService.Message.Split(";");
            CurrentTime = TimeSpan.Parse(message[1]);
      
           
        }
    }

    [ObservableProperty]
    ushort heartRateValue;

    public async Task ConnectToDeviceAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

                            
}
