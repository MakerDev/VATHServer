using VathServer.ViewModels;

namespace VathServer;

public partial class SettingPageView : ContentPage
{
    public SettingPageView(SettingPageViewModel settingPageViewModel)
	{
		InitializeComponent();

		BindingContext = settingPageViewModel;
	}
}