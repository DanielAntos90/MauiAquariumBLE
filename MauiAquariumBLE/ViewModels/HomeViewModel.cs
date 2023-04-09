namespace MauiBluetoothBLE.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public IAsyncRelayCommand OnLedStatusButtonClicked { get; private set; }

    BluetoothLEService BluetoothLEService;

    [ObservableProperty]
    string bluetoothStatus = "Unknown";

    [ObservableProperty]
    TimeSpan currentTime = TimeSpan.Parse("00:00");
    [ObservableProperty]
    TimeSpan ledOnTime = TimeSpan.Parse("00:00");
    [ObservableProperty]
    TimeSpan ledOffTime = TimeSpan.Parse("00:00");

    [ObservableProperty]
    int ledBrightness = 0;
    [ObservableProperty]
    int ledDimmingMinutes = 0;
    [ObservableProperty]
    public List<int> numbers = Enumerable.Range(0, 101).ToList();

    [ObservableProperty]
    DateTime currentDate = DateTime.ParseExact("01.01.1990", "dd.MM.yyyy", null);

    [ObservableProperty]
    string ledStatusButtonSource = "led_q.png";

    [ObservableProperty]
    string bluetoothStatusImage = "bluetooth_disconected.png";

    [ObservableProperty]
    bool isDataReceived = false;

    public HomeViewModel(BluetoothLEService bluetoothLEService)
    {
        Title = "Home";

        BluetoothLEService = bluetoothLEService;
        BluetoothLEService.StatusChanged += ReadBluetoothStatus;
        BluetoothLEService.MessageReceived += ReadBluetoothMessage;

        OnLedStatusButtonClicked = new AsyncRelayCommand(ChangeLed);
    }

    private void ReadBluetoothStatus()
    {
        BluetoothStatus = BluetoothLEService.Status;
        BluetoothStatusImage = BluetoothLEService.Device?.State == DeviceState.Connected ? "bluetooth.png" : "bluetooth_disconected.png";
    }
    private void ReadBluetoothMessage()
    {
        if (BluetoothLEService.Message.StartsWith("ArduinoOutputs"))
        {
            //ArduinoOutputs;22:27;30.03.2023;10:00;19:00;42;42;led off\n
            string[] message = BluetoothLEService.Message.Split(";");
            CurrentTime = TimeSpan.Parse(message[1]);
            CurrentDate = DateTime.ParseExact(message[2], "dd.MM.yyyy", null);
            LedOnTime = TimeSpan.Parse(message[3]);
            LedOffTime = TimeSpan.Parse(message[4]);
            LedBrightness = int.TryParse(message[5], out var parsedValue) ? parsedValue : 0;
            LedDimmingMinutes = int.TryParse(message[6], out parsedValue) ? parsedValue : 0;
            LedStatusButtonSource = message[7].Contains("led on") ? "led_on.png" : "led_off.png";
        }
        else if (BluetoothLEService.Message.StartsWith("led on"))
        {
            LedStatusButtonSource = "led_on.png";
            BluetoothStatus = "Light changed succesfully";
        }
        else if (BluetoothLEService.Message.StartsWith("led off"))
        {
            LedStatusButtonSource = "led_off.png";
            BluetoothStatus = "Light changed succesfully";
        }
        IsDataReceived = true;
    }

    public async Task ConnectToDeviceAsync()
    {
        IsWorking = true; //TODO use custom event for IsWorking
        IsDataReceived = false;
        await BluetoothLEService.ConnectToDeviceAsync();
        if(BluetoothLEService.Device != null && BluetoothLEService.Device.Name != null)
        {
            Title = BluetoothLEService.Device.Name;
            await BluetoothLEService.SendData("inputs");
        }
        
        IsWorking = false;
    }

    private async Task ChangeLed()
    {
        IsDataReceived = false;
        var command = LedStatusButtonSource.Contains("led_on") ? "led off" : "led on";
        LedStatusButtonSource = "led_q.png";
        await BluetoothLEService.SendData($"turn {command}");
    }              
}
