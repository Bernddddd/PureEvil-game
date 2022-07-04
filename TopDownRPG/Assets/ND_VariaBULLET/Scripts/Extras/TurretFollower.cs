#region Script Synopsis
    //Rotates a top-level object, pointing at a target object
#endregion

using System;
using UnityEngine;

namespace ND_VariaBULLET
{
	public class TurretFollower : MonoBehaviour
	{
		[Tooltip("Directly sets the transform of the target to follow. [Takes precedence over targetFromTag].")]
		public Transform TargetDirect;

		[Tooltip("Sets target(s) to follow by tag. Finds closest to follow when more than one target is active.")]
		public string TargetFromTag;

		[Range(1, 50)]
		[Tooltip("Sets the rotation speed at which the turret turns to face the target.")]
		public int TargetingSpeed = 10;

		[Range(1, 10)]
		[Tooltip("Sets an FPS interval at which point the shot re-checks for the closest target (when TargetFromTag is set) to home in on. [Higher number = more frequent re-check].")]
		public int RecalculationFPS = 1; //used to recalc closest target every 6-to-60 frames.

		private Transform targetLock;
		private HomingCalc calc;

		private void Start()
		{
			RecalculationFPS = 60 / RecalculationFPS;
			calc = new HomingCalc();

			if (TargetDirect != null)
				targetLock = TargetDirect;
			else
			{
                calc.recalcClosestObject(this.transform, ref targetLock, 0, TargetFromTag); //Simply used to set target on first frame
				calc.recalcTimer.Reset();
            }
		}

		void Update()
		{
			calc.recalcClosestObject(this.transform, ref targetLock, RecalculationFPS, TargetFromTag);

			if (targetLock == null)
				return;
				
			Vector2 direction = targetLock.position - transform.position;
			direction = (transform.lossyScale.x < 0) ? -direction : direction;

			transform.rotation = CalcObject.VectorToRotationSlerp(
				transform.rotation, direction, TargetingSpeed
			);
		}
	}
}