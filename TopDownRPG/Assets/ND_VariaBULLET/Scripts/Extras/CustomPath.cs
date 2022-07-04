#region Script Synopsis
    //Moves objects in linear paths
#endregion

using UnityEngine;
using System;

namespace ND_VariaBULLET
{
    public class CustomPath : MonoBehaviour
    {
        [Tooltip("Enables looping behavior once all paths complete.")]
        public PathLoopType LoopBehaviour;

        [Tooltip("Sets the underlying positioning system in case of parenting. [local = transform.localPostion; world = transform.position;].")]
        public CoordinateType CoordsSystem;
        private Func<Vector2> getPos;
        private Action<Vector2> setPos;

        private bool isRev;

        [Tooltip("Sets the number of paths.")]
        public PathData[] Paths;
        private int index;

        [Tooltip("Enables drawing the path in scene view.")]
        public bool DebugDraw;

        [Tooltip("Set a time (in frames) when this object is destroyed. [0 = never destroyed].")]
        public int autoDestroy;
        private Timer destroyTime;

        [Tooltip("Sets the type of destruction that occurs when autoDestroy time has been reached, or once the path has completed (in the case when LoopBehaviour = None).")]
        public DestroyType DestroyAction;

        void Start()
        {
            autoDestroy = Math.Abs(autoDestroy);
            destroyTime = new Timer(0);

            if (CoordsSystem == CoordinateType.Local)
            {
                getPos = getLocalPos;
                setPos = setLocalPos;
            }
            else
            {
                getPos = getWorldPos;
                setPos = setWorldPos;
            }

            if (Paths.Length < 2)
                LoopBehaviour = PathLoopType.None;

            Paths[0].Init(getPos());
        }

        void Update()
        {
            if (DestroyCheck())
                DoDestroy();

            if (Paths[index].Delay())
                return;

            Vector2 prevPos = transform.position;
            setPos(Paths[index].move());
            Vector2 newPos = transform.position;

            if (DebugDraw)
                Utilities.DebugDrawLine(prevPos, newPos, Color.green, 20);

            goToNext(Paths[index].isDone);      
        }

        private void goToNext(bool isDone)
        {
            if (isDone)
            {
                if (!isRev)
                {
                    if (index < Paths.Length - 1)
                    {
                        index++;
                        Paths[index].Init(getPos());
                        isRev = false;
                    }
                    else
                    {
                        if (LoopBehaviour == PathLoopType.None)
                            DoDestroy();                
                        else if (LoopBehaviour == PathLoopType.PingPong)
                            isRev = true;
                        else
                        {
                            index = 0;
                            Paths[0].Init(getPos());
                        }
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        index--;
                        Paths[index].Init(getPos());
                        isRev = true;
                    }
                    else
                        isRev = false;
                }
            }
        }

        private Vector2 getLocalPos()
        {
            return transform.localPosition;
        }

        private Vector2 getWorldPos()
        {
            return transform.position;
        }

        private void setLocalPos(Vector2 pos)
        {
            transform.localPosition = pos;
        }

        private void setWorldPos(Vector2 pos)
        {
            transform.position = pos;
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


        [System.Serializable]
        public struct PathData
        {
            [Tooltip("Sets the position the object moves to.")]
            public Vector2 ToPosition;
            private Vector2 FromPosition;

            [Tooltip("Sets the movement speed.")]
            public int speed;

            [Tooltip("Define a custom rate of change, represented as a curvature, as it moves along the path [Undefined Curve = Linear].")]
            public AnimationCurve Curve;

            [Tooltip("Sets a delay time (in frames) before the object moves to the To Position.")]
            public int initialDelay;
            private Timer delay;

            private float accumulator;

            public bool isDone { get; private set; }

            public void Init(Vector2 startPos)
            {
                FromPosition = startPos;
                speed = Math.Abs(speed);
                initialDelay = Math.Abs(initialDelay);
                delay = new Timer(0);
                delay.Reset();
                accumulator = 0;
                isDone = false;

                if (Curve.length < 2)
                {
                    for (int i = 0; i < Curve.length; i++)
                        Curve.RemoveKey(i);

                    Curve.AddKey(0, 0);
                    Curve.AddKey(1, 1);
                    Curve.preWrapMode = Curve.postWrapMode = WrapMode.Clamp;
                }
            }

            public bool Delay()
            {
                if (initialDelay <= 0)
                    return false;

                delay.RunOnce(initialDelay);
                return !delay.Flag;
            }

            public Vector2 move()
            {
                accumulator += Time.deltaTime;

                float distance = Vector2.Distance(FromPosition, ToPosition);
                float relativeSpeed = speed / distance;

                Vector2 lerp = Vector2.Lerp(FromPosition, ToPosition, Curve.Evaluate(relativeSpeed * accumulator));

                if (lerp == ToPosition)
                    isDone = true;

                return lerp;
            }
        }
        public enum CoordinateType
        {
            Local,
            World
        }

        public enum PathLoopType
        {
            None,
            Restart,
            PingPong
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