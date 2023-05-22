import io, sys
from ytmusicapi import YTMusic

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
# API呼び出し
yt = YTMusic("scripts\\Youtube\\yt_outh.json", language="ja")

playlist_id = str(sys.argv[1])

# ライブラリーの曲を取得
SONGS = yt.get_playlist(playlist_id,10000)
print(SONGS["title"]+" length:"+str(len(SONGS["tracks"])))
for SONG in SONGS["tracks"]:
    print(SONG)
