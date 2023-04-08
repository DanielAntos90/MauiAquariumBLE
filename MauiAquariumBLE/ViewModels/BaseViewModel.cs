namespace MauiBluetoothBLE.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotWorking))]
    bool isWorking;

    [ObservableProperty]
    string title;

    public bool IsNotWorking => !IsWorking;
}

