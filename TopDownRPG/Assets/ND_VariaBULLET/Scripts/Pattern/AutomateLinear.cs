#region Script Synopsis
//Linear automator, which automates controller parameters in a fluid progression.
//Becomes attached to a controller via the attached spreadpattern (controller) script.
//Learn more about automators at: https://neondagger.com/variabullet2d-in-depth-controller-guide/#automators
#endregion

using System;
using UnityEngine;

namespace ND_VariaBULLET
{
    public class AutomateLinear : AutomateBase
    {
        [Tooltip("Sets the initial value for the control being modified.")]
        public float From;

        [Tooltip("Sets the end value for the control being modified.")]
        public float To;

        [Tooltip("Sets the speed at which values gradually modify between From and To.")]
        public float Speed;

        [Tooltip("Creates a smoothing effect between the from/to values.")]
        public bool Ease;

        [Tooltip("Define a custom rate of change, represented as a curvature, between the from/to values except in the case of randomized behaviors. [Undefined Curve = Linear].")]
        public AnimationCurve Curve;

        [Tooltip("Sets the amount of full cycles through the From/To values before this automation procedure ends. [0 = infinite].")]
        public int TotalCycles;
        private int totalCycles;

        [Tooltip("Prints out the amount of frames it took for the entire automation procedure to complete.")]
        public string FinishTimeDebug;

        private Func<float, float, float, float>[] lerpMethod;

        private bool isRev;
        private float currentPing;

        protected override void Awake()
        {
            Speed = Mathf.Abs(Speed);
            TotalCycles = Math.Abs(TotalCycles);

            lerpMethod = new Func<float, float, float, float>[2] { Mathf.Lerp, Mathf.SmoothStep };
            lerpCurveInit();

            base.Awake();
        }

        void Update()
        {
            delay.RunOnce(InitialDelay);
            if (!delay.Flag) return;

            accumulator += Time.deltaTime;
            accumulatorTotal += Time.deltaTime;
            controlLink[Destination]((method(From, To, Speed)));
        }

        protected override float SinglePass(float from, float to, float speed)
        {
            float relativeSpeed = getRelativeSpeed(from, to, speed);
            float current = lerpMethod[Convert.ToInt16(Ease)](from, to, Curve.Evaluate(accumulator * relativeSpeed));

            if (to == current)
            {
                this.enabled = false;
                getCompletionTime(ref FinishTimeDebug);
            }
                
            return current;
        }

        protected override float Continuous(float from, float to, float speed)
        {
            if (TotalCycles == 0)
            {
                float direction = (from <= to) ? 1 : -1;
                return from + accumulator * speed * direction;
            }

            float relativeSpeed = getRelativeSpeed(from, to, speed);
            float current = lerpMethod[Convert.ToInt16(Ease)](from, to, Curve.Evaluate(accumulator * relativeSpeed));

            if (to == current)
            {
                totalCycles++;
                if (totalCycles == TotalCycles)
                {
                    this.enabled = false;
                    getCompletionTime(ref FinishTimeDebug);

                    return current;
                }

                accumulator = 0;
                current = from;
            }

            return current;
        }

        protected override float PingPong(float from, float to, float speed)
        {
            float relativeSpeed = getRelativeSpeed(from, to, speed);
            float curve = Curve.Evaluate(Mathf.PingPong(accumulator * relativeSpeed, 1));
            float current = lerpMethod[Convert.ToInt16(Ease)](from, to, curve);

            if (TotalCycles > 0)
            {
                if (curve < currentPing && !isRev)
                {
                    isRev = true;
                    currentPing = 1;
                }
                else if (curve > currentPing & isRev)
                {
                    isRev = false;
                    currentPing = curve;
                    totalCycles++;
                }
                else
                    currentPing = curve;

                if (totalCycles == TotalCycles)
                {
                    this.enabled = false;
                    getCompletionTime(ref FinishTimeDebug);
                    return from;
                }
            }

            return current;
        }

        private float prevRand;

        protected override float Randomized(float from, float to, float speed)
        {
            float accMod = accumulator * 60;

            if (accumulator == Time.deltaTime)
            {
                prevRand = UnityEngine.Random.Range(from, to);

                if (TotalCycles > 0)
                {
                    totalCycles++;
                    if (totalCycles == TotalCycles)
                    {
                        this.enabled = false;
                        getCompletionTime(ref FinishTimeDebug);
                    }
                }
            }

            if (accMod >= Mathf.Abs(100 - Mathf.Min(speed, 100)))
                accumulator = 0;

            return prevRand;
        }

        private float getRelativeSpeed(float from, float to, float speed)
        {
            float difference = from - to;
            float relativeSpeed = (difference != 0) ? speed / Mathf.Abs(difference) : speed;

            return relativeSpeed;
        }

        private void lerpCurveInit()
        {
            if (Curve.length < 2)
            {
                for (int i = 0; i < Curve.length; i++)
                    Curve.RemoveKey(i);

                Curve.AddKey(0, 0);
                Curve.AddKey(1, 1);
                Curve.preWrapMode = Curve.postWrapMode = WrapMode.Clamp;
            }
        }
    }
}