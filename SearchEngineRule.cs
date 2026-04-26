namespace Community.PowerToys.Run.Plugin.ExpressSearch;

public class SearchEngineRule
{
    /// <summary>
    /// ショートカットキー (例: "g", "b")
    /// </summary>
    public string Shortcut { get; set; } = string.Empty;

    /// <summary>
    /// 表示ラベル (例: "Google", "Bing")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 検索 URL テンプレート (例: "https://www.google.com/search?q=%s")
    /// </summary>
    public string QueryUrl { get; set; } = string.Empty;

    /// <summary>
    /// このルールを有効にするかどうか
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}