# XamMap: 都道府県別の塗り分け(コロプレス図)サンプル

`XamMap` で、日本の47都道府県を**人口の大きさに応じて塗り分ける**地図を表示するサンプルです。
地図の形はシェイプファイル、色分けに使う値は自前のデータ(`PrefectureData`)から取り、
両者を**キー(都道府県コード)で結びつけています**。
自社の売上や件数など、任意の統計値に差し替えれば、そのまま同じ形の地図になります。

出典・注記は、画面下部と「データについて」に記載しています。

## 前提

* .NET 10 SDK (`net10.0-windows`)
* Infragistics WPF 製品版ライセンス(有効なプロダクトキーが紐付いたInfragisticsアカウント)
* Infragistics 2026.1 以降(.NET 10.0対応版)



## ファイル構成

|ファイル|役割|
|-|-|
|`Shapefiles/japan\_prefectures.\*`|47都道府県のシェイプファイル(`.shp` `.shx` `.dbf` `.prj`)。属性は `CODE`(2桁のコード)と `NAME\_EN`(英語名)のみ|
|`PrefectureData.cs`|都道府県ごとの統計データ(`Code`、`NameJa`、`Population`)|
|`MainViewModel.cs`|47都道府県の人口データ(令和2年国勢調査)|
|`MainWindow.xaml`|`XamMap`・色分けの設定・凡例・右側の一覧|
|`MainWindow.xaml.cs`|シェイプファイルの読み込みと、人口データの結びつけ|
|`tools/prepare\_japan\_shapefile.py`|シェイプファイルを再生成するスクリプト(データの出どころの記録を兼ねる)|

## 仕組み

### 1\. 地図の形: `ShapeFileReader`

`XamMap` は `MapLayer` に `ShapeFileReader` を設定してシェイプファイルを読み込み、
シェイプ1つを1つの `MapElement` として描画します(公式ドキュメント「Binding Shapefiles」)。



```csharp
var reader = new ShapeFileReader
{
    Uri = Path.Combine(AppContext.BaseDirectory, "Shapefiles", "japan\_prefectures"),
};
var converter = new DataMapping.Converter();
reader.DataMapping = converter.ConvertFromString("Name=CODE") as DataMapping;
PrefectureLayer.Reader = reader;
```

### 2\. 色分け: `Choropleth` + `Brushes` + `ValueScale`

`MapLayer` の `FillMode="Choropleth"` にすると、要素の `Value` に応じて、`Brushes`(色の並び)から
色が選ばれます(公式ドキュメント「Using Fill Modes」)。値と色の対応は `ValueScale` で決めます。

* `Brushes`: 薄い青から濃い青までの5色(値が小さい順)
* `ValueScale`: **対数スケール**(`LogarithmicScale`)で、範囲は50万〜1,500万。
人口は東京などに極端に偏っているため、線形スケールだとほとんどの県が最も薄い色になってしまう。

### 3\. 形と数値の結びつけ: キーによる結合

シェイプファイルには人口の列がありません。公式ドキュメント「Binding Xml Data」の手順に沿って、
自前のデータを `MapLayer.DataSource` に渡し、**同じキーを持つ要素に値を割り当て**ています。

||キーになるもの|
|-|-|
|シェイプ側|`ShapeFileReader.DataMapping = "Name=CODE"`(`.dbf` の `CODE` 列を要素の `Name` にする)|
|データ側|`MapLayer.DataMapping = "Name=Code; Value=Population"`(`PrefectureData.Code` を `Name` に、`Population` を `Value` にする)|

```csharp
PrefectureLayer.DataSource = viewModel.Prefectures;
PrefectureLayer.DataBind();
```

## データについて

### 出典・注記

画面下部とこのREADMEに、次の出典・注記を記載しています。このサンプルを転載・再配布する場合も、同じ内容を残してください。

> \*\*人口:\*\* 「令和2年国勢調査 人口等基本集計結果」(総務省統計局)(https://www.stat.go.jp/data/kokusei/2020/kekka.html)を加工して作成(2020年10月1日現在)
>
> \*\*地図:\*\* Made with Natural Earth.(`ne\_10m\_admin\_1\_states\_provinces`、1:10m)。日本の47都道府県のみを抽出し、属性を `CODE` と `NAME\_EN` に整理(図形は加工なし)。
>
> 簡略化された小縮尺のデータのため、公式の行政界とは細部(島の扱い・境界線)が異なります。境界の正確さが求められる用途には使用できません。

### 人口データ

* 総務省統計局「令和2年国勢調査 人口等基本集計結果」の都道府県別総人口(2020年10月1日現在)から、
47都道府県の値を抜き出して `MainViewModel.cs` に記述しています(これが上記の「加工」にあたります)。
* 47都道府県の値の合計は **126,146,099人**で、全国の総人口と一致しています。
* **令和2年(2020年)の値です。** 令和7年(2025年)国勢調査の結果も公表されています
(人口速報集計。確定値は令和8年9月に公表予定とされていました)。
最新の値を使う場合は、`MainViewModel.cs` の値を差し替え、出典の年次も合わせて更新してください。
* 業務で使う場合は、総務省統計局の公表資料で最新・正確な値を確認してください。

### 地図データ

* Natural Earth のデータは、パブリックドメインとして公開されており、利用に許可は不要です。
出典の表記も不要ですが、慣例に従い「Made with Natural Earth.」を表示しています。
* [Natural Earth](https://www.naturalearthdata.com/) の `ne\_10m\_admin\_1\_states\_provinces`(1:10m、行政区画)から、
日本の47件だけを抜き出して作成しています(手順は `tools/prepare\_japan\_shapefile.py`。
再実行して、同梱のファイルとバイト単位で同じものが生成されることを確認済み)。
* 図形は加工せず、そのままコピーしています。属性は `CODE` と `NAME\_EN` の2つだけにしました。
DBFの文字コードの扱いを気にしなくて済むよう、英語名のマクロン(Ōita → Oita)を除いてASCIIにしています。
日本語の名前は、シェイプファイルではなく `PrefectureData.NameJa` に持たせています。
* 簡略化された小縮尺のデータのため、**公式の行政界とは細部が異なります**。
境界の正確さが求められる用途には向きません。

