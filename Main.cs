using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.ExpressSearch;

public class Main : IPlugin, IPluginI18n, ISettingProvider
{
    public static string PluginID => "919dc50a-20c7-4ccb-80d3-d37a30e1cd7c";

    private PluginInitContext? _context;
    private string? _iconPath;
    private Settings _settings = Settings.CreateDefault();

    private const string SettingsKey = "engines_json";
    private string _cachedEnginesJson = "[]";

    private readonly PluginAdditionalOption _enginesJsonOption = new()
    {
        Key = SettingsKey,
        DisplayLabel = "Search engines JSON",
        DisplayDescription = "Edit search engines as JSON array",
        PluginOptionType = PluginAdditionalOption.AdditionalOptionType.MultilineTextbox,
        TextValue = "[]"
    };

    public string Name => "Express Search";
    public string Description => "Search with configurable engines in the default browser";

    public void Init(PluginInitContext context)
    {
        _context = context;
        _iconPath = @"Images\\icon.dark.png";
        _settings = LoadSettings();

        _cachedEnginesJson = SerializeEnginesForTextBox(_settings.Engines);
        _enginesJsonOption.TextValue = _cachedEnginesJson;
    }

    public List<Result> Query(Query query)
    {
        var results = new List<Result>();

        // query.Terms はスペースで分割された検索文字列の配列です
        // 1. プラグインの呼び出しキーワードのみが入力された状態（例: "s "）
        if (query.Terms.Count == 0 || string.IsNullOrWhiteSpace(query.Terms[0]))
        {
            // 利用可能なエンジンをすべて一覧表示する
            foreach (var e in _settings.Engines.Where(e => e.IsEnabled))
            {
                var actionKey = string.IsNullOrEmpty(query.ActionKeyword) ? "" : $"{query.ActionKeyword} ";
                var autoCompleteString = $"{actionKey}{e.Shortcut} ";

                results.Add(new Result
                {
                    Title = $"{e.Label} で検索",
                    SubTitle = $"ショートカット: {e.Shortcut} (クリックまたはTabキーで選択)",
                    IcoPath = _iconPath,
                    Score = 100,
                    Action = _ =>
                    {
                        // ユーザーが項目をクリック/Enterした時に入力欄を更新する
                        _context?.API.ChangeQuery(autoCompleteString, true);
                        return false; // ランチャーは閉じない
                    }
                });
            }

            if (results.Count == 0)
            {
                results.Add(new Result
                {
                    Title = "検索エンジンが設定されていません",
                    SubTitle = "settings.json を編集してください",
                    IcoPath = _iconPath,
                    Score = 100,
                    Action = _ => false
                });
            }

            return results;
        }

        var engineKey = query.Terms[0].Trim().ToLowerInvariant();
        var keyword = string.Join(" ", query.Terms.Skip(1)).Trim();

        var engine = _settings.Engines
            .Where(e => e.IsEnabled)
            .FirstOrDefault(e => string.Equals(e.Shortcut, engineKey, StringComparison.OrdinalIgnoreCase));

        // 2. 入力された最初の文字がショートカットと一致しない場合
        if (engine is null)
        {
            results.Add(new Result
            {
                Title = $"不明なエンジン: {engineKey}",
                SubTitle = BuildUsageText(),
                IcoPath = _iconPath,
                Score = 100,
                Action = _ => false
            });
            return results;
        }

        if (string.IsNullOrWhiteSpace(engine.QueryUrl) || !engine.QueryUrl.Contains("%s", StringComparison.Ordinal))
        {
            results.Add(new Result
            {
                Title = $"無効なURL設定: {engine.Shortcut}",
                SubTitle = "QueryUrlに %s を含める必要があります",
                IcoPath = _iconPath,
                Score = 100,
                Action = _ => false
            });
            return results;
        }

        // 3. ショートカットは入力されたが、キーワードがまだ入力されていない状態（例: "s g" または "s g "）
        if (string.IsNullOrWhiteSpace(keyword))
        {
            results.Add(new Result
            {
                Title = $"{engine.Label} で検索...",
                SubTitle = "続けて検索キーワードを入力してください",
                IcoPath = _iconPath,
                Score = 100,
                Action = _ => false // まだ検索キーワードがないので何もしない
            });
            return results;
        }

        // 4. キーワードまで入力された状態（例: "s g 検索テスト"）
        var url = BuildUrl(engine.QueryUrl, keyword);
        results.Add(new Result
        {
            Title = $"{engine.Label} で検索: {keyword}",
            SubTitle = url,
            IcoPath = _iconPath,
            Score = 100,
            Action = _ => OpenInDefaultBrowser(url) // ブラウザを開いてランチャーを閉じる
        });

        return results;
    }

