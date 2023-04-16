#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PogapogaEditor.Component
{
    [CustomEditor(typeof(MirrorTransformController))]
    public class MirrorTransformControllerEditor : Editor
    {
        #region // 変数宣言
        MirrorTransformController mirrorTransformController;
        private float _spacePixels = 5;
        private float _toggleWidth = 20f;
        private bool _settingIsOpen = false;
        private bool _advancedSettingIsOpen = false;
        private bool _transformListIsOpen = true;
        private string[] _cartesianAxis = { "X", "Y", "Z", "W" };
        private string[] _rotationSign = { "R", "F" };
        private string _getButtonText = "Transformの自動取得";
        private string _toolName = "MirrorTransformController";
        private Vector2 _scrollPosition = Vector2.zero;
        #endregion

        private void OnEnable()
        {
            mirrorTransformController = (MirrorTransformController)target;
        }

        public override void OnInspectorGUI()
        {
            string undoMessage;

            //base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            bool _isRootTransform = mirrorTransformController.rootTransform != null;

            #region // RootTransform
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("RootTransform");
                Transform _rootTransform = EditorGUILayout.ObjectField(mirrorTransformController.rootTransform, typeof(Transform), true) as Transform;
                // RottoTransformに変更があったの場合
                if (_rootTransform != mirrorTransformController.rootTransform)
                {
                    Undo.RecordObject(mirrorTransformController, $"{_toolName} RootTransformの変更");
                    mirrorTransformController.rootTransform = _rootTransform;
                    mirrorTransformController.toolEnabled = false;
                }
            }
            // RottoTransformが空の場合
            if (mirrorTransformController.rootTransform == null)
            {
                mirrorTransformController.toolEnabled = false;
                EditorGUILayout.HelpBox("RootTransformを設定してください", MessageType.Warning);
            }
            #endregion

            // RootTransformが設定されていない場合に操作不能にする
            EditorGUI.BeginDisabledGroup(_isRootTransform == false);

            #region // 左右対称編集の有効・無効
            bool _toolEnabled = EditorGUILayout.Toggle("左右対称編集の有効・無効", mirrorTransformController.toolEnabled);
            if (_toolEnabled != mirrorTransformController.toolEnabled)
            {
                // Undo登録
                undoMessage = $"{_toolName} 左右対称編集の有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.toolEnabled, _toolEnabled, undoMessage);
            }
            if (mirrorTransformController.toolEnabled == false)
            {
                EditorGUILayout.HelpBox("左右対称編集が無効です", MessageType.Warning);
            }
            #endregion

            #region // 設定
            _settingIsOpen = EditorGUILayout.Foldout(mirrorTransformController.settingIsOpen, "設定");
            if (mirrorTransformController.settingIsOpen != _settingIsOpen) 
            {
                Undo.RecordObject(mirrorTransformController, $"{_toolName} 設定");
                mirrorTransformController.settingIsOpen = _settingIsOpen; 
            };
            if (_settingIsOpen == true)
            {
                EditorGUI.indentLevel++;
                bool _positionEnabled = EditorGUILayout.Toggle("Positionの有効・無効", mirrorTransformController.positionEnabled);
                bool _rotationEnabled = EditorGUILayout.Toggle("Rotationの有効・無効", mirrorTransformController.rotationEnabled);
                bool _scaleEnabled = EditorGUILayout.Toggle("Scaleの有効・無効", mirrorTransformController.scaleEnabled);
                bool _rotationQuaternionMode = mirrorTransformController.rotationQuaternionMode;
                bool _rotationEulerMode = mirrorTransformController.rotationEulerMode;
                //mirrorTransformController.decimalPlaces = EditorGUILayout.IntSlider("自動取得の判定に利用する小数点以下の桁数", mirrorTransformController.decimalPlaces, 0, 10);

                _advancedSettingIsOpen = EditorGUILayout.Toggle("詳細設定", mirrorTransformController.advancedSettingIsOpen);
                if (_advancedSettingIsOpen == true)
                {
                    EditorGUI.indentLevel++;
                    string[] rotationModeNames = { "Quaternion", "Euler" };
                    int _rotationMode = EditorGUILayout.Popup("Rotation", mirrorTransformController.rotationMode, rotationModeNames);
                    mirrorTransformController.rotationMode = _rotationMode;
                    
                    if (mirrorTransformController.rotationMode == 0)
                    {
                        _rotationQuaternionMode = true;
                        _rotationEulerMode = false;
                    }
                    else
                    {
                        _rotationQuaternionMode = false;
                        _rotationEulerMode = true;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;

                // Undo登録
                undoMessage = $"{_toolName} Positionの有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.positionEnabled, _positionEnabled, undoMessage);
                undoMessage = $"{_toolName} Rotationの有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.rotationEnabled, _rotationEnabled, undoMessage);
                undoMessage = $"{_toolName} Scaleの有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.scaleEnabled, _scaleEnabled, undoMessage);
                undoMessage = $"{_toolName} 詳細設定の有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.advancedSettingIsOpen, _advancedSettingIsOpen, undoMessage);
                undoMessage = $"{_toolName} QuaternionModeの有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.rotationQuaternionMode, _rotationQuaternionMode, undoMessage);
                undoMessage = $"{_toolName} EulerModeの有効・無効";
                RegisterUndoOnStateChange(ref mirrorTransformController.rotationEulerMode, _rotationEulerMode, undoMessage);
            }
            #endregion

            GUILayout.Space(_spacePixels);

            #region // TransformListの折りたたみ切り替え
            _transformListIsOpen = EditorGUILayout.Foldout(mirrorTransformController.transformListIsOpen, "TransformList");
            if (mirrorTransformController.transformListIsOpen != _transformListIsOpen)
            {
                Undo.RecordObject(mirrorTransformController, $"{_toolName} TransformList Open Close");
                mirrorTransformController.transformListIsOpen = _transformListIsOpen;
            }
            if (_transformListIsOpen == true)
            {
                mirrorTransformController.transformListNum = 
                    EditorGUILayout.IntField("Transform数", Mathf.Max(mirrorTransformController.transformListNum, 0));
                #region // Listの要素数の調整
                // 左右対称処理の処理方法の管理
                AdjustListSize<Transform>(mirrorTransformController.rightTransformList, mirrorTransformController.transformListNum, null);
                AdjustListSize<Transform>(mirrorTransformController.leftTransformList, mirrorTransformController.transformListNum, null);
                AdjustListSize<bool>(mirrorTransformController.mirrorEnabledList, mirrorTransformController.transformListNum, false);

                AdjustListSize<Vector3>(mirrorTransformController.transformPositionSignList, mirrorTransformController.transformListNum, Vector3.one);
                AdjustListSize<Vector3>(mirrorTransformController.transformScaleSignList, mirrorTransformController.transformListNum, Vector3.one);
                AdjustListSize<Vector4>(mirrorTransformController.transformRotationQuaternionSignList, mirrorTransformController.transformListNum, new Vector4(1, 1, 1, 1));
                AdjustListSize<Vector4>(mirrorTransformController.transformRotationHandleAxisList, mirrorTransformController.transformListNum, new Vector4(0, 1, 2, 3));
                AdjustListSize<Vector3>(mirrorTransformController.transformRotationEulerAngleSignList, mirrorTransformController.transformListNum, Vector3.one);
                AdjustListSize<bool>(mirrorTransformController.transformPositionEnabledList, mirrorTransformController.transformListNum, true);
                AdjustListSize<bool>(mirrorTransformController.transformRotationEulerEnabledList, mirrorTransformController.transformListNum, true);
                AdjustListSize<bool>(mirrorTransformController.transformRotationQuaternionEnabledList, mirrorTransformController.transformListNum, true);
                AdjustListSize<bool>(mirrorTransformController.transformScaleEnabledList, mirrorTransformController.transformListNum, true);
                #endregion

                if (mirrorTransformController.rightTransformList.Count == 0)
                {
                    EditorGUILayout.HelpBox($"編集対象となるTransformがありません{System.Environment.NewLine}" +
                        $"RootTransformの子のTransformを取得するには「{_getButtonText}」を押してください。{System.Environment.NewLine}" +
                        $"手動で設定する場合にはTransform数を変更し、Transformをセットしてください。", MessageType.Warning);
                }

                #region // Listの内容の表示
                if (_advancedSettingIsOpen) { _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition); }
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    undoMessage = $"{_toolName} AllEnabled";
                    DisplayEnabledListInInspector(ref mirrorTransformController.mirrorEnabledList,
                        ref mirrorTransformController.allMirrorEnabled, undoMessage);

                    if (_advancedSettingIsOpen == true)
                    {
                        DisplayVector3ListInInspector("Position", mirrorTransformController.transformPositionSignList, 
                            ref mirrorTransformController.transformPositionEnabledList,
                            ref mirrorTransformController.allPositionEnabled, $"{_toolName} Positionの設定の更新");
                        if (mirrorTransformController.rotationQuaternionMode == true)
                        {
                            DisplayVector4ListInInspector("Rotation(Quaternion)", mirrorTransformController.transformRotationQuaternionSignList,
                                mirrorTransformController.transformRotationHandleAxisList, 
                                ref mirrorTransformController.transformRotationQuaternionEnabledList,
                                ref mirrorTransformController.allRotationQuaternionEnabled, $"{_toolName} Rotation(Quaternion)の設定の更新");
                        }
                        if (mirrorTransformController.rotationEulerMode == true)
                        {
                            DisplayVector3ListInInspector("Rotation(Euler)", mirrorTransformController.transformRotationEulerAngleSignList, 
                                ref mirrorTransformController.transformRotationEulerEnabledList,
                                ref mirrorTransformController.allRotationEulerEnabled, $"{_toolName} Rotation(Euler)の設定の更新");
                        }
                        DisplayVector3ListInInspector("Scale", mirrorTransformController.transformScaleSignList, 
                            ref mirrorTransformController.transformScaleEnabledList,
                            ref mirrorTransformController.allScaleEnabled, $"{_toolName} Scaleの設定の更新");
                    }

                    // Transformの表示
                    DisplayTransformListInInspector("LeftTransform", ref mirrorTransformController.leftTransformList,
                        mirrorTransformController.rightTransformList, $"{_toolName} Transformの更新");
                    DisplayTransformListInInspector("RightTransform", ref mirrorTransformController.rightTransformList,
                        mirrorTransformController.leftTransformList, $"{_toolName} Transformの更新");
                }
                if (_advancedSettingIsOpen) { EditorGUILayout.EndScrollView(); }
                #endregion
            }
            #endregion


            GUILayout.Space(_spacePixels);

            if (GUILayout.Button(_getButtonText))
            {
                // TransformListが0でないとき
                if (mirrorTransformController.transformListNum > 0)
                {
                    string dialogMessage = $"RootTransformの子のTransformを取得します。{System.Environment.NewLine}" +
                        $"現在のTransformListをクリアしてから取得します。{System.Environment.NewLine}" +
                        $"よろしいですか？";
                    bool _dialogFlag = EditorUtility.DisplayDialog(_toolName, dialogMessage, "OK", "Cancel");
                    if (_dialogFlag == false) { return; }
                }

                // Rotationが正面を向いていないとき
                if (mirrorTransformController.rootTransform.eulerAngles != Vector3.zero)
                {
                    Quaternion tmpRotation = mirrorTransformController.rootTransform.rotation;
                    mirrorTransformController.rootTransform.eulerAngles = Vector3.zero;
                    mirrorTransformController.GetMirrorSymmetricalTransformList();
                    mirrorTransformController.rootTransform.rotation = tmpRotation;
                }
                else
                {
                    mirrorTransformController.GetMirrorSymmetricalTransformList();
                }
                mirrorTransformController.transformListNum = mirrorTransformController.leftTransformList.Count;
                if (mirrorTransformController.transformListNum == 0)
                {
                    Debug.LogError("取得できるTransformがありませんでした");
                }
                else
                {
                    Debug.Log("Transformの取得完了");
                }
            }

            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck())
            {
                if (mirrorTransformController.toolEnabled == true && mirrorTransformController.rootTransform != null)
                {
                    mirrorTransformController.MirrorTransform(mirrorTransformController.leftTransformList,
                        mirrorTransformController.rightTransformList, mirrorTransformController.mirrorEnabledList);
                }
                EditorUtility.SetDirty(mirrorTransformController);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region // ListのInspector上の表示
        // EnabledListの表示
        private void DisplayEnabledListInInspector(ref List<bool> enabledList, ref bool allMirrorEnabled, string message)
        {
            int _updateFlag = 0;
            using (new EditorGUILayout.VerticalScope())
            {
                if (_advancedSettingIsOpen == true) { EditorGUILayout.LabelField("", GUILayout.Width(_toggleWidth)); }
                for (int _objectNum_i = 0; _objectNum_i < enabledList.Count; _objectNum_i++)
                {
                    // 先頭のみすべてのON・OFFのToggleを付与
                    if (_objectNum_i == 0)
                    {
                        bool lastAllMirrorEnabled = EditorGUILayout.Toggle(allMirrorEnabled, GUILayout.Width(_toggleWidth));

                        // 変更があった場合
                        if (lastAllMirrorEnabled != allMirrorEnabled)
                        {
                            _updateFlag++;
                            if (_updateFlag == 1) 
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            allMirrorEnabled = lastAllMirrorEnabled;
                            enabledList = Enumerable.Repeat(allMirrorEnabled, enabledList.Count).ToList();
                        }
                    }
                    bool _enabled = EditorGUILayout.Toggle(enabledList[_objectNum_i], GUILayout.Width(_toggleWidth));

                    // 変更があった場合
                    if (_enabled != enabledList[_objectNum_i])
                    {
                        _updateFlag++;
                        if (_updateFlag == 1)
                        {
                            // Undo登録
                            RecordUndoEditorAndTransform(message);
                        }
                        enabledList[_objectNum_i] = _enabled;
                    }
                }
            }
        }

        // Transformの表示
        private void DisplayTransformListInInspector(string labelName, ref List<Transform> sourceTransformList,
            List<Transform> mirrorTransformList, string message)
        {
            Transform _sourceTransform;
            int _updateFlag = 0;
            using (new EditorGUILayout.VerticalScope())
            {
                if (_advancedSettingIsOpen == true) { EditorGUILayout.LabelField("Transform"); }

                for (int _objectNum_i = 0; _objectNum_i < sourceTransformList.Count; _objectNum_i++)
                {
                    // 先頭のみラベルを付与
                    if (_objectNum_i == 0) { EditorGUILayout.LabelField(labelName); }

                    _sourceTransform = EditorGUILayout.ObjectField(sourceTransformList[_objectNum_i], typeof(Transform), true) as Transform;

                    // Transformの更新がある場合
                    if (_sourceTransform != sourceTransformList[_objectNum_i])
                    {
                        _updateFlag++;
                        if (_updateFlag == 1)
                        {
                            // Undo登録
                            RecordUndoEditorAndTransform(message);
                        }
                        // TransformのFieldの更新
                        sourceTransformList[_objectNum_i] = _sourceTransform;

                        // L,Rともにnullじゃないときには符号チェック
                        if (sourceTransformList[_objectNum_i] != null && mirrorTransformList[_objectNum_i] != null)
                        {
                            // 符号の更新
                            mirrorTransformController.StoreSignToList(mirrorTransformController.leftTransformList,
                                mirrorTransformController.rightTransformList, _objectNum_i);
                            // 無効化
                            mirrorTransformController.mirrorEnabledList[_objectNum_i] = false;
                        }
                    }
                }
            }
        }

        private void DisplayVector3ListInInspector(string labelName, List<Vector3> signList, ref List<bool> enabledList,
            ref bool allMirrorEnabled, string message)
        {
            int _updateFlag = 0;
            float _width = 35;
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(labelName, GUILayout.Width(_width * 4));
                for (int _objectNum_i = 0; _objectNum_i < signList.Count; _objectNum_i++)
                {
                    // 先頭のみラベルを付与
                    if (_objectNum_i == 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            bool newAllEnabled = EditorGUILayout.Toggle(allMirrorEnabled, GUILayout.Width(_toggleWidth));

                            // 変更があった場合
                            if (newAllEnabled != allMirrorEnabled)
                            {
                                _updateFlag++;
                                if (_updateFlag == 1)
                                {
                                    // Undo登録
                                    RecordUndoEditorAndTransform(message);
                                }
                                allMirrorEnabled = newAllEnabled;
                                enabledList = Enumerable.Repeat(allMirrorEnabled, enabledList.Count).ToList();
                            }
                            EditorGUILayout.LabelField("X", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("Y", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("Z", GUILayout.Width(_width));
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool _enabled = EditorGUILayout.Toggle(enabledList[_objectNum_i], GUILayout.Width(_toggleWidth));
                        Vector3 tmpSignVector3 = Vector3.zero;
                        for (int _axis_i = 0; _axis_i < 3; _axis_i++)
                        {
                            tmpSignVector3[_axis_i] = EditorGUILayout.Popup((int)signList[_objectNum_i][_axis_i], _rotationSign, GUILayout.Width(_width));
                        }

                        // 変更があった場合
                        if (_enabled != enabledList[_objectNum_i])
                        {
                            _updateFlag++;
                            if (_updateFlag == 1)
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            enabledList[_objectNum_i] = _enabled;
                        }
                        // 変更があった場合
                        if (tmpSignVector3 != signList[_objectNum_i])
                        {
                            _updateFlag++;
                            if (_updateFlag == 1)
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            signList[_objectNum_i] = tmpSignVector3;
                        }
                    }
                }
            }
        }

        private void DisplayVector4ListInInspector(string labelName, List<Vector4> signList, List<Vector4> axisList, ref List<bool> enabledList,
            ref bool allMirrorEnabled, string message)
        {
            int _updateFlag = 0;
            float _width = 35;
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(labelName, GUILayout.Width(_width * 8));
                for (int _objectNum_i = 0; _objectNum_i < signList.Count; _objectNum_i++)
                {
                    // 先頭のみラベルを付与
                    if (_objectNum_i == 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            bool newAllEnabled = EditorGUILayout.Toggle(allMirrorEnabled, GUILayout.Width(_toggleWidth));

                            // 変更があった場合
                            if (newAllEnabled != allMirrorEnabled)
                            {
                                _updateFlag++;
                                if (_updateFlag == 1)
                                {
                                    // Undo登録
                                    RecordUndoEditorAndTransform(message);
                                }
                                allMirrorEnabled = newAllEnabled;
                                enabledList = Enumerable.Repeat(allMirrorEnabled, enabledList.Count).ToList();
                            }
                            EditorGUILayout.LabelField("+-", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("X", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("+-", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("Y", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("+-", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("Z", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("+-", GUILayout.Width(_width));
                            EditorGUILayout.LabelField("W", GUILayout.Width(_width));
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool _enabled = EditorGUILayout.Toggle(enabledList[_objectNum_i], GUILayout.Width(_toggleWidth));
                        
                        Vector4 tmpSignVector4 = Vector4.zero;
                        Vector4 tmpAxisVector4 = Vector4.zero;
                        for (int _axis_i = 0; _axis_i < 4; _axis_i++)
                        {
                            tmpSignVector4[_axis_i] = EditorGUILayout.Popup((int)signList[_objectNum_i][_axis_i], _rotationSign, GUILayout.Width(_width));
                            tmpAxisVector4[_axis_i] = EditorGUILayout.Popup((int)axisList[_objectNum_i][_axis_i], _cartesianAxis, GUILayout.Width(_width));
                        }

                        // 変更があった場合
                        if (_enabled != enabledList[_objectNum_i])
                        {
                            _updateFlag++;
                            if (_updateFlag == 1)
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            enabledList[_objectNum_i] = _enabled;
                        }
                        // 変更があった場合
                        if (tmpSignVector4 != signList[_objectNum_i])
                        {
                            _updateFlag++;
                            if (_updateFlag == 1)
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            signList[_objectNum_i] = tmpSignVector4;
                        }
                        // 変更があった場合
                        if (tmpAxisVector4 != axisList[_objectNum_i])
                        {
                            _updateFlag++;
                            if (_updateFlag == 1)
                            {
                                // Undo登録
                                RecordUndoEditorAndTransform(message);
                            }
                            axisList[_objectNum_i] = tmpAxisVector4;
                        }
                    }
                }
            }
        }
        #endregion

        #region // Undo関係
        private void RecordUndoEditorAndTransform(string message)
        {
            Undo.RecordObject(mirrorTransformController, message);
            RecordUndoTransform(mirrorTransformController.leftTransformList, message);
            RecordUndoTransform(mirrorTransformController.rightTransformList, message);
        }
        private void RegisterUndoOnStateChange(ref bool enabled, bool newEnabledd, string message)
        {
            // 前回と状態が変わった場合にUndo登録
            if (newEnabledd != enabled)
            {
                RecordUndoEditorAndTransform(message);
                enabled = newEnabledd;
            }
            return;
        }

        private void RecordUndoTransform(List<Transform> transformList, string message)
        {
            foreach (Transform transform in transformList)
            {
                if (transform != null)
                {
                    Undo.RecordObject(transform, message);
                }
            }
        }
        #endregion

        #region // Listの要素数の調整
        private void AdjustListSize<T>(List<T> targetList, int listCount, T defaultValue)
        {
            int _listCount = Mathf.Max(0, listCount);
            while (_listCount > targetList.Count)
            {
                targetList.Add(defaultValue);
            }
            while (_listCount < targetList.Count)
            {
                targetList.RemoveAt(targetList.Count - 1);
            }
        }
        #endregion
    }
}
#endif