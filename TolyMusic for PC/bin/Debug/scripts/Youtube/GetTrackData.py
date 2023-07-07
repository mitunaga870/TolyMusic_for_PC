from ytmusicapi import YTMusic
import io, sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# API呼び出し
yt = YTMusic("scripts\\Youtube\\yt_outh.json", language="ja")

# ライブラリーの曲を取得
SONG = yt.get_song("kzXj1ZMg9Gg")
