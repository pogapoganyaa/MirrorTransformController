「MirrorTransformController Unityにおける左右対称編集ツール」をDLしていただきありがとうございます。
Unity上でのGameObjectを左右対称に編集することを目的としたツールです。

使い方
1.MirrorTransformControllerがセットされたGameObjectをSceneに配置してください。
2.基準となるTransformを「RootTransform」に設定してください。(例：AvatarのRootとなるObject)
3.「Transformの自動取得」ボタンまたは「Transform数」を変更し、編集対象となるTransformを設定してください。
4.「左右対称編集の有効・無効」及び、Transform欄の左端のチェックをつけた状態で対象となるTransformの編集をしてください。
5.編集終了後は「左右対称編集の有効・無効」のチェックを外す、Objectの非アクティブ、ツールの削除などをし、ツールの無効化をしてください。

注意点
・Transformの自動取得は、RootTransformから編集対象のTransformまでの階層数とPositionを基に、対応するTransformを自動判定します。
・同じ条件のTransformが複数ある場合、正しいTransformを取得できない可能性があります。その場合は手動で対応してください。
・TransformのRotationの状態によって、対応する回転方向が取得できない場合があります。
「設定」内の「詳細設定」をチェックすることで、個別に設定することができます。

利用規約
・作者は、本ツールの利用によって生じたいかなる請求や損害、その他の義務について一切の責任を負わないものとします。
・本ツールの再配布を禁止します。

https://pogapoganyaa.booth.pm/items/4676415