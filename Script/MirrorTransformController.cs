#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PogapogaEditor.Component
{
    [ExecuteInEditMode]
    public class MirrorTransformController : MonoBehaviour
    {        
        #region // 変数宣言
        // 設定
        public bool toolEnabled = false; // 左右対称編集の有効・無効
        public bool transformRotationEnabled = false; // Rotationも判定対象とするか
        public bool transformScaleEnabled = false; // Scaleも判定対象とするか
        public int decimalPlaces = 4; // 判定に使う少数桁数の指定
        public Transform rootTransform; // X軸の基準となるTransform
        public bool positionEnabled = true;
        public bool rotationEnabled = true;
        public bool scaleEnabled = true;
        public bool rotationQuaternionMode = true;
        public bool rotationEulerMode = false;
        public bool allPositionEnabled = true;
        public bool allRotationQuaternionEnabled = true;
        public bool allRotationEulerEnabled = false;
        public bool allScaleEnabled = true;
        public int rotationMode = 0;

        // TransformのList
        public int transformListNum = 0;
        public bool allMirrorEnabled = false;
        public List<Transform> rightTransformList = new List<Transform>();
        public List<Transform> leftTransformList = new List<Transform>();
        public List<bool> mirrorEnabledList = new List<bool>();

        // 左右対称処理の処理方法の管理
        public List<Vector3> transformPositionSignList = new List<Vector3>();
        public List<Vector3> transformScaleSignList = new List<Vector3>();
        public List<Vector4> transformRotationQuaternionSignList = new List<Vector4>();
        public List<Vector4> transformRotationHandleAxisList = new List<Vector4>();
        public List<Vector3> transformRotationEulerAngleSignList = new List<Vector3>();
        public List<bool> transformPositionEnabledList = new List<bool>();
        public List<bool> transformRotationEulerEnabledList = new List<bool>();
        public List<bool> transformRotationQuaternionEnabledList = new List<bool>();
        public List<bool> transformScaleEnabledList = new List<bool>();
        
        // Editor用
        public bool settingIsOpen = false;
        public bool advancedSettingIsOpen = false;
        public bool transformListIsOpen = true;
        #endregion

        #region // 自動取得関係
        /// <summary>
        /// RootTransformから左右で対応するTransformを取得する
        /// </summary>
        public void GetMirrorSymmetricalTransformList()
        {
            Transform[] _childTransforms;
            Dictionary<string, Transform> _autoRightTransformDict = new Dictionary<string, Transform>();
            Dictionary<string, Transform> _autoLeftTransformDict = new Dictionary<string, Transform>();

            ClearListAndFlagReset();

            if (rootTransform == null) { rootTransform = this.transform; }
            _childTransforms = rootTransform.GetComponentsInChildren<Transform>(true);

            // 子のObjectが無い場合
            if (_childTransforms.Length == 0)
            {
                return;
            }

            #region // Transformの辞書への格納
            for (int _childNum_i = 0; _childNum_i < _childTransforms.Length; _childNum_i++)
            {
                
                // 自身のTransformのときはスキップ
                if (_childTransforms[_childNum_i] == rootTransform) { continue; }

                // World基準のPositionを取得
                string _keyX = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].position.x - rootTransform.position.x)).ToString();
                string _keyY = TruncateFloat((_childTransforms[_childNum_i].position.y - rootTransform.position.y)).ToString();
                string _keyZ = TruncateFloat((_childTransforms[_childNum_i].position.z - rootTransform.position.z)).ToString();

                // Hierarchyの階層数を取得
                string _keyName = $"{CountHierarchy(_childTransforms[_childNum_i])}:";
                _keyName += $"{_keyX}{_keyY}{_keyZ}";

                if (transformRotationEnabled == true)
                {
                    string _keyRX = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localRotation.x)).ToString();
                    string _keyRY = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localRotation.y)).ToString();
                    string _keyRZ = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localRotation.z)).ToString();
                    string _keyRW = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localRotation.w)).ToString();
                    _keyName += $"{_keyRX}{_keyRY}{_keyRZ}{_keyRW}";
                }
                if (transformScaleEnabled == true)
                {
                    string _keySX = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localScale.x)).ToString();
                    string _keySY = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localScale.y)).ToString();
                    string _keySZ = TruncateFloat(Mathf.Abs(_childTransforms[_childNum_i].localScale.z)).ToString();
                    _keyName += $"{_keySX}{_keySY}{_keySZ}";
                }

                // 中央のTransform
                if (_childTransforms[_childNum_i].position.x - rootTransform.position.x == 0) { continue; }
                // 右側のTransform
                else if (_childTransforms[_childNum_i].position.x - rootTransform.position.x > 0) { _autoRightTransformDict[_keyName] = _childTransforms[_childNum_i]; }
                // 左側のTransform
                else { _autoLeftTransformDict[_keyName] = _childTransforms[_childNum_i]; }
            }
            #endregion

            #region // TransformのListへの格納
            string[] _autoKeys = _autoRightTransformDict.Keys.ToArray().Intersect( _autoLeftTransformDict.Keys).ToArray();
            foreach(string _key in _autoKeys)
            {
                rightTransformList.Add(_autoRightTransformDict[_key]);
                leftTransformList.Add(_autoLeftTransformDict[_key]);
            }
            #endregion

            #region // Listの要素数の確保
            mirrorEnabledList = Enumerable.Repeat(false, leftTransformList.Count).ToList();

            // 左右対称処理の処理方法の管理
            transformPositionSignList = Enumerable.Repeat(new Vector3(1, 1, 1), leftTransformList.Count).ToList();
            transformScaleSignList = Enumerable.Repeat(new Vector3(1, 1, 1), leftTransformList.Count).ToList();
            transformRotationQuaternionSignList = Enumerable.Repeat(new Vector4(1, 1, 1, 1), leftTransformList.Count).ToList();
            transformRotationHandleAxisList = Enumerable.Repeat(new Vector4(1, 1, 1, 1), leftTransformList.Count).ToList();
            transformRotationEulerAngleSignList = Enumerable.Repeat(new Vector3(1, 1, 1), leftTransformList.Count).ToList();
            transformPositionEnabledList = Enumerable.Repeat(true, leftTransformList.Count).ToList();
            transformRotationEulerEnabledList = Enumerable.Repeat(false, leftTransformList.Count).ToList();
            transformRotationQuaternionEnabledList = Enumerable.Repeat(false, leftTransformList.Count).ToList();
            transformScaleEnabledList = Enumerable.Repeat(true, leftTransformList.Count).ToList();
            #endregion

            for (int _objectNum_i = 0; _objectNum_i < leftTransformList.Count; _objectNum_i++)
            {
                StoreSignToList(leftTransformList, rightTransformList, _objectNum_i);
            }
        }

        /// <summary>
        /// 自動取得したListのClear処理
        /// </summary>
        public void ClearListAndFlagReset()
        {
            allMirrorEnabled = false;
            allPositionEnabled = true;
            allRotationQuaternionEnabled = true;
            allRotationEulerEnabled = false;
            allScaleEnabled = true;

            // TransformのList
            rightTransformList.Clear();
            leftTransformList.Clear();
            mirrorEnabledList.Clear();
            // 左右対称処理の処理方法の管理
            transformPositionSignList.Clear();
            transformScaleSignList.Clear();
            transformRotationQuaternionSignList.Clear();
            transformRotationHandleAxisList.Clear();
            transformRotationEulerAngleSignList.Clear();
            transformPositionEnabledList.Clear();
            transformRotationEulerEnabledList.Clear();
            transformRotationQuaternionEnabledList.Clear();
            transformScaleEnabledList.Clear();
        }

        public void StoreSignToList(List<Transform> leftTransformList, List<Transform> rightTransformList, int targetNum)            
        {
            #region // PositionSign
            Vector3 _leftLocalPosition = leftTransformList[targetNum].localPosition;
            Vector3 _rightLocalPosition = rightTransformList[targetNum].localPosition;
            Vector3 _positionSign = Vector3.one;
            for (int _axis_i = 0; _axis_i < 3; _axis_i++)
            {
                _positionSign[_axis_i] = IsSameSign(_leftLocalPosition[_axis_i], _rightLocalPosition[_axis_i]) ? 1 : 0;
            }
            transformPositionSignList[targetNum] = _positionSign;
            #endregion

            #region // RotationQuaternion
            Quaternion _leftLocalRotation = leftTransformList[targetNum].localRotation;
            Quaternion _rightLocalRotation = rightTransformList[targetNum].localRotation;
            // RotationAxis
            float[] _leftRotationAbs = new float[4]{
                Mathf.Abs(TruncateFloat(_leftLocalRotation.x)),
                Mathf.Abs(TruncateFloat(_leftLocalRotation.y)),
                Mathf.Abs(TruncateFloat(_leftLocalRotation.z)),
                Mathf.Abs(TruncateFloat(_leftLocalRotation.w))};
            float[] _rightRotationAbs = new float[4]{
                Mathf.Abs(TruncateFloat(_rightLocalRotation.x)),
                Mathf.Abs(TruncateFloat(_rightLocalRotation.y)),
                Mathf.Abs(TruncateFloat(_rightLocalRotation.z)),
                Mathf.Abs(TruncateFloat(_rightLocalRotation.w))};

            // RotationQuaternionの数値の対応関係処理
            // Quaternionの絶対値の等しいものとの対応関係を確認
            float[] _tmpAxis = new float[4];
            for (int _axis_i = 0; _axis_i < _leftRotationAbs.Length; _axis_i++)
            {
                // 同じAxisでの一致
                if (Mathf.Approximately(_leftRotationAbs[_axis_i], _rightRotationAbs[_axis_i]))
                {
                    _tmpAxis[_axis_i] = _axis_i;
                    continue;
                }
                else
                {
                    for (int _axis_j = 0; _axis_j < _leftRotationAbs.Length; _axis_j++)
                    {
                        if (_axis_i == _axis_j) { continue; }

                        if (Mathf.Approximately(_leftRotationAbs[_axis_i], _rightRotationAbs[_axis_j]))
                        {
                            _tmpAxis[_axis_i] = _axis_j;
                            break;
                        }
                        if (_axis_j == _leftRotationAbs.Length - 1)
                        {
                            _tmpAxis[_axis_i] = _axis_i;
                        }
                    }
                }
            }
            transformRotationHandleAxisList[targetNum] = new Vector4(_tmpAxis[0], _tmpAxis[1], _tmpAxis[2], _tmpAxis[3]);
            // 対応関係の重複を確認
            int[] _duplicateArray = new int[4];
            for (int _axis_i = 0; _axis_i < 4; _axis_i++)
            {
                _duplicateArray[_axis_i] = (int)transformRotationHandleAxisList[targetNum][_axis_i];
            }
            // 重複がない場合にはEnabledListにtrue
            transformRotationQuaternionEnabledList[targetNum] = _duplicateArray.Distinct().Count() == 4;
            // RotationQuaternionの符号の対応関係処理
            Vector4 _vector4Sign = Vector4.zero;
            for (int _axis_i = 0; _axis_i < 4; _axis_i++)
            {
                _vector4Sign[_axis_i] = IsSameSign(_leftLocalRotation[_axis_i], _rightLocalRotation[(int)transformRotationHandleAxisList[targetNum][_axis_i]]) ? 1 : 0;
            }
            transformRotationQuaternionSignList[targetNum] = _vector4Sign;
            #endregion

            #region // RotationEuler
            // Eulerの対応関係処理
            Vector3 _leftEulerAngles = leftTransformList[targetNum].localEulerAngles;
            Vector3 _rightEulerAngles = rightTransformList[targetNum].localEulerAngles;
            Vector3 _eulerSign = Vector3.zero;
            for (int _axis_i = 0; _axis_i < 3; _axis_i++)
            {
                _eulerSign[_axis_i] = IsSameValue(_leftEulerAngles[_axis_i], _rightEulerAngles[_axis_i]) ? 1 : 0;
            }

            transformRotationEulerAngleSignList[targetNum] = _eulerSign;
            #endregion

            #region // ScaleSign
            Vector3 _leftLocalScale = leftTransformList[targetNum].localScale;
            Vector3 _rightLocalScale = rightTransformList[targetNum].localScale;
            Vector3 _scaleSign = Vector3.zero;
            float _scaleEnabled = 0;
            for (int _axis_i = 0; _axis_i < 3; _axis_i++)
            {
                _scaleSign[_axis_i] = IsSameSign(_leftLocalScale[_axis_i], _rightLocalScale[_axis_i]) ? 1 : 0;
                _scaleEnabled += IsSameValue(_leftLocalScale[_axis_i], _rightLocalScale[_axis_i]) ? 1 : 0;
            }
            transformScaleSignList[targetNum] = _scaleSign;
            transformScaleEnabledList[targetNum] = _scaleEnabled == 3 ? true : false ;
            #endregion
        }

        private float TruncateFloat(float targetValue)
        {
            return (float)Math.Truncate(targetValue * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        }

        private float CeilingFloat(float targetValue)
        {
            return (float)Math.Ceiling(targetValue * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        }
        private float RoundFloat(float targetValue)
        {
            return (float)Math.Round(targetValue * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);            
        }

        public bool IsSameValue(float leftValue, float rightValue)
        {
            if (Mathf.Approximately(TruncateFloat(leftValue), TruncateFloat(rightValue)))
            {
                return true;
            }
            if ((Mathf.Approximately(CeilingFloat(leftValue), CeilingFloat(rightValue))))
            {
                return true;
            }
            if ((Mathf.Approximately(RoundFloat(leftValue), RoundFloat(rightValue))))
            {
                return true;
            }
            if (Mathf.Approximately(leftValue, rightValue))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 左右の値の符号が等しいかの確認
        /// </summary>
        /// <param name="leftValue"></param>
        /// <param name="rightValue"></param>
        /// <returns></returns>
        public bool IsSameSign(float leftValue, float rightValue)
        {
            // 符号が同じ
            if (Mathf.Sign(leftValue) == Mathf.Sign(rightValue))
            {
                return true;
            }
            // ほぼ等しい
            if (Mathf.Approximately(leftValue, rightValue))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// targetの階層をカウントする
        /// </summary>
        /// <returns></returns>
        public int CountHierarchy(Transform targetTransform)
        {
            int resultNum = 0;
            Transform transform = targetTransform;
            while (transform.parent != null)
            {
                transform = transform.parent;
                resultNum++;
            }
            return resultNum;
        }
        #endregion


        void Update()
        {
            // 処理をスキップ
            if (toolEnabled == false) { return; }
            if (rootTransform == null) { return; }
            if (Selection.objects.Length == 0) { return; }
            if (Selection.activeTransform == null) { return; }
            
            // 左右対称編集
            if(leftTransformList.Contains(Selection.activeTransform) == true)
            {
                MirrorTransform(leftTransformList, rightTransformList, mirrorEnabledList);
            }
            if (rightTransformList.Contains(Selection.activeTransform) == true)
            {
                MirrorTransform(rightTransformList, leftTransformList, mirrorEnabledList);
            }
        }


        #region // MirrorTransform
        /// <summary>
        /// sourceTransformを元にmirrorTransformを編集する
        /// </summary>
        private void MirrorTransform(Transform sourceTransform, Transform mirrorTransform, bool mirrorEnabled,
            Vector3 positionSign, Vector4 rotationQuaternionSign, Vector4 rotationAxis, Vector3 scaleSign, Vector3 rotationEulerSign,
            bool singlePositionEnabled, bool singleRotationQuaternionEnabled, bool singleScaleEnabled, bool singleRotationEulerEnabled)
        {
            #region // スキップ処理
            // 対象となるTransformがないときはスキップ
            if (mirrorTransform == null) { return; }

            // FlagがFalseのときはスキップ
            if (mirrorEnabled == false) { return; }

            // sourceとmirrorが同じ場合はスキップ
            if (sourceTransform == mirrorTransform) { return; }
            #endregion

            #region // 反対側のPosition
            if (positionEnabled == true && singlePositionEnabled == true)
            {
                Vector3 _tmpPosition = sourceTransform.localPosition;
                if (positionSign.x == 0) { _tmpPosition.x *= -1; }
                if (positionSign.y == 0) { _tmpPosition.y *= -1; }
                if (positionSign.z == 0) { _tmpPosition.z *= -1; }

                if (mirrorTransform.localPosition != _tmpPosition)
                {
                    mirrorTransform.localPosition = _tmpPosition;
                }
            }
            #endregion

            #region // 反対側のRotation
            if (rotationQuaternionMode == true)
            {
                if (rotationEnabled == true && singleRotationQuaternionEnabled == true)
                {
                    Quaternion _tmpRotation = sourceTransform.localRotation;
                    _tmpRotation.x = sourceTransform.localRotation[(int)rotationAxis[0]];
                    _tmpRotation.y = sourceTransform.localRotation[(int)rotationAxis[1]];
                    _tmpRotation.z = sourceTransform.localRotation[(int)rotationAxis[2]];
                    _tmpRotation.w = sourceTransform.localRotation[(int)rotationAxis[3]];

                    if (rotationQuaternionSign.x == 0) { _tmpRotation.x *= -1; }
                    if (rotationQuaternionSign.y == 0) { _tmpRotation.y *= -1; }
                    if (rotationQuaternionSign.z == 0) { _tmpRotation.z *= -1; }
                    if (rotationQuaternionSign.w == 0) { _tmpRotation.w *= -1; }

                    if (mirrorTransform.localRotation != _tmpRotation)
                    {
                        mirrorTransform.localRotation = _tmpRotation;
                    }
                }
            }

            else if (rotationEulerMode == true)
            {
                if (rotationEnabled == true && singleRotationEulerEnabled == true)
                {
                    Vector3 _tmpEuler = sourceTransform.localEulerAngles;
                    if (rotationEulerSign.x == 0) { _tmpEuler.x = 360 - _tmpEuler.x; }
                    if (rotationEulerSign.y == 0) { _tmpEuler.y = 360 - _tmpEuler.y; }
                    if (rotationEulerSign.z == 0) { _tmpEuler.z = 360 - _tmpEuler.z; }
                    Quaternion _tmpQuaternion = Quaternion.Euler(_tmpEuler);

                    if (mirrorTransform.localRotation != _tmpQuaternion)
                    {
                        mirrorTransform.localRotation = _tmpQuaternion;
                    }
                }
            }
            #endregion

            #region // 反対側のScale
            if (scaleEnabled == true && singleScaleEnabled == true)
            {
                Vector3 _tmpScale = sourceTransform.localScale;
                if (scaleSign.x == 0) { _tmpScale.x *= -1; }
                if (scaleSign.y == 0) { _tmpScale.y *= -1; }
                if (scaleSign.z == 0) { _tmpScale.z *= -1; }

                if (mirrorTransform.localScale != _tmpScale)
                {
                    mirrorTransform.localScale = _tmpScale;
                }
            }
            #endregion
            return;
        }

        /// <summary>
        /// List版
        /// </summary>
        public void MirrorTransform(List<Transform> sourceList, List<Transform> mirrorList, List<bool> enabledList)
        {
            for (int _objectNum_i = 0; _objectNum_i < sourceList.Count; _objectNum_i++)
            {
                // 存在しない場合のスキップ
                if (sourceList[_objectNum_i] == null || mirrorList[_objectNum_i] == null) { continue; }
                MirrorTransform(sourceList[_objectNum_i], mirrorList[_objectNum_i], enabledList[_objectNum_i],
                    transformPositionSignList[_objectNum_i], transformRotationQuaternionSignList[_objectNum_i], transformRotationHandleAxisList[_objectNum_i], 
                    transformScaleSignList[_objectNum_i], transformRotationEulerAngleSignList[_objectNum_i],
                    transformPositionEnabledList[_objectNum_i], transformRotationQuaternionEnabledList[_objectNum_i], 
                    transformScaleEnabledList[_objectNum_i], transformRotationEulerEnabledList[_objectNum_i]);
            }
            return;
        }
        #endregion
    }
}
#endif