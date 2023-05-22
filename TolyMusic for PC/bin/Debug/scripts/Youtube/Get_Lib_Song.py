from ytmusicapi import YTMusic
import io, sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# API呼び出し
yt = YTMusic("script\\Youtube\\yt_outh.json", language="ja")

# ライブラリーの曲を取得
SONGS = yt.get_library_songs(10000)
for SONG in SONGS:
    print(SONG)