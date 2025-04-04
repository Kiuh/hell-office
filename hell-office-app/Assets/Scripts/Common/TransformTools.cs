﻿using UnityEngine;

namespace Common
{
    public static class TransformTools
    {
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            while (transform.childCount > 0)
            {
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public static void SetXPosition(this Transform transform, float x)
        {
            transform.position = new(x, transform.position.y, transform.position.z);
        }

        public static void SetYPosition(this Transform transform, float y)
        {
            transform.position = new(transform.position.x, y, transform.position.z);
        }

        public static void SetZPosition(this Transform transform, float z)
        {
            transform.position = new(transform.position.x, transform.position.y, z);
        }

        public static void SetLocalXPosition(this Transform transform, float x)
        {
            transform.localPosition = new(x, transform.localPosition.y, transform.localPosition.z);
        }

        public static void SetLocalYPosition(this Transform transform, float y)
        {
            transform.localPosition = new(transform.localPosition.x, y, transform.localPosition.z);
        }

        public static void SetLocalZPosition(this Transform transform, float z)
        {
            transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, z);
        }
    }
}
