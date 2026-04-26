namespace Community.PowerToys.Run.Plugin.ExpressSearch;

public static class DefaultSettings
{
    public static IReadOnlyList<SearchEngineRule> GetDefaultEngines()
    {
        return new List<SearchEngineRule>
        {
            new()
            {
                Shortcut = "g",
                Label = "Google",
                QueryUrl = "https://www.google.com/search?q=%s",
                IsEnabled = true
            },
            new()
            {
                Shortcut = "b",
                Label = "Bing",
                QueryUrl = "https://www.bing.com/search?q=%s",
                IsEnabled = true
            }
        };
    }
}