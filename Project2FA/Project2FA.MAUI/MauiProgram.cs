using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Project2FA.Core.Services.JSON;
using Project2FA.MAUI.Controls;
using Project2FA.MAUI.Services.JSON;
using Project2FA.MAUI.ViewModels;
using Project2FA.MAUI.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Project2FA.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        //https://github.com/microsoft/fluentui-system-icons/tree/master/fonts
        var builder = MauiApp.CreateBuilder();
		builder
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FluentSystemIcons-Filled.ttf", "FluentSystemIconsFilled");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentSystemIconsRegular");
            });

        builder.Services.AddSingleton<BlankPage>();
        builder.Services.AddSingleton<BlankPageViewModel>();

        builder.Services.AddSingleton<WelcomePage>();
        builder.Services.AddSingleton<WelcomePageViewModel>();

        builder.Services.AddSingleton<AccountCodePage>();
        builder.Services.AddSingleton<AccountCodePageViewModel>();

        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<LoginPageViewModel>();

        builder.Services.AddSingleton<UseDataFilePage>();
        builder.Services.AddSingleton<UseDataFilePageViewModel>();

        builder.Services.AddSingleton<FileActivationPage>();
        builder.Services.AddSingleton<FileActivationPageViewModel>();

        builder.Services.AddSingleton<INewtonsoftJSONService>(new NewtonsoftJSONService());
        builder.Services.AddSingleton<ISerializationService>(new SerializationService());


        builder.ConfigureMauiHandlers(collection =>
        {
#if __ANDROID__
            collection.AddHandler(typeof(SvgImageSource), typeof(Platforms.Android.SvgImageSourceHandler));
            Project2FA.MAUI.Platforms.Android.SvgImage.Init(Platform.AppContext);
#endif
#if __IOS__
            collection.AddHandler(typeof(SvgImageSource), typeof(Project2FA.MAUI.Platforms.iOS.SvgImageSourceHandler));
            Project2FA.MAUI.Platforms.iOS.SvgImage.Init();
#endif
        });

        return builder.Build();
	}

    public static async Task FileActivationIOS()
    {
        await Shell.Current.GoToAsync("//" + nameof(FileActivationPage));
    }
}
