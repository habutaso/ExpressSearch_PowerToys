using System.Text.Json;

namespace Community.PowerToys.Run.Plugin.ExpressSearch;

public class Settings
{
    public List<SearchEngineRule> Engines { get; set; } = new();

    /// <summary>
    /// デフォルト設定 (Google / Bing) を返す。
    /// </summary>
    public static Settings CreateDefault()
    {
        return new Settings
        {
            Engines = DefaultSettings.GetDefaultEngines().Select(e => new SearchEngineRule
            {
                Shortcut = e.Shortcut,
                Label = e.Label,
                QueryUrl = e.QueryUrl,
                IsEnabled = e.IsEnabled
            }).ToList()
        };
    }

    /// <summary>
    /// 文字列のTrimや空行の削除など、軽い正規化を行う。
    /// </summary>
    public void Normalize()
    {
        foreach (var rule in Engines)
        {
            rule.Shortcut = rule.Shortcut?.Trim() ?? string.Empty;
            rule.Label = rule.Label?.Trim() ?? string.Empty;
            rule.QueryUrl = rule.QueryUrl?.Trim() ?? string.Empty;
        }

        // Shortcut と QueryUrl が両方空のルールは削除
        Engines = Engines
            .Where(r => !string.IsNullOrWhiteSpace(r.Shortcut) ||
                        !string.IsNullOrWhiteSpace(r.QueryUrl))
            .ToList();
    }

    /// <summary>
    /// JSON 文字列から HelloSettings を復元するヘルパー。
    /// </summary>
    public static Settings FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateDefault();
        }

        var settings = JsonSerializer.Deserialize<Settings>(json);
        if (settings is null)
        {
            return CreateDefault();
        }

        settings.Normalize();
        return settings;
    }

    /// <summary>
    /// 設定を JSON 文字列にシリアライズするヘルパー。
    /// </summary>
    public string ToJson()
    {
        Normalize();

        return JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}