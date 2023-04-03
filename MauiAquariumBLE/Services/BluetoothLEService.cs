using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Text;
using System.Threading.Channels;
using System.Xml.Linq;

namespace MauiBluetoothBLE.Services;

public class BluetoothLEService : INotifyPropertyChanged
{
    public BluetoothDevice SelectedBluetoothDevice { get; private set; } = new();
    public List<BluetoothDevice> BluetoothDeviceList { get; private set; } = new List<BluetoothDevice>();
    public IBluetoothLE BluetoothLE { get; private set; }
    public IAdapter Adapter { get; private set; }
    public IDevice Device { get; private set; }
    public IService BluetoothConnectionService { get; private set; }
    public ICharacteristic BluetoothConnectionCharacteristic { get; private set; }

    private string _fullValue;

    private string _myProperty;
    public string MyProperty
    {
        get => _myProperty;
        set
        {
            _myProperty = value;
            OnPropertyChanged(nameof(MyProperty));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }




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

    private async Task<bool> IsScanning()
    {
        if (Adapter.IsScanning)
        {
            await ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
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

            Device = await Adapter.ConnectToKnownDeviceAsync(SelectedBluetoothDevice.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable connect to known Bluetooth LE device {Device.Name}. ERROR: {ex.Message}.");
            await ShowToastAsync($"Unable connect to known Bluetooth LE device {Device.Name}.");
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
            }

        }
    }

    public async Task ConnectToDeviceAsync()
    {
        MyProperty = "Connecting";
        try
        {
            if (Device != null && Device.State == DeviceState.Connected && Device.Id.Equals(SelectedBluetoothDevice.Id))
            {
                await ShowToastAsync($"{Device.Name} is already connected.");
                return;
            }

            await ScanAndConnectToKnownDeviceAsync();

            if (Device.State == DeviceState.Connected)
            {
                BluetoothConnectionService = await Device.GetServiceAsync(Uuids.TISensorTagSmartKeys);

            }
            else
            {
                await ShowToastAsync($"{Device.Name} connection failed.");
                return;
            }

            if (BluetoothConnectionService != null)
            {
                BluetoothConnectionCharacteristic = await BluetoothConnectionService.GetCharacteristicAsync(Uuids.TXRX);
            }
            else
            {
                await ShowToastAsync($"{Device.Name} connection failed.");
                return;
            }

            if (BluetoothConnectionCharacteristic != null && BluetoothConnectionCharacteristic.CanUpdate)
            {
                await SecureStorage.Default.SetAsync("device_name", $"{Device.Name}");
                await SecureStorage.Default.SetAsync("device_id", $"{Device.Id}");

                BluetoothConnectionCharacteristic.ValueUpdated += ReceivedData;
                await BluetoothConnectionCharacteristic.StartUpdatesAsync();

                await Send("inputs");

                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to connect to  Bluetooth: {ex.Message}");
            await ShowToastAsync($"{Device.Name} connection failed.");
        }
        MyProperty = "Done";
    }

    private void ReceivedData(object sender, CharacteristicUpdatedEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Characteristic.Value);
        _fullValue += message;

        if (message.Contains('\n'))
        {
            // Full value received, process it

            var a = _fullValue; //    ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
            _fullValue = null;
        }
    }

    private async Task Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await BluetoothConnectionCharacteristic.WriteAsync(data);
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

