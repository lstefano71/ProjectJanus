using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Janus.App.Services
{
    public class UserUiSettingsService
    {
        private const string SettingsFileName = "UserUiSettings.json";
        private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        private static readonly SemaphoreSlim fileLock = new(1, 1);
        public static UserUiSettingsService Instance { get; } = new UserUiSettingsService();

        private UserUiSettingsService() { }

        public class UiSettings
        {
            public int MinutesBefore { get; set; } = 5;
            public int MinutesAfter { get; set; } = 5;
            public double MainWindowWidth { get; set; } = 900;
            public double MainWindowHeight { get; set; } = 400;
            public double ResultsSplitterPosition { get; set; } = 0.5; // Relative position (0-1)
            public double DetailsSplitterPosition { get; set; } = 0.33; // Relative position (0-1)
            // Add other UI parameters here as needed
        }

        public async Task<UiSettings> LoadAsync()
        {
            await fileLock.WaitAsync();
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(SettingsFilePath);
                        var settings = JsonSerializer.Deserialize<UiSettings>(json);
                        return settings ?? new UiSettings();
                    }
                    catch
                    {
                        return new UiSettings();
                    }
                }
                return new UiSettings();
            }
            finally
            {
                fileLock.Release();
            }
        }

        public async Task SaveAsync(UiSettings settings)
        {
            await fileLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(SettingsFilePath, json);
            }
            finally
            {
                fileLock.Release();
            }
        }
    }
}
