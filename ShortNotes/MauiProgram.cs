﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ShortNotes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<MainPage>();
#if ANDROID
            builder.Services.AddSingleton<ISpeechToText, Platforms.SpeechToTextImplementation>();
#endif
            return builder.Build();
        }
    }
}