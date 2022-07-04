#region Script Synopsis
    //Stepped automator, which automates controller parameters in a series of stepped intervals.
    //Becomes attached to a controller via the attached spreadpattern (controller) script.
    //Learn more about automators at: https://neondagger.com/variabullet2d-in-depth-controller-guide/#automators
#endregion

using UnityEngine;

namespace ND_VariaBULLET
{
    public class AutomateStepped : AutomateBase
    {
        [Tooltip("Sets time interval for moving from one step to the next. [higher number = longer delay].")]
        public float Interval;

        [Tooltip("Sets values as a series of steps for the control being modified.")]
        public float[] Steps;

        [Tooltip("Takes precedence over the controller's TriggerAutoFire & FireCommand settings, instead syncing shot triggering to each step change.")]
        public bool AutoSyncTrigger;

        [Tooltip("Sets the duration at each step when AutoSyncTrigger is enabled. [0 = default AutoHoldDuration].")]
        public float AutoSyncDuration;

        [Tooltip("Sets the amount of trigger increments before this automation procedure ends. [0 = infinite].")]
        public int TriggerPasses;
        private int triggersPassed = 0;

        [Tooltip("Ensures that firing continues after the final step finishes.")]
        public bool KeepFiringOnExit;

        [Tooltip("Prints out the amount of frames it took for the entire automation procedure to complete.")]
        public string FinishTimeDebug;

        private int index;
        private bool isFwd = true;

        protected override void Awake()
        {
            Interval = Mathf.Abs(Interval);
            AutoSyncDuration = Mathf.Abs(AutoSyncDuration);
            TriggerPasses = System.Math.Abs(TriggerPasses);

            accumulator = Interval + 1;
            index = -1;

            base.Awake();
        }

        void Start()
        {
            if (AutoSyncTrigger && pattern.TriggerAutoFire)
            {
                if (pattern.TriggerDelay > this.InitialDelay)
                {
                    Utilities.Warn("BasePattern.TriggerAutoFire.TriggerDelay conflicts with Stepped Automator.AutoSyncTrigger.InitialDelay. Disabling controller's BasePattern.TriggerAutoFire setting. If you want to use TriggerAutoFire as the main trigger instead, turn off the Stepped Automator trigger", this.transform.parent.parent);
                    pattern.TriggerAutoFire = false;
                }
            }
        }

        void Update()
        {
            delay.RunOnce(InitialDelay);
            if (!delay.Flag) return;

            if (Steps.Length == 0) { Utilities.Warn("No steps have been set for ", this, transform.parent.parent);  return; }

            accumulator += Time.deltaTime * 4;
            accumulatorTotal += Time.deltaTime;
            controlLink[Destination]((method(0, Steps.Length - 1, Interval)));
        }

        protected override float SinglePass(float start, float end, float interval)
        {
            if (index == end)
            {
                if (AutoSyncTrigger && KeepFiringOnExit)
                    pattern.FireCommand = BasePattern.CommandType.Automatic;

                this.enabled = false;
                getCompletionTime(ref FinishTimeDebug);
            }
                
                
            if (accumulator > interval)
            {
                index++;

                accumulator = 0;
                triggerCheck();
            } 

            return Steps[index];
        }

        protected override float Continuous(float start, float end, float interval)
        {
            if (accumulator > interval)
            {
                if (index < end)
                    index++;
                else
                    index = (int)start;
                    
                accumulator = 0;
                triggerCheck();
            }
         
            return Steps[index];
        }

        protected override float PingPong(float start, float end, float interval)
        {
            if (accumulator > interval)
            {
                accumulator = 0;

                if (Steps.Length > 1)
                {
                    if (isFwd)
                    {
                        if (index < end)
                            index++;
                        else
                            index = (int)end - 1; isFwd = false; accumulator = 0;
                    }
                    else
                    {
                        if (index > start)
                            index--;
                        else { index = (int)start + 1; isFwd = true; accumulator = 0; }
                    }
                }
                else
                {
                    index = 0;
                    triggerCheck();
                    return Steps[index];
                }

                triggerCheck();
            }

            return Steps[index];
        }

        protected override float Randomized(float start, float end, float interval)
        {
            if (accumulator > interval)
            {
                int rand = index;

                if (Steps.Length > 2)
                    while (index == rand)
                        rand = Random.Range((int)start, (int)end + 1);
                else if (Steps.Length > 1)
                    rand = Random.Range((int)start, (int)end + 1);
                else
                    rand = 0;

                index = rand;
                accumulator = 0;
                triggerCheck();
            }

            return Steps[index];
        }

        private void triggerCheck()
        {
            if (AutoSyncTrigger)
            {
                pattern.FireCommand = BasePattern.CommandType.AutomaticAutoHold;

                if (AutoSyncDuration != 0)
                    pattern.AutoHoldDuration = AutoSyncDuration;
            }

            triggersPassed++;

            if (triggersPassed == TriggerPasses)
            {
                this.enabled = false;
                getCompletionTime(ref FinishTimeDebug);

                if (AutoSyncTrigger && KeepFiringOnExit)
                    pattern.FireCommand = BasePattern.CommandType.Automatic;
            }

            pattern.TriggerAutoFire = true;
        }
    }
}