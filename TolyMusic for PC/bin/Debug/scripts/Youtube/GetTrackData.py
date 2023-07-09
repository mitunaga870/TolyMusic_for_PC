from json import JSONDecodeError
from ytmusicapi import YTMusic
import io, sys

# 引数を取得
args = sys.argv
if len(args) < 2:
    exit("引数が足りません")

#出力コードをUTF-8に
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
# API呼び出し
try:#本番時
    yt = YTMusic("scripts\\Youtube\\yt_outh.json", language="ja")
except JSONDecodeError:#デバッグ時
    yt = YTMusic("yt_outh.json", language="ja")

# ライブラリーの曲を取得
SONGS = yt.search(args[1], limit=1)

print(SONGS[0])