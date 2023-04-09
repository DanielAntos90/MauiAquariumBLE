namespace MauiBluetoothBLE.Views;

public partial class HomeView : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomeView(HomeViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ConnectToDeviceAsync();
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
       
    }

    private async void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        await _viewModel.LedDimmingValueChange(e.NewValue);
    }
}