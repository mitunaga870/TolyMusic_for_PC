<h1>TolyMusic for PC</h1>
C#と一部にpython,javascriptを用いた音楽プレーヤーです<br>
wpfを用いた最初に作成したプロジェクトです<br>

以下の楽曲を単一のライブラリで聞くことができます
<ol>
  <li>ローカル上のオーディオファイル(wav,flac,mp3,etc...)</li>
  <li>Youtube上の動画(埋め込み再生に対応している必要があります）</li>
</ol>
プレイリストの作成・再生はまだ実装することはできていませんが、他の基本的な音楽プレイヤーの機能を持っています。<br>
それと同時に、管理にSQLデータベースを用いることで、設定した楽曲やアーティストの情報を常に同期させ、すべての端末で同様に扱うことができます。<br>
<br>
pythonを利用することで、youtubemusicからの楽曲取り込みを容易に行うことができます。
<br>
現在、パッケージ化されたバージョンはありませんが、最新のブランチを用いてビルドし、ビルドを行ったファイルにDebug内のScriptsファイルをコピーすることで利用することができます。<br>

スクリーンショット：
![image](https://github.com/mitunaga870/TolyMusic_for_PC/assets/94438265/089cbb40-8e93-4fd9-9831-4bb107e11f00)
![image](https://github.com/mitunaga870/TolyMusic_for_PC/assets/94438265/ffaf5ba6-c6a3-40dd-a3a4-2c71ce9236c4)
