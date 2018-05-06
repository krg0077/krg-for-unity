﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KRG {

    [CanEditMultipleObjects, CustomEditor(typeof(Transform))]
    public class TransformEditor : TransformInspector {

        const string _resetAllControl = "ResetAll";
        const string _resetPositionControl = "ResetPosition";
        const string _resetRotationControl = "ResetRotation";
        const string _resetScaleControl = "ResetScale";

        //style
        GUIStyle _resetAllStyle;
        GUIStyle _resetSingleStyle;

        public override void OnInspectorGUI() {
            EditorGUIUtility.wideMode = TransformInspector.WIDE_MODE;

            // align field to right of inspector
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 3.5f;

            _resetAllStyle = new GUIStyle(EditorStyles.miniButton);
            _resetAllStyle.fixedHeight = 20;
            _resetAllStyle.fixedWidth = EditorGUIUtility.labelWidth + 28;

            _resetSingleStyle = new GUIStyle(EditorStyles.miniButton);
            _resetSingleStyle.fixedWidth = 30;

            serializedObject.Update();

            GUI.SetNextControlName(_resetAllControl);
            if (GUILayout.Button("Reset All", _resetAllStyle)) ResetAll();

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName(_resetPositionControl);
            if (GUILayout.Button("0", _resetSingleStyle)) ResetPosition();
            EditorGUILayout.PropertyField(positionProperty, positionGUIContent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName(_resetRotationControl);
            if (GUILayout.Button("0", _resetSingleStyle)) ResetRotation();
            RotationPropertyField(rotationProperty, rotationGUIContent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName(_resetScaleControl);
            if (GUILayout.Button("1", _resetSingleStyle)) ResetScale();
            EditorGUILayout.PropertyField(scaleProperty, scaleGUIContent);
            EditorGUILayout.EndHorizontal();

            if (!ValidatePosition(((Transform)target).position)) {
                EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void ResetAll() {
            GUI.FocusControl(_resetAllControl);
            //perform the equivalent of Reset(), but without referencing each Transform
            positionProperty.vector3Value = Vector3.zero;
            rotationProperty.quaternionValue = Quaternion.identity;
            scaleProperty.vector3Value = Vector3.one;
        }

        protected void ResetPosition() {
            GUI.FocusControl(_resetPositionControl);
            positionProperty.vector3Value = Vector3.zero;
        }

        protected void ResetRotation() {
            GUI.FocusControl(_resetRotationControl);
            rotationProperty.quaternionValue = Quaternion.identity;
        }

        protected void ResetScale() {
            GUI.FocusControl(_resetScaleControl);
            scaleProperty.vector3Value = Vector3.one;
        }
    }
}
