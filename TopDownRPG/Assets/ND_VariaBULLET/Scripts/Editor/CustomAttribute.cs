#region Script Synopsis
    //Creates a custom range attribute that limits Range values to a given decimal place. EG: [RangeStepped(0.1f, 2, 1)] allows a range of 0.1 to 2, at stepped increments of 1 decimal place
#endregion

using System;
using UnityEngine;
using UnityEditor;

namespace ND_VariaBULLET.EditorGUI
{
    [CustomPropertyDrawer(typeof(RangeSteppedAttribute))]
    internal sealed class RangeSteppedDrawer : PropertyDrawer
    {
        private float val;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = (RangeSteppedAttribute)base.attribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                val = UnityEditor.EditorGUI.Slider(position, label, property.floatValue, range.min, range.max);
                val = (float)Math.Round(val, range.step);
                property.floatValue = val;
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                val = UnityEditor.EditorGUI.IntSlider(position, label, property.intValue, (int)range.min, (int)range.max);

                if (val > range.min)
                    val = ((int)val / range.step) * range.step;

                property.intValue = (int)val;
            }
            else
                UnityEditor.EditorGUI.LabelField(position, label.text, "Use RangeStepped Attribute for floats or ints");
        }
    }
}