#region Script Synopsis
    //Moves objects in cyclical curved paths (eg: circles, ovals and sine waves).
#endregion

using UnityEngine;
using System;

namespace ND_VariaBULLET
{
    public class SinePath : MonoBehaviour
    {
        [Tooltip("Multiplies values for speed and amp on all axes.")]
        [Range(0.1f, 10)]
        public float Multiplier = 1;

        [Tooltip("Links Y-axis values to X-axis as a multiple. [0 = unlinked].")]
        [Range(-10, 100)]
        public int LinkAxes;

        [Tooltip("Determines the link behavior when LinkAxis is enabled (not 0). Normal creates circular/oval patterns, while inverted creates sine patterns.")]
        public LinkType linkType;

        [Tooltip("Speed on the X-axis.")]
        [Range(0.1f, 100)]
        public float XSpeed = 1;

        [Tooltip("Amplitude (travel length) on the X-axis.")]
        [Range(0.1f, 100)]
        public float XAmp = 1;

        [Tooltip("Speed on the Y-axis.")]
        [Range(0.1f, 100)]
        public float YSpeed = 1;

        [Tooltip("Amplitude (travel length) on the Y-axis.")]
        [Range(0.1f, 100)]
        public float YAmp = 1;

        [Tooltip("Begins X-axis movement in negative direction when enabled.")]
        public bool InvertX;

        [Tooltip("Begins Y-axis movement in negative direction when enabled.")]
        public bool InvertY;

        [Tooltip("Enables drawing the path in scene view.")]
        public bool DebugDraw;

        [Tooltip("Sets a delay time (in frames) before the object moves to the To Position.")]
        public int initialDelay;
        private Timer delay;

        [Tooltip("Moves the object as an adjustment on X/Y axis.")]
        public Vector2 Nudge;

        [Tooltip("Set a time (in frames) when this object is destroyed. [0 = never destroyed].")]
        public int autoDestroy;
        private Timer destroyTime;

        [Tooltip("Sets the type of destruction that occurs when autoDestroy time has been reached, or once the path has completed (in the case when LoopBehaviour = None).")]
        public DestroyType DestroyAction;

        private float accumulator = 0;
        private Vector2 startPos;

        void Start()
        {
            initialDelay = Math.Abs(initialDelay);
            autoDestroy = Math.Abs(autoDestroy);
            delay = new Timer(0);
            destroyTime = new Timer(0);
            startPos = (Vector2)transform.localPosition - move() + Nudge;
        }

        void Update()
        {
            if (DestroyCheck())
                DoDestroy();

            if (Delay())
                return;

            Vector2 prevPos = transform.position;
            transform.localPosition = move();
            Vector2 newPos = transform.position;

            if (DebugDraw)
                Utilities.DebugDrawLine(prevPos, newPos, Color.green, 20);
        }

        private Vector2 move()
        {
            accumulator += Time.deltaTime;

            if (LinkAxes != 0)
            {
                if (linkType == LinkType.Normal)
                {
                    YSpeed = (LinkAxes < 0) ? XSpeed / LinkAxes : XSpeed * LinkAxes;
                    YAmp = (LinkAxes < 0) ? XAmp / LinkAxes : XAmp * LinkAxes;
                }
                else
                {
                    YSpeed = (LinkAxes < 0) ? XAmp / LinkAxes : XAmp * LinkAxes;
                    YAmp = (LinkAxes < 0) ? XSpeed / LinkAxes : XSpeed * LinkAxes;
                }
            }

            float x = Mathf.Cos(accumulator * XSpeed / XAmp) * XAmp * Multiplier * (InvertX ? -1 : 1);
            float y = Mathf.Sin(accumulator * YSpeed / YAmp) * YAmp * Multiplier * (InvertY ? -1 : 1);

            return startPos + Nudge + new Vector2(x, y);
        }

        private bool Delay()
        {
            if (initialDelay <= 0)
                return false;

            delay.RunOnce(initialDelay);
            return !delay.Flag;
        }

        private bool DestroyCheck()
        {
            if (autoDestroy <= 0)
                return false;

            destroyTime.RunOnce(autoDestroy);
            return destroyTime.Flag;
        }
        private void DoDestroy()
        {
            switch (DestroyAction)
            {
                case DestroyType.DestroyObject:
                    GameObject.Destroy(this.gameObject);
                    break;
                case DestroyType.DestroyParent:
                    GameObject.Destroy(transform.root.gameObject);
                    break;
                case DestroyType.DisableObject:
                    this.transform.gameObject.SetActive(false);
                    break;
                case DestroyType.DisableParent:
                    this.transform.root.gameObject.SetActive(false);
                    break;
                case DestroyType.DisableScript:
                    this.enabled = false;
                    break;
            }
        }

        public enum LinkType
        {
            Normal,
            Inverted
        }

        public enum DestroyType
        {
            DisableScript,
            DisableObject,
            DisableParent,
            DestroyObject,
            DestroyParent
        }
    }
}