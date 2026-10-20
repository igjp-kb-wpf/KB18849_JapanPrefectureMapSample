namespace JapanPrefectureMapSample;

/// <summary>
/// 都道府県ごとの統計データ。
/// Code はシェイプファイル(japan_prefectures.dbf)の CODE 列と一致させるキーで、
/// xamMapはこのキーを使って、シェイプの各要素にPopulationの値を結びつける。
/// </summary>
public class PrefectureData
{
    /// <summary>都道府県コード(JIS X 0401)。"01"〜"47" の2桁の文字列。</summary>
    public required string Code { get; init; }

    public required string NameJa { get; init; }

    /// <summary>人口(人)。コロプレス図の色分けに使う値。</summary>
    public required double Population { get; init; }
}
