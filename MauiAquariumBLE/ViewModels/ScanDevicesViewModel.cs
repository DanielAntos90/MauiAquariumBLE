namespace MauiBluetoothBLE.ViewModels;

public partial class ScanDevicesViewModel : BaseViewModel
{
    private BluetoothLEService BluetoothLEService;

    public ObservableCollection<BluetoothDevice> BluetoothDevices { get; } = new();

    public IAsyncRelayCommand GoToHomeViewAsyncCommand { get; }
    public IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; }
    public IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; }

    public ScanDevicesViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = $"Scan and select device";
 
        BluetoothLEService = bluetoothLEService;

        GoToHomeViewAsyncCommand = new AsyncRelayCommand<BluetoothDevice>(async (BluetoothDevice) => await GoToHomeViewAsync(BluetoothDevice));

        ScanNearbyDevicesAsyncCommand = new AsyncRelayCommand(ScanDevicesAsync);
        CheckBluetoothAvailabilityAsyncCommand = new AsyncRelayCommand(CheckBluetoothAvailabilityAsync);
    }

    async Task GoToHomeViewAsync(BluetoothDevice BluetoothDevice)
    {
        if (IsWorking)
        {
            await NotificationService.ShowToastAsync("Bluetooth adapter is scanning. Try again.");
            return;
        }

        if (BluetoothDevice == null)
        {
            return;
        }

        BluetoothLEService.SelectedBluetoothDevice = BluetoothDevice;

        await Shell.Current.GoToAsync("//HomeView", true);
    }

    async Task ScanDevicesAsync()
    {
        IsWorking = true;

        await BluetoothLEService.ScanForDevicesAsync();
        BluetoothDevices.Clear();

        foreach (var bluetoothDevice in BluetoothLEService.BluetoothDeviceList)
        {
            if (!BluetoothDevices.Contains(bluetoothDevice))
            {
                BluetoothDevices.Add(bluetoothDevice);
            }
        }
        if (BluetoothDevices.Count == 0)
        {
            await NotificationService.ShowToastAsync("Unable to find nearby Bluetooth LE devices. Try again.");
        }

        IsWorking = false;
    }


    async Task CheckBluetoothAvailabilityAsync()
    {
        if (IsWorking)
        {
            return;
        }


        if (!await BluetoothLEService.IsBluetoothAvailable())
        {
            return;
        }

        if (await BluetoothLEService.IsBluetoothOn())
        {
            await Shell.Current.DisplayAlert($"Bluetooth is on", $"You are good to go.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
        }

    }
}

