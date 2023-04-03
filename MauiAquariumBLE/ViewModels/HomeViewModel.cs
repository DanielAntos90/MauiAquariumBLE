
using System.ComponentModel;

namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
    public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string status;

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = $"Home";

        BluetoothLEService = bluetoothLEService;
        BluetoothLEService.PropertyChanged += ViewModel_PropertyChanged;

        ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync );

        DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);

    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BluetoothLEService.MyProperty))
        {
            Status = BluetoothLEService.MyProperty;
            // update the property in this class
        }
    }

    [ObservableProperty]
    ushort heartRateValue;

    private async Task ConnectToDeviceCandidateAsync()
    {
        await BluetoothLEService.ConnectToDeviceAsync();
    
    }

    private async Task DisconnectFromDeviceAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (BluetoothLEService.Device == null)
        {
            await BluetoothLEService.ShowToastAsync($"Nothing to do.");
            return;
        }

        if (!BluetoothLEService.BluetoothLE.IsOn)
        {
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            return;
        }

        if (BluetoothLEService.Adapter.IsScanning)
        {
            await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
            return;
        }

        if (BluetoothLEService.Device.State == DeviceState.Disconnected)
        {
            await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} is already disconnected.");
            return;
        }

        try
        {
            IsBusy = true;

            //TODO make stopUpdatesAsync as method
            await BluetoothLEService.BluetoothConnectionCharacteristic.StopUpdatesAsync();

           await BluetoothLEService.Adapter.DisconnectDeviceAsync(BluetoothLEService.Device);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to disconnect from {BluetoothLEService.Device.Name} {BluetoothLEService.Device.Id}: {ex.Message}.");
            await Shell.Current.DisplayAlert($"{BluetoothLEService.Device.Name}", $"Unable to disconnect from {BluetoothLEService.Device.Name}.", "OK");
        }
        finally
        {
            Title = "Heart rate";
            HeartRateValue = 0;
            IsBusy = false;
            BluetoothLEService.Device?.Dispose();
            await Shell.Current.GoToAsync("//HomePage", true);
        }
    }
}
