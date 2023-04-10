namespace MauiBluetoothBLE.ViewModels;

public partial class ScanDevicesViewModel : BaseViewModel
{
    private BluetoothLEService BluetoothLEService;
    private NotificationService NotificationService;

    public ObservableCollection<BluetoothDevice> DeviceCandidates { get; } = new();

    public IAsyncRelayCommand GoToHomeViewAsyncCommand { get; }
    public IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; }
    public IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; }

    public ScanDevicesViewModel(BluetoothLEService bluetoothLEService, NotificationService notificationService)
    {
        Title = $"Scan and select device";

        BluetoothLEService = bluetoothLEService;
        NotificationService = notificationService;

        DeviceCandidates = new ObservableCollection<BluetoothDevice>(BluetoothLEService.BluetoothDeviceList);

        GoToHomeViewAsyncCommand = new AsyncRelayCommand<BluetoothDevice>(async (devicecandidate) => await GoToHomeViewAsync(devicecandidate));

        ScanNearbyDevicesAsyncCommand = new AsyncRelayCommand(ScanDevicesAsync);
        CheckBluetoothAvailabilityAsyncCommand = new AsyncRelayCommand(CheckBluetoothAvailabilityAsync);
    }

    async Task GoToHomeViewAsync(BluetoothDevice deviceCandidate)
    {
        if (IsWorking)
        {
            await NotificationService.ShowToastAsync("Bluetooth adapter is scanning. Try again.");
            return;
        }

        if (deviceCandidate == null)
        {
            return;
        }

        BluetoothLEService.SelectedBluetoothDevice = deviceCandidate;

        Title = $"{deviceCandidate.Name}";

        await Shell.Current.GoToAsync("//HomeView", true);
    }

    async Task ScanDevicesAsync()
    {
        IsWorking = true;

        await BluetoothLEService.ScanForDevicesAsync();
        foreach (var deviceCandidate in BluetoothLEService.BluetoothDeviceList)
        {
            DeviceCandidates.Add(deviceCandidate);
        }
        if (DeviceCandidates.Count == 0)
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

        try
        {
            //TODO refactor
            if (!BluetoothLEService.BluetoothLE.IsAvailable)
            {
                Debug.WriteLine($"Error: Bluetooth is missing.");
                await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
                return;
            }

            if (BluetoothLEService.BluetoothLE.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is on", $"You are good to go.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to check Bluetooth availability: {ex.Message}");
            await Shell.Current.DisplayAlert($"Unable to check Bluetooth availability", $"{ex.Message}.", "OK");
        }
    }
}

