﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
 
namespace Utils.Editor {
    public abstract class DictionaryDrawer<T, TK, TV> : PropertyDrawer where T : SerializableDictionary<TK, TV>, new() {
        private T _dictionary;
        private bool _foldout;
        private const float KButtonWidth = 18f;
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CheckInitialize(property, label);
            if (_foldout)
                return (_dictionary.Count + 1) * 17f;
            return 17f;
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CheckInitialize(property, label);
 
            position.height = 17f;
 
            var foldoutRect = position;
            foldoutRect.width -= 2 * KButtonWidth;
            EditorGUI.BeginChangeCheck();
            _foldout = EditorGUI.Foldout(foldoutRect, _foldout, label, true);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(label.text, _foldout);
 
            var buttonRect = position;
            buttonRect.x = position.width - KButtonWidth + position.x;
            buttonRect.width = KButtonWidth + 2;
 
            if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
            {
                AddNewItem();
            }
 
            buttonRect.x -= KButtonWidth;
 
            if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
            {
                ClearDictionary();
            }
 
            if (!_foldout)
                return;
 
            foreach (KeyValuePair<TK, TV> item in _dictionary)
            {
                var key = item.Key;
                var value = item.Value;
 
                position.y += 17f;
 
                var keyRect = position;
                keyRect.width /= 2;
                keyRect.width -= 4;
                EditorGUI.BeginChangeCheck();
                var newKey = DoField(keyRect, typeof(TK), key);
                if (EditorGUI.EndChangeCheck())
                {
                    try
                    {
                        _dictionary.Remove(key);
                        _dictionary.Add(newKey, value);
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                    break;
                }
 
                var valueRect = position;
                valueRect.x = position.width / 2 + 15;
                valueRect.width = keyRect.width - KButtonWidth;
                EditorGUI.BeginChangeCheck();
                value = DoField(valueRect, typeof(TV), value);
                if (EditorGUI.EndChangeCheck())
                {
                    _dictionary[key] = value;
                    break;
                }
 
                var removeRect = valueRect;
                removeRect.x = valueRect.xMax + 2;
                removeRect.width = KButtonWidth;
                if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
                {
                    RemoveItem(key);
                    break;
                }
            }
        }
 
        private void RemoveItem(TK key)
        {
            _dictionary.Remove(key);
        }
 
        private void CheckInitialize(SerializedProperty property, GUIContent label)
        {
            if (_dictionary == null)
            {
                var target = property.serializedObject.targetObject;
                _dictionary = fieldInfo.GetValue(target) as T;
                if (_dictionary == null)
                {
                    _dictionary = new T();
                    fieldInfo.SetValue(target, _dictionary);
                }
 
                _foldout = EditorPrefs.GetBool(label.text);
            }
        }
 
        private static readonly Dictionary<Type, Func<Rect, object, object>> Fields =
            new Dictionary<Type,Func<Rect,object,object>>
            {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
            };
 
        private static TF DoField<TF>(Rect rect, Type type, TF value)
        {
            Func<Rect, object, object> field;
            if (Fields.TryGetValue(type, out field))
                return (TF)field(rect, value);
 
            if (type.IsEnum)
                return (TF)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);
 
            if (typeof(UnityObject).IsAssignableFrom(type))
                return (TF)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);
 
            Debug.Log("Type is not supported: " + type);
            return value;
        }
 
        private void ClearDictionary()
        {
            _dictionary.Clear();
        }
 
        private void AddNewItem()
        {
            TK key;
            if (typeof(TK) == typeof(string))
                key = (TK)(object)"";
            else key = default(TK);
 
            var value = default(TV);
            try
            {
                _dictionary.Add(key, value);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}