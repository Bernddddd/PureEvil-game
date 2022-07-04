#region Script Synopsis
//Utitilies class contains common static helper methods used throughout the codebase.
//CalcObject class containts common static helper methods with common calculations used by shots and firing scripts.
#endregion

using System;
using UnityEngine;

namespace ND_VariaBULLET
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangeSteppedAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;
        public readonly int step;

        public RangeSteppedAttribute(float min, float max, int step)
        {
            this.min = min;
            this.max = max;
            this.step = step;
        }
    }

    public static class Utilities
    {
        public static void Warn(string message, params object[] senderObjects)
        {
            string objects = "";

            if (senderObjects != null)
                foreach (object item in senderObjects)
                    objects += item.ToString() + "; ";

            Debug.Log("WARNING: " + message + " | Object(s): " + objects);
        }

        public static bool IsEditorMode()
        {
            if (!Application.isPlaying)
                return true;
            else
                return false;
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }

        public static void DebugDrawLine(Vector2 start, Vector2 end)
        {
            #if UNITY_EDITOR
            Debug.DrawLine(start, end, Color.green, 2);
            #endif
        }

        public static void DebugDrawLine(Vector2 start, Vector2 end, Color color, float duration)
        {
            #if UNITY_EDITOR
            Debug.DrawLine(start, end, color, duration);
            #endif
        }

        public static void ExecutionTime<T>(string prefix, int iterations, T obj, Action<T> routine)
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < iterations; i++)
                routine(obj);

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime.Subtract(startTime);
            Debug.Log(prefix + " Execution Time: " + duration.TotalMilliseconds + "ms");
        }

        public static void ExecutionTime(string prefix, int iterations, Action routine)
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < iterations; i++)
                routine();

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime.Subtract(startTime);
            Debug.Log(prefix + " Execution Time: " + duration.TotalMilliseconds + "ms");
        }
    }

    public static class CalcObject
    {
        private static bool IsOffScreen(Vector2 position, Vector2 padding)
        {
            Vector2 tmpPos = GlobalShotManager.Instance.MainCam.WorldToScreenPoint(position);

            if (tmpPos.x < 0 - padding.x || tmpPos.x > Screen.width + padding.x || tmpPos.y < 0 - padding.y || tmpPos.y > Screen.height + padding.y)
                return true;
            else
                return false;
        }

        public static bool IsOutBounds(Vector2 position)
        {
            return IsOffScreen(position, new Vector2(0, 0));
        }

        public static bool IsOutBounds(Transform transform)
        {
            return IsOffScreen(transform.position, GlobalShotManager.Instance.OutBoundsRange);
        }

        public static Vector2 RotationToShotVector(float rotationAngle)
        {
            return new Vector2(Mathf.Cos(rotationAngle * Mathf.Deg2Rad), Mathf.Sin(rotationAngle * Mathf.Deg2Rad));
        }

        public static Quaternion VectorToRotationSlerp(Quaternion srcRotation, Vector3 targetPos, float slerpSpeed)
        {      
            float angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
            Quaternion rotationTarget = Quaternion.AngleAxis(angle, Vector3.forward);
            return Quaternion.Slerp(srcRotation, rotationTarget, Time.deltaTime * slerpSpeed);
        }

        public static float AngleBetweenVectors(Vector2 from, Vector2 to)
        {
            return Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        }

        public static Vector2 AngleToAddForce(Vector2 from, Vector2 to, float force)
        {
            float angle = CalcObject.AngleBetweenVectors(from, to);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;

            return dir * force;
        }

        public static float getMaxScreenDistance()
        {
            float maxScreen;

            if (GlobalShotManager.Instance.MainCam.aspect > 1) //horizontal viewport
                maxScreen = GlobalShotManager.Instance.MainCam.orthographicSize * 2 * (GlobalShotManager.Instance.MainCam.pixelRect.width / GlobalShotManager.Instance.MainCam.pixelRect.height);
            else
                maxScreen = GlobalShotManager.Instance.MainCam.orthographicSize * 2 * (GlobalShotManager.Instance.MainCam.pixelRect.height / GlobalShotManager.Instance.MainCam.pixelRect.width);

            float padding = maxScreen / 4;

            return maxScreen + padding;
        }
    }
}