namespace MauiBluetoothBLE.Services;

public class BluetoothLEService
{
    public BluetoothDevice SelectedBluetoothDevice { get; set; } = new();
    public List<BluetoothDevice> BluetoothDeviceList { get; private set; } = new List<BluetoothDevice>();
    public IBluetoothLE BluetoothLE { get; private set; }
    public IAdapter Adapter { get; private set; }
    public IDevice Device { get; set; }

    private bool IsScanning = false;

    public BluetoothLEService()
    {
        BluetoothLE = CrossBluetoothLE.Current;
        Adapter = CrossBluetoothLE.Current.Adapter;
        Adapter.ScanTimeout = 4000;

        Adapter.DeviceDiscovered += DeviceDiscovered;
        Adapter.DeviceConnected += Adapter_DeviceConnected;
        Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
        Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;

        BluetoothLE.StateChanged += BluetoothLE_StateChanged;
    }

    private async Task<bool> IsBluetoothAvailable()
    {
        if (IsScanning)
        {
            return false;
        } else if(!BluetoothLE.IsAvailable)
        {
            Debug.WriteLine($"Bluetooth is missing.");
            await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
            return false;
        }
        return true;
    }
    private async Task<bool> isPermissionGranded()
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
#elif WINDOWS
#endif
        return true;
    }
    private async Task<bool> IsBluetoothOn()
    {
        if (!BluetoothLE.IsOn)
        {
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            return false;
        }
        return true;
    }
    public async Task ScanForKnownDeviceAsync()
    {
        try
        {
            if(!await IsBluetoothAvailable() || !await isPermissionGranded() || !await IsBluetoothOn()) { return; }
            //if(!await isPermissionGranded()) { return; }
            //if(!await isPermissionGranded()) { return; }

            await Adapter.StartScanningForDevicesAsync(Uuids.HM10Service);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to scan known Bluetooth LE devices: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to scan known Bluetooth LE devices", $"{ex.Message}.", "OK");
        } 
    }

    public async Task<List<BluetoothDevice>> ScanForDevicesAsync()
    {
        try
        {
            if (!await IsBluetoothAvailable() || !await isPermissionGranded() || !await IsBluetoothOn()) { return null; }

            await Adapter.StartScanningForDevicesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to scan known Bluetooth LE devices: {ex.Message}.");
            await Shell.Current.DisplayAlert($"Unable to scan known Bluetooth LE devices", $"{ex.Message}.", "OK");
        }

        return BluetoothDeviceList;
    }

    #region DeviceEventArgs
    private void DeviceDiscovered(object sender, DeviceEventArgs e)
    {
        if (!BluetoothDeviceList.Any(d => d.Id == e.Device.Id))
        {
            BluetoothDeviceList.Add(new BluetoothDevice{Id = e.Device.Id, Name = e.Device.Name});
        }
    }

    private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                //await ShowToastAsync($"{e.Device.Name} connection is lost.");
            }
            catch
            {
                //await ShowToastAsync($"Device connection is lost.");
            }
        });
    }

    private void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                //await ShowToastAsync($"{e.Device.Name} is connected.");
            }
            catch
            {
                //await ShowToastAsync($"Device is connected.");
            }
        });
    }

    private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                //await ShowToastAsync($"{e.Device.Name} is disconnected.");
            }
            catch
            {
                //await ShowToastAsync($"Device is disconnected.");
            }
        });
    }
    #endregion DeviceEventArgs

    #region BluetoothStateChangedArgs
    private void BluetoothLE_StateChanged(object sender, BluetoothStateChangedArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await ShowToastAsync($"Bluetooth state is {e.NewState}.");
            }
            catch
            {
                await ShowToastAsync($"Bluetooth state has changed.");
            }
        });
    }
    #endregion BluetoothStateChangedArgs

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

    public static async Task ShowToastAsync(string message)
    {
        ToastDuration toastDuration = ToastDuration.Long;
        IToast toast = Toast.Make(message, toastDuration);
        await toast.Show();
    }
}

