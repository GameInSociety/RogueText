using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/*
[CustomPropertyDrawer(typeof(Item))]
public class ItemUI : PropertyDrawer {
    private Item value;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var targetObject = property.serializedObject.targetObject;
        value = property.GetValue<Item>();

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (value != null && value.props != null) {
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            var propRect = new Rect(position.x, position.y, position.width, position.height);
            EditorGUI.PropertyField(propRect, property.FindPropertyRelative("props"), GUIContent.none);
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var targetObject = property.serializedObject.targetObject;
        value = property.GetValue<Item>();
        if ( value == null) {
            return 30f;
        }
        if ( value.props == null) {
            return 30f;
        }

        var height = 20f;
        for (int i = 0; i < value.props.Count; i++) {
            height += (value.props[i].parts.Count + 1) * 20 + 50;
        }
        return height;
    }
}

[CustomPropertyDrawer(typeof(Property.Part))]
public class PropertyPartUI : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var nameRect = new Rect(position.x, position.y, position.width, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("content"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(Property))]
public class PropertyUI : PropertyDrawer {
    private Property value;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        var targetObject = property.serializedObject.targetObject;
        value = property.GetValue<Property>();

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var enabledRect = new Rect(position.x, position.y, position.width, position.height);
        var partsRect = new Rect(position.x + 30, position.y, position.width - 30, position.height+30);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(enabledRect, property.FindPropertyRelative("enabled"), GUIContent.none);
        EditorGUI.PropertyField(partsRect, property.FindPropertyRelative("parts"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var targetObject = property.serializedObject.targetObject;
        value = property.GetValue<Property>();
        return (value.parts.Count + 1) * 20 + 30;
    }
}
public static class SerializedUtility {

    public static T GetValue<T>(this SerializedProperty property) where T : class {
        object obj = property.serializedObject.targetObject;
        string path = property.propertyPath.Replace(".Array.data", "");
        string[] fieldStructure = path.Split('.');
        Regex rgx = new Regex(@"\[\d+\]");
        for (int i = 0; i < fieldStructure.Length; i++) {
            if (fieldStructure[i].Contains("[")) {
                int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
            } else {
                obj = GetFieldValue(fieldStructure[i], obj);
            }
        }
        return (T)obj;
    }

    public static bool SetValue<T>(this SerializedProperty property, T value) where T : class {
        object obj = property.serializedObject.targetObject;
        string path = property.propertyPath.Replace(".Array.data", "");
        string[] fieldStructure = path.Split('.');
        Regex rgx = new Regex(@"\[\d+\]");
        for (int i = 0; i < fieldStructure.Length - 1; i++) {
            if (fieldStructure[i].Contains("[")) {
                int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
            } else {
                obj = GetFieldValue(fieldStructure[i], obj);
            }
        }

        string fieldName = fieldStructure.Last();
        if (fieldName.Contains("[")) {
            int index = System.Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
            return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
        } else {
            Debug.Log(value);
            return SetFieldValue(fieldName, obj, value);
        }
    }

    private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        FieldInfo field = obj.GetType().GetField(fieldName, bindings);
        if (field != null) {
            return field.GetValue(obj);
        }
        return default(object);
    }

    private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        FieldInfo field = obj.GetType().GetField(fieldName, bindings);
        if (field != null) {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray) {
                return ((object[])list)[index];
            } else if (list is IEnumerable) {
                return ((IList)list)[index];
            }
        }
        return default(object);
    }

    public static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        FieldInfo field = obj.GetType().GetField(fieldName, bindings);
        if (field != null) {
            field.SetValue(obj, value);
            return true;
        }
        return false;
    }

    public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        FieldInfo field = obj.GetType().GetField(fieldName, bindings);
        if (field != null) {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray) {
                ((object[])list)[index] = value;
                return true;
            } else if (value is IEnumerable) {
                ((IList)list)[index] = value;
                return true;
            }
        }
        return false;
    }
}*/