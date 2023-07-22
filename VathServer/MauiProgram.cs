using VathServer.ViewModels;
using VathServer.Interfaces;
using VathServer.Views;
#if WINDOWS
using VathServer.Platforms.Windows;
#elif MACCATALYST
using VathServer.Platforms.MacCatalyst;
#endif

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
		builder.Services.AddTransient<FinalResultView>();
		builder.Services.AddTransient<FinalResultViewModel>();
#if MACCATALYST
		builder.Services.AddSingleton<IMultipeerManager, MacMultipeerManager>();
#elif WINDOWS
        builder.Services.AddSingleton<IMultipeerManager, WindowsMultipeerManager>();
#endif

		Routing.RegisterRoute(nameof(SessionView), typeof(SessionView));
		Routing.RegisterRoute(nameof(FinalResultView), typeof(FinalResultView));

		return builder.Build();
	}
}
