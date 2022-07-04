#region Script Synopsis
    //A animatable and rotatable and bullet subclass. Allows for dynamically altering the bullet scale independently on x and y axis.
    //If the bullet's initial scale is greater or less than the "FromX/Y" value, it will first scale to the "From" value and then continue the routine from there.
#endregion

using System;
using UnityEngine;

namespace ND_VariaBULLET
{
    public class ShotBaseScalable : ShotBaseAnimatable
    {
        private const float rate = 0.1f;
        private const int minScaleSpeed = 1;
        private const int maxScaleSpeed = 15;

        [Header("Scale Settings")]

        [Tooltip("Enables or disables the scaling effect.")]
        public bool EnableScaling;

        [Range(minScaleSpeed, maxScaleSpeed)]
        [Tooltip("Sets the speed of the scaling effect.")]
        public int ScaleSpeed = 12;
        private int counter = 0;

        [RangeStepped(0.1f, 2, 1)]
        [Tooltip("Sets the starting point of the scaling effect on the X-axis.")]
        public float XFrom = 0.9f;

        [RangeStepped(0.1f, 2, 1)]
        [Tooltip("Sets the ending point of the scaling effect on the X-axis.")]
        public float XTo = 1.1f;

        [RangeStepped(0.1f, 2, 1)]
        [Tooltip("Sets the starting point of the scaling effect on the Y-axis.")]
        public float YFrom = 0.9f;

        [RangeStepped(0.1f, 2, 1)]
        [Tooltip("Sets the ending point of the scaling effect on the Y-axis.")]
        public float YTo = 1.1f;

        private float scaleX;
        private float scaleY;

        private bool fwdX;
        private bool fwdY;

        private bool isStartX;
        private bool isStartY;

        public override void InitialSet()
        {
            base.InitialSet();

            if (!EnableScaling)
                return;

            counter = 0;

            if (XFrom < XTo)
                scaleX = 0;
            else if (XFrom > XTo)
                scaleX = XFrom + 1;

            if (YFrom < YTo)
                scaleY = 0;
            else if (YFrom > YTo)
                scaleY = YFrom + 1;

            fwdX = fwdY = isStartX = isStartY = true;

            //Rounds off to start, adding a small amount to avoid floor/ceiling rounding error in the case of a x.5 value
            transform.localScale = new Vector2(roundToTenth(transform.localScale.x + 0.00001f), roundToTenth(transform.localScale.y + 0.00001f));

            if (transform.localScale.x == XFrom)
                isStartX = false;

            if (transform.localScale.y == YFrom)
                isStartY = false;
        }

        public override void Update()
        {
            base.Update();

            if (!EnableScaling)
                return;

            counter += 1;

            if (counter % (maxScaleSpeed + minScaleSpeed - ScaleSpeed) != 0)
                return;

            if (isStartX)
                startRoutine(ref isStartX, XFrom, () => { return roundToTenth(transform.localScale.x); }, (n) => { return new Vector2(n, transform.localScale.y); });
            else
                loopRoutine(ref scaleX, ref fwdX, XFrom, XTo, (n) => { return new Vector2(n, transform.localScale.y); });

            if (isStartY)
                startRoutine(ref isStartY, YFrom, () => { return roundToTenth(transform.localScale.y); }, (n) => { return new Vector2(transform.localScale.x, n); });
            else
                loopRoutine(ref scaleY, ref fwdY, YFrom, YTo, (n) => { return new Vector2(transform.localScale.x, n); });
        }

        private void startRoutine(ref bool isStart, float from, Func<float> currentScale, Func<float, Vector2> newScale)
        {
            float scale = currentScale();

            if (scale > from)
                transform.localScale = newScale(scale - rate);
            else if (scale < from)
                transform.localScale = newScale(scale + rate);

            if (currentScale() == from)
                isStart = false;
        }

        private void loopRoutine(ref float scale, ref bool fwd, float from, float to, Func<float, Vector2> newScale)
        {
            if (from == to)
                return;

            if (from < to)
                doFromTo(ref scale, ref fwd, from, to);
            else
                doToFrom(ref scale, ref fwd, to, from);

            if (fwd)
                scale += rate;
            else
                scale -= rate;

            scale = roundToTenth(scale);
            transform.localScale = newScale(scale);
        }

        private void doFromTo(ref float scale, ref bool isFwd, float from, float to)
        {
            if (scale <= from)
            {
                scale = from;
                isFwd = true;
            }
            else if (scale >= to)
            {
                scale = to;
                isFwd = false;
            }
        }

        private void doToFrom(ref float scale, ref bool isFwd, float to, float from)
        {
            if (scale >= from)
            {
                scale = from;
                isFwd = false;
            }
            else if (scale <= to)
            {
                scale = to;
                isFwd = true;
            }
        }

        private float roundToTenth(float val)
        {
            return (float)Math.Round(val, 1);
        }
    }
}