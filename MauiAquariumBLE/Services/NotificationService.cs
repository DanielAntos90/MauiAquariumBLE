namespace MauiBluetoothBLE.Services
{
    public partial class NotificationService
    {
        public partial Task ShowNotification(string message);

        public static async Task ShowToastAsync(string message)
        {
            ToastDuration toastDuration = ToastDuration.Long;
            IToast toast = Toast.Make(message, toastDuration);
            await toast.Show();
        }
    }
}

