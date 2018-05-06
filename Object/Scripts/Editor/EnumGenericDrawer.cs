﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KRG {

    //[CustomPropertyDrawer(typeof(EnumGeneric))]
    public abstract class EnumGenericDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty stringTypeProp = property.FindPropertyRelative("_stringType");
            SerializedProperty intValueProp = property.FindPropertyRelative("_intValue");

            Rect rect = new Rect(position.x, position.y, position.width, position.height);

            label.text = SwapLabelText(label.text);

            string stringType = stringTypeProp.stringValue;
            if (SwapEnum(ref stringType)) {
                stringTypeProp.stringValue = stringType;
            }

            System.Enum selected = EnumGeneric.ToEnum(stringType, intValueProp.intValue);
            selected = EditorGUI.EnumPopup(rect, label, selected);
            intValueProp.intValue = System.Convert.ToInt32(selected);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Swaps the enum type used to draw this EnumGeneric instance. Will retain the underlying int value.
        /// Example override functionality may be as follows:
        /// stringType = stringType.Replace("KRG.", "MyGame."); return true;
        /// </summary>
        /// <returns><c>true</c>, if enum was swapped, <c>false</c> otherwise.</returns>
        /// <param name="stringType">A string representation of the enum type.</param>
        protected abstract bool SwapEnum(ref string stringType);

        protected virtual string SwapLabelText(string text) {
            switch (text) {
                case "Time Thread Index":
                    return "Time Thread";
                default:
                    return text;
            }
        }
    }
}
