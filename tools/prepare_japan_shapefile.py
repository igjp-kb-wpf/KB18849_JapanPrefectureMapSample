"""
Natural Earth の行政区画データ(admin-1)から、日本の47都道府県だけを抜き出した
シェイプファイル(Shapefiles/japan_prefectures.*)を作るスクリプト。

使い方:
    pip install pyshp
    python tools/prepare_japan_shapefile.py

やっていること:
  1. Natural Earth の ne_10m_admin_1_states_provinces(1:10m、パブリックドメイン)をダウンロードして展開する
  2. iso_a2 == "JP" の47件を取り出し、iso_3166_2("JP-13" など)の数字部分を CODE 列(2桁の文字列)にする
  3. 英語名(NAME_EN)はマクロン付きの表記(Ōita, Kōchi など)をASCIIにする
     (DBFの文字コードの扱いを気にしなくて済むよう、属性はASCIIのみにしている)
  4. 図形(座標)は加工せずそのままコピーし、.prj(WGS84の経緯度)も一緒に出力する
"""
import io
import shutil
import unicodedata
import urllib.request
import zipfile
from pathlib import Path

import shapefile  # pyshp

SOURCE_URL = "https://naturalearth.s3.amazonaws.com/10m_cultural/ne_10m_admin_1_states_provinces.zip"
SOURCE_NAME = "ne_10m_admin_1_states_provinces"

root = Path(__file__).resolve().parent.parent
work = root / "tools" / "_work"
dest = root / "Shapefiles" / "japan_prefectures"


def ascii_name(text: str) -> str:
    return unicodedata.normalize("NFKD", text).encode("ascii", "ignore").decode("ascii")


def main() -> None:
    work.mkdir(parents=True, exist_ok=True)
    print("ダウンロード中:", SOURCE_URL)
    with urllib.request.urlopen(SOURCE_URL) as response:
        data = response.read()
    with zipfile.ZipFile(io.BytesIO(data)) as archive:
        archive.extractall(work)

    reader = shapefile.Reader(str(work / SOURCE_NAME), encoding="utf-8")
    items = []
    for shape_record in reader.shapeRecords():
        record = shape_record.record.as_dict()
        if record.get("iso_a2") == "JP":
            code = record["iso_3166_2"].split("-")[1]  # "JP-13" -> "13"
            items.append((code, ascii_name(record["name"]), shape_record.shape))

    items.sort(key=lambda item: item[0])
    assert len(items) == 47, f"都道府県の数が47ではありません: {len(items)}"

    dest.parent.mkdir(parents=True, exist_ok=True)
    writer = shapefile.Writer(str(dest), shapeType=shapefile.POLYGON)
    writer.field("CODE", "C", size=2)
    writer.field("NAME_EN", "C", size=24)
    for code, name_en, shape in items:
        writer.record(code, name_en)
        writer.shape(shape)
    writer.close()

    shutil.copyfile(work / f"{SOURCE_NAME}.prj", f"{dest}.prj")
    shutil.rmtree(work)
    print("作成しました:", dest)


if __name__ == "__main__":
    main()
