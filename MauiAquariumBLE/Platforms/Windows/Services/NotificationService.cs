namespace MauiBluetoothBLE.Services;

public partial class NotificationService
{
    public partial async Task ShowNotification(string message)
    {
        ToastDuration toastDuration = ToastDuration.Long;
        IToast toast = Toast.Make(message, toastDuration);
        await toast.Show();
    }
}

