#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace FTG
{
    public class HideToggleDrawer : MaterialPropertyDrawer
    {
        private readonly string toggleName;
        private float toggle;
        private readonly ColorPickerHDRConfig hDRConfig;
        public HideToggleDrawer(string _toggleName)
        {
            toggleName = _toggleName;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            toggle = editor.GetFloat(toggleName, out bool _);
            if (toggle == 1)
            {

                switch (prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        Color valueColor;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle control
                        valueColor = editor.ColorProperty(position, prop, label);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.colorValue = valueColor;
                        }
                        break;
                    case MaterialProperty.PropType.Float:
                        float valueFloat;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle conterol
                        valueFloat = editor.FloatProperty(position, prop, label);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.floatValue = valueFloat;
                        }
                        break;
                    case MaterialProperty.PropType.Vector:
                        Vector4 vectorValue;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle control
                        vectorValue = editor.VectorProperty(position, prop, label);
                        // EditorGUI.Vector4Field(position, label, vectorValue);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.vectorValue = vectorValue;
                        }
                        break;
                    case MaterialProperty.PropType.Range:
                        float floatValue;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle control
                        floatValue = editor.RangeProperty(position, prop, label);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.floatValue = floatValue;
                        }
                        break;

                    case MaterialProperty.PropType.Texture:
                        Texture textureValue;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle control
                        textureValue = editor.TextureProperty(prop, label);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.textureValue = textureValue;
                        }
                        break;
#if UNITY_2021_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        int intValue = prop.intValue;
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = prop.hasMixedValue;

                        // Show the toggle control
                        intValue = EditorGUI.IntField(position, label, intValue);

                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set the new value if it has changed
                            prop.intValue = intValue;
                        }
                        break;
#endif
                }
            }

        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            //Debug.Log ("Getting height");
            if (toggle == 1)
                return base.GetPropertyHeight(prop, label, editor);
            else return 0;

        }
    }
}
#endif