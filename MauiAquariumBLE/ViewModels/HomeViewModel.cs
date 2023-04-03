
using System.ComponentModel;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
   // public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
   // public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string bluetoothStatus;

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = $"Home";
        BluetoothStatus = "Unknown";

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
        } 
    }

    [ObservableProperty]
    ushort heartRateValue;

    public async Task ConnectToDeviceAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

                            
}
