using Microsoft.Extensions.Logging;

namespace EntryTest;

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

		builder.Services.AddSingleton<NewPage1ViewModel>();
		builder.Services.AddSingleton<NewPage2ViewModel>();
		builder.Services.AddSingleton<NewPage1>();
		builder.Services.AddSingleton<NewPage2>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

