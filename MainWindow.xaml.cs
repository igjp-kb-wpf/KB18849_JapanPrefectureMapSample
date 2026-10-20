using Infragistics;
using Infragistics.Controls.Maps;
using System.Windows;
using System.Windows.Controls;

namespace JapanPrefectureMapSample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var viewModel = new MainViewModel();
        DataContext = viewModel;

        LoadPrefectureShapes();
        BindPopulation(viewModel);
    }

    /// <summary>
    /// 地図の形(シェイプファイル)を読み込む。
    /// ShapeFileReader はファイルパスで読み込むため、実行ファイルの場所を基準にした
    /// 絶対パスを指定する(相対パスは実行方法によって基準が変わるため避ける)。
    /// パスは拡張子なし。同じ名前の .shp / .dbf などが必要。
    /// </summary>
    private void LoadPrefectureShapes()
    {
        var reader = new ShapeFileReader
        {
            Uri = global::System.IO.Path.Combine(AppContext.BaseDirectory, "Shapefiles", "japan_prefectures"),
        };

        // シェイプファイルの CODE 列を、地図要素の Name(結びつけ用のキー)として読み込む。
        var converter = new DataMapping.Converter();
        reader.DataMapping = converter.ConvertFromString("Name=CODE") as DataMapping;

        PrefectureLayer.Reader = reader;
    }

    /// <summary>
    /// 自前の統計データ(人口)を、地図の各要素に結びつける。
    /// レイヤー側の DataMapping(Name=Code; Value=Population)により、
    /// データの Code が同じ Name を持つ要素へ、Population が Value として割り当てられる。
    /// その Value に応じて、Choropleth モードが色を決める。
    /// </summary>
    private void BindPopulation(MainViewModel viewModel)
    {
        PrefectureLayer.DataSource = viewModel.Prefectures;
        PrefectureLayer.DataBind();
    }

    private void JapanMap_ElementHover(object sender, MapElementHoverEventArgs e)
    {
        var prefectureName = (e.Element?.Layer?.DataSource as IEnumerable<PrefectureData>)?
            .FirstOrDefault(x => x.Code == e.Element?.Name)?.NameJa ?? e.Element?.Name ?? string.Empty;
        var formattedValue = e.Element is null
            ? string.Empty
            : e.Element.Value.ToString("N0");
        var toolTipText = $"{prefectureName} : {formattedValue}";
        myToolTip.Content = toolTipText;

        myToolTip.IsOpen = true;
    }

    private void JapanMap_ElementUnhover(object sender, MapElementHoverEventArgs e)
    {
        myToolTip.IsOpen = false;
    }
}
