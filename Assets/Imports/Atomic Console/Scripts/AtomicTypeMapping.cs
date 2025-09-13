using System;
using System.Collections.Generic;
namespace AtomicMapping
{
    public static class AtomicTypeMapping
    {
        public static Dictionary<string, string> TypeMappings = new Dictionary<string, string>
        {
            {"Single", "Float"},
            {"Int32", "Int"},
            {"Int16", "Short"},
            {"Int64", "Long"},
            {"UInt32", "Uint"},
            {"UInt16", "Ushort"},
            {"UInt64", "Ulong"},
            {"Boolean", "Bool"},
            {"String", "String"},
            {"Char", "Char"},
            {"Byte", "Byte"},
            {"SByte", "Sbyte"},
            {"Double", "Double"},
            {"Decimal", "Decimal"},
            {"Object", "Object"},
            {"Vector2", "Vector2"},
            {"Vector3", "Vector3"},
            {"Vector4", "Vector4"},
            {"Quaternion", "Quaternion"},
            {"Color", "Color"},
            {"Rect", "Rect"},
            {"Transform", "Transform"},
            {"GameObject", "GameObject"},
            {"Sprite", "Sprite"},
            {"Texture2D", "Texture2D"},
            {"AudioClip", "AudioClip"},
            {"AnimationClip", "AnimationClip"},
            {"Rigidbody", "Rigidbody"},
            {"Rigidbody2D", "Rigidbody2D"},
            {"Collider", "Collider"},
            {"Collider2D", "Collider2D"},
            {"Material", "Material"},
            {"Shader", "Shader"},
            {"Mesh", "Mesh"},
            {"Camera", "Camera"},
            {"Light", "Light"},
            {"Animator", "Animator"},
            {"Canvas", "Canvas"},
            {"RectTransform", "RectTransform"},
            {"Text", "Text"},
            {"Image", "Image"},
        };
        public static string GetType(Type type)
        {
            string mappedType;
            if (TypeMappings.TryGetValue(type.Name, out mappedType))
            {
                return mappedType;
            }
            return type.Name;
        }
    }
}
