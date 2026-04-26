# Express Search for PowerToys Run

PowerToys Run から、定義済みの検索エンジンショートカットを使って素早く検索を実行するためのサードパーティプラグインです。

## 概要

Express Search は、PowerToys Run 上で `s g PowerToys` のように入力すると、設定済みの検索エンジンとキーワードを組み合わせて既定のブラウザーで検索を開くプラグインです。

検索エンジンの定義は `settings.json` で管理します。好みのエンジンを登録して即座に検索できます。

## 機能

- 検索エンジンごとにショートカットを定義できます
- `shortcut + keyword` 形式で素早く検索できます
- 検索エンジンごとに有効 / 無効を切り替えられます
- 設定は `settings.json` で管理できます
- 検索語は URL エンコードされて既定ブラウザーで開かれます

## 必須要件

- Windows 10 または Windows 11
- Microsoft PowerToys
- PowerToys Run が有効になっていること

## インストール方法

### 1. PowerToys を終了する

PowerToys が起動中の場合は終了します。サードパーティプラグインの配置や更新時は、先に閉じておく方法が案内されています。

### 2. プラグインを配置する

- [リリース一覧](https://github.com/habutaso/ExpressSearch_PowerToys/releases/)から`ExpressSearch-vx.y.z.zip`をダウンロードします。  
- `ExpressSearch/`フォルダを以下フォルダにコピーします。

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins
```

フォルダ構成の例:

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\
  ExpressSearch\
    Community.PowerToys.Run.Plugin.ExpressSearch.dll
    plugin.json
    Images\
    ...
```

### 3. PowerToys を再起動する

PowerToys を再起動します。

### 4. プラグインを有効化する

PowerToys Settings を開き、PowerToys Run のプラグイン一覧から Express Search が有効になっていることを確認します。

## 使い方

PowerToys Run を開いて、次の形式で入力します。

```text
s [shortcut] [keyword]
```

例:

```text
s g PowerToys
s gh PowerToys Run plugin
```

## 設定

設定ファイルは次の場所に保存されます。

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\ExpressSearch\settings.json
```

検索エンジンの追加、変更、削除はこの `settings.json` を直接編集してください。

### 設定項目

各検索エンジンは次のプロパティを持ちます。

- `shortcut`: 検索時に使う短い識別子
- `label`: 表示名
- `queryUrl`: URL
- `isEnabled`: 有効 / 無効

`queryUrl` には必ず `%s` を含めてください。`%s` が検索語に置き換えられます。

## settings.json Example

```json
{
  "Engines": [
    {
      "shortcut": "g",
      "label": "Google",
      "queryUrl": "https://www.google.com/search?q=%s",
      "isEnabled": true
    },
    {
      "shortcut": "gh",
      "label": "GitHub",
      "queryUrl": "https://github.com/search?q=%s",
      "isEnabled": true
    }
  ]
}
```

例えば、youtubeを追加したい場合は以下のように修正します。

```json
{
  "Engines": [
    {
      "shortcut": "g",
      "label": "Google",
      "queryUrl": "https://www.google.com/search?q=%s",
      "isEnabled": true
    },
    {
      "shortcut": "gh",
      "label": "GitHub",
      "queryUrl": "https://github.com/search?q=%s",
      "isEnabled": true
    }
    {
      "shortcut": "y",
      "label": "Youtube",
      "queryUrl": "https://www.youtube.com/results?search_query=%s",
      "isEnabled": true
    }
  ]
}
```

## Behavior

- 有効化されている検索エンジンのみ使用されます
- 検索語は URL エンコードされます
- 一致する `shortcut` がない場合はエラーメッセージを表示します
- `queryUrl` に `%s` がない場合は実行しません

## Notes

PowerToys Settings 側に表示される JSON テキストエリアは、現在設定の確認用途を想定しています。実際の設定変更は `settings.json` を直接編集してください。

設定を変更したあと、反映されない場合は PowerToys の再起動を試してください。PowerToys Run プラグイン仕様では、設定反映まわりはプラグイン側実装に依存する部分があります。

他のプラグインや設定とアクションワードが競合しないようにしてください。PowerToys Run では direct activation command の重複は避けるべきです。

## Troubleshooting

### プラグインが表示されない

- 配置先フォルダが正しいか確認してください
- `plugin.json` と DLL が正しく配置されているか確認してください
- PowerToys を再起動してください

PowerToys のサードパーティプラグインは、推奨フォルダに配置する必要があります。

### 検索しても何も起きない

- `settings.json` の JSON 構文が正しいか確認してください
- 対象エンジンの `isEnabled` が `true` か確認してください
- `queryUrl` に `%s` が含まれているか確認してください
- 既定ブラウザーが正しく設定されているか確認してください

### 設定が反映されない

- `settings.json` 保存後に PowerToys を再起動してください
- PowerToys Settings の表示内容ではなく、実ファイルの内容を確認してください
