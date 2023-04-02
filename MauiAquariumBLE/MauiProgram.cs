namespace MauiBluetoothBLE;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

        // Initialise the toolkit
        builder.UseMauiApp<App>().UseMauiCommunityToolkit();

        builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("fa_solid.ttf", "FontAwesome");
				fonts.AddFont("fa-brands-400.ttf", "FontAwesomeBrands");
            });

        builder.Services.AddSingleton<BluetoothLEService>();

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);

        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<HomeView>();

        builder.Services.AddSingleton<ScanDevicesViewModel>();
        builder.Services.AddSingleton<ScanDevicesView>();

        builder.Services.AddSingleton<SettingsPageViewModel>();
        builder.Services.AddSingleton<SettingsPage>();

        builder.Services.AddSingleton<AboutPageViewModel>();
        builder.Services.AddSingleton<AboutPage>();

        return builder.Build();
	}
}