    public string GetTranslatedPluginTitle() => Name;
    public string GetTranslatedPluginDescription() => Description;

    public IEnumerable<PluginAdditionalOption> AdditionalOptions
    {
        get
        {
            _enginesJsonOption.TextValue = _cachedEnginesJson;
            return new[] { _enginesJsonOption };
        }
    }

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var json = settings.AdditionalOptions
            .FirstOrDefault(x => x.Key == SettingsKey)
            ?.TextValue;

        if (string.IsNullOrWhiteSpace(json))
        {
            Log.Warn("engines_json is empty", GetType());
            return;
        }

        var normalizedInput = NormalizeJsonText(json);

        if (string.Equals(normalizedInput, NormalizeJsonText(_cachedEnginesJson), StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            var engines = JsonSerializer.Deserialize<List<SearchEngineRule>>(normalizedInput, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (engines is null)
            {
                Log.Warn("engines_json deserialized to null", GetType());
                return;
            }

            var nextSettings = new Settings
            {
                Engines = engines
            };

            nextSettings.Normalize();

            _settings = nextSettings;
            SaveSettings(_settings);

            _cachedEnginesJson = SerializeEnginesForTextBox(_settings.Engines);
            _enginesJsonOption.TextValue = _cachedEnginesJson;

            Log.Info($"Settings updated. Engine count = {_settings.Engines.Count}", GetType());
        }
        catch (Exception ex)
        {
            Log.Exception("Failed to parse engines_json", ex, GetType());
        }
    }

    public Control CreateSettingPanel() => throw new NotImplementedException();

    private Settings LoadSettings()
    {
        try
        {
            var path = GetSettingsPath();

            if (!File.Exists(path))
            {
                var defaults = Settings.CreateDefault();
                SaveSettings(defaults);
                Log.Info("settings.json not found. Created default settings.", GetType());
                return defaults;
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                var defaults = Settings.CreateDefault();
                SaveSettings(defaults);
                Log.Warn("settings.json was empty. Recreated default settings.", GetType());
                return defaults;
            }

            var settings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settings is null)
            {
                var defaults = Settings.CreateDefault();
                SaveSettings(defaults);
                Log.Warn("settings.json could not be deserialized. Recreated default settings.", GetType());
                return defaults;
            }

            settings.Normalize();
            Log.Info($"settings.json loaded. Engine count = {settings.Engines.Count}", GetType());
            return settings;
        }
        catch (Exception ex)
        {
            Log.Exception("Failed to load settings", ex, GetType());

            var defaults = Settings.CreateDefault();

            try
            {
                SaveSettings(defaults);
            }
            catch
            {
            }

            return defaults;
        }
    }

    private void SaveSettings(Settings settings)
    {
        try
        {
            settings.Normalize();

            var path = GetSettingsPath();
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Log.Exception("Failed to save settings", ex, GetType());
        }
    }

    private string GetSettingsPath()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(
            baseDir,
            "Microsoft",
            "PowerToys",
            "PowerToys Run",
            "Plugins",
            "ExpressSearch",
            "settings.json");
    }

    private static string BuildUrl(string template, string keyword)
    {
        return template.Replace("%s", Uri.EscapeDataString(keyword), StringComparison.Ordinal);
    }

    private static bool OpenInDefaultBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string BuildUsageText()
    {
        var engines = _settings.Engines
            .Where(e => e.IsEnabled)
            .Select(e => $"{e.Shortcut}={e.Label}")
            .ToArray();

        return engines.Length == 0
            ? "No engines configured. Edit settings.json."
            : $"Available engines: {string.Join(", ", engines)}";
    }

    private static string SerializeEnginesForTextBox(List<SearchEngineRule> engines)
    {
        return JsonSerializer.Serialize(engines, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string NormalizeJsonText(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return json.Trim();
        }
    }
}