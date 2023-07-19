using VathServer.ViewModels;

namespace VathServer;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<SettingPageView>();
		builder.Services.AddTransient<SessionView>();
		builder.Services.AddTransient<SettingPageViewModel>();
		builder.Services.AddTransient<SessionViewModel>();

		Routing.RegisterRoute(nameof(SessionView), typeof(SessionView));

		return builder.Build();
	}
}
