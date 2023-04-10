using System.Text;

namespace MauiBluetoothBLE.Services;

public class BluetoothLEService
{
    private NotificationService NotificationService;
    public BluetoothDevice SelectedBluetoothDevice { get; set; } = new();
    public List<BluetoothDevice> BluetoothDeviceList { get; private set; } = new List<BluetoothDevice>();
    public IBluetoothLE BluetoothLE { get; private set; }
    public IAdapter Adapter { get; private set; }
    public IDevice Device { get; private set; }
    public IService BluetoothConnectionService { get; private set; }
    public ICharacteristic BluetoothConnectionCharacteristic { get; private set; }

    // custom events
    public event Action StatusChanged;
    public event Action MessageReceived;

    private string _message;

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
        }
    }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            StatusChanged?.Invoke();
        }
    }

    public BluetoothLEService(NotificationService notificationService)
    {
        NotificationService = notificationService;
        BluetoothLE = CrossBluetoothLE.Current;
        //TODO refactor create Adapter class
        Adapter = CrossBluetoothLE.Current.Adapter;
        Adapter.ScanTimeout = 4000;
        Adapter.DeviceDiscovered += DeviceDiscovered;
        //Adapter.DeviceConnected += Adapter_DeviceConnected;
        Adapter.DeviceDisconnected += DeviceDisconnected;
        Adapter.DeviceConnectionLost += DeviceConnectionLost;
        BluetoothLE.StateChanged += AdapterStateChanged;
    }

    private async Task<bool> IsBluetoothAvailable()
    {
        if(!BluetoothLE.IsAvailable)
        {
            Debug.WriteLine($"Bluetooth is missing.");
            await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
            return false;
        }
        return true;
    }
    private async Task<bool> IsPermissionGranded()
    {
#if ANDROID
        PermissionStatus permissionStatus = await CheckBluetoothPermissions();
        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await RequestBluetoothPermissions();
            if (permissionStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert($"Bluetooth LE permissions", $"Bluetooth LE permissions are not granted.", "OK");
                return false;
            }
        }
#elif IOS
#elif MACCATALYST
#elif WINDOWS
#endif
        return true;
    }
    private async Task<bool> IsBluetoothOn()
    {
        if (!BluetoothLE.IsOn)
        {
#if MACCATALYST
            return true;
#endif
            Status = "Bluetooth is off.";
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            return false;
        }
        return true;
    }

    private async Task<bool> IsScanning()
    {
        if (Adapter.IsScanning)
        {
            Status = "Bluetooth adapter is scanning";
            await NotificationService.ShowToastAsync("Bluetooth adapter is scanning. Try again.");
            return true;
        }
        return false;
    }
    public async Task ScanAndConnectToKnownDeviceAsync()
    {
        try
        {
            if(!await IsBluetoothAvailable() || !await IsPermissionGranded() || !await IsBluetoothOn() || await IsScanning()) { return; }

            await SetStoredDevice();
            Status = "Connecting to bluetooth device";
            Device = await Adapter.ConnectToKnownDeviceAsync(SelectedBluetoothDevice.Id);
            Device.UpdateConnectionInterval(ConnectionInterval.High);
            Status = "Connection to device done";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable connect to known Bluetooth LE device {Device.Name}. ERROR: {ex.Message}.");
            await NotificationService.ShowToastAsync($"Unable connect to known Bluetooth LE device {Device.Name}.");
        } 
    }

    public async Task<List<BluetoothDevice>> ScanForDevicesAsync()
    {
        try
        {
            if (!await IsBluetoothAvailable() || !await IsPermissionGranded() || !await IsBluetoothOn() || await IsScanning()) { return null; }

            await Adapter.StartScanningForDevicesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to scan known Bluetooth LE devices: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to scan known Bluetooth LE devices", $"{ex.Message}.", "OK");
        }

        return BluetoothDeviceList;
    }

    private async Task SetStoredDevice()
    {

        if (SelectedBluetoothDevice.Id.Equals(Guid.Empty))
        {
            var device_name = await SecureStorage.Default.GetAsync("device_name");
            var device_id = await SecureStorage.Default.GetAsync("device_id");

            if (!string.IsNullOrEmpty(device_id))
            {
                SelectedBluetoothDevice.Name = device_name;
                SelectedBluetoothDevice.Id = Guid.Parse(device_id);
            } else
            {
                SelectedBluetoothDevice.Id = Uuids.HM10Service;
#if MACCATALYST
                SelectedBluetoothDevice.Id = Uuids.HM10ServiceMACOS;
#endif
            }

        }
    }

    public async Task ConnectToDeviceAsync()
    {
        try
        {
            if (Device != null && Device.State == DeviceState.Connected && Device.Id.Equals(SelectedBluetoothDevice.Id))
            {
                await NotificationService.ShowToastAsync($"{Device.Name} is already connected.");
                return;
            } else
            {
                await ScanAndConnectToKnownDeviceAsync();
            }

            if (Device.State == DeviceState.Connected)
            {
                BluetoothConnectionService = await Device.GetServiceAsync(Uuids.TISensorTagSmartKeys);
                Status = "Bluetooth device connected";
            }
            else
            {
                Status = $"{Device.Name} service connection failed";
                await NotificationService.ShowToastAsync($"{Device.Name} service connection failed.");
                return;
            }

            if (BluetoothConnectionService != null)
            {
                BluetoothConnectionCharacteristic = await BluetoothConnectionService.GetCharacteristicAsync(Uuids.TXRX);
            }
            else
            {
                Status = $"{Device.Name} characteristics connection failed";
                await NotificationService.ShowToastAsync($"{Device.Name} characteristics connection failed.");
                return;
            }

             if (BluetoothConnectionCharacteristic != null && BluetoothConnectionCharacteristic.CanUpdate)
            {
                await SecureStorage.Default.SetAsync("device_name", $"{Device.Name}");
                await SecureStorage.Default.SetAsync("device_id", $"{Device.Id}");

                BluetoothConnectionCharacteristic.ValueUpdated += ReceivedData;
                await BluetoothConnectionCharacteristic.StartUpdatesAsync();
                Status = "Done";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to connect to  Bluetooth: {ex.Message}");
            Status = $"Unable to connect to  device {Device?.Name}";
            await NotificationService.ShowToastAsync($"Unable to connect to  device {Device?.Name}");
        }
        
    }

    private void ReceivedData(object sender, CharacteristicUpdatedEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Characteristic.Value);
        Message += message;

        if (message.Contains('\n'))
        {
            Status = "Data received";
            MessageReceived?.Invoke();
            Message = null;
        }
    }

    public async Task SendData(string message)
    {
        try
        {
            Status = "Requesting data";
            await BluetoothConnectionCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(message));
        }
        catch (Exception)
        {
            Status = "Unable to request data";
        }
    }

    private void DeviceDiscovered(object sender, DeviceEventArgs e)
    {
        if (!BluetoothDeviceList.Any(d => d.Id == e.Device.Id))
        {
            BluetoothDeviceList.Add(new BluetoothDevice{Id = e.Device.Id, Name = e.Device.Name});
        }
    }

    private void DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Adapter.IsScanning)
            {
                Status = $"{e.Device.Name ?? "Device"} connection is lost.";
                await NotificationService.ShowToastAsync($"{e.Device.Name ?? "Device"} connection is lost.");
            }
        });
    }

    private void DeviceDisconnected(object sender, DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Adapter.IsScanning)
            {
                Status = $"{e.Device.Name ?? "Device"} is disconnected.";
                await NotificationService.ShowToastAsync($"{e.Device.Name ?? "Device"} is disconnected.");
            }
        });
    }


    private void AdapterStateChanged(object sender, BluetoothStateChangedArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Adapter.IsScanning)
            {
                Status = $"Bluetooth state changed to {e.NewState}.";
                await NotificationService.ShowToastAsync($"Bluetooth state changed to {e.NewState}.");
            }
        });
    }


#if ANDROID
    #region BluetoothPermissions
    public async Task<PermissionStatus> CheckBluetoothPermissions()
    {
        PermissionStatus status = PermissionStatus.Unknown;
        try
        {
            status = await Permissions.CheckStatusAsync<BluetoothLEPermissions>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to check Bluetooth LE permissions: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to check Bluetooth LE permissions", $"{ex.Message}.", "OK");
        }
        return status;
    }

    public async Task<PermissionStatus> RequestBluetoothPermissions()
    {
        PermissionStatus status = PermissionStatus.Unknown;
        try
        {
            status = await Permissions.RequestAsync<BluetoothLEPermissions>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to request Bluetooth LE permissions: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to request Bluetooth LE permissions", $"{ex.Message}.", "OK");
        }
        return status;
    }

    #endregion BluetoothPermissions
#elif IOS
#elif WINDOWS
#endif

}

