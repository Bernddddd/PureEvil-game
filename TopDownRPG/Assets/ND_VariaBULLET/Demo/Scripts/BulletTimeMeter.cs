#region Script Synopsis
	//A demonstration script for controlling GAME SPEED globally. Used in the "BossAutomatorSeries" demo scene. NOTE: for only controlling global shot speed, see the GlobalSlowDown demo script. 
#endregion

using UnityEngine;

namespace ND_VariaBULLET.Demo
{
	public class BulletTimeMeter : MonoBehaviour
	{
		public KeyCode button;
		private bool toggle;

		[Range(0.01f, 0.9f)]
		public float SlowestSpeed = 0.3f;
		private const float normalSpeed = 1;

		public float Meter;
		private float maxMeter = 1000;
	
		[Range(1, 10)]
		public float TransitionSpeed = 6;
		public int DepletionRate = 3;

		private const int engagePenalty = 50;
		private GUIStyle FontStyle = new GUIStyle();

		private int scaledFont { get { return (Screen.width + Screen.height) / 140; } }

		void Start()
        {
			maxMeter = Meter;
        }

		void Update()
		{
			if (Input.GetKeyDown(button) && Meter > 0)
            {
				toggle = !toggle;

				if (Meter > maxMeter / 10)
					Meter = Mathf.Max(0, Meter - engagePenalty);
			}
				
			if (toggle)
            {
				Meter -= Timer.deltaCounter * DepletionRate;

				if (Meter <= 0)
                {
					Meter = 0;
					toggle = false;
					return;
				}
				setTimeScale(SlowestSpeed, rampSpeed: TransitionSpeed / 3);		
            }
            else
				setTimeScale(normalSpeed, rampSpeed: TransitionSpeed / 2);
		}

		private void setTimeScale(float timeScale, float rampSpeed)
        {
			GlobalShotManager.Instance.GlobalTimeScale = Mathf.Lerp(GlobalShotManager.Instance.GlobalTimeScale, timeScale, Time.deltaTime * rampSpeed);
		}

        void OnGUI()
        {
			FontStyle.fontSize = scaledFont + 6;
			FontStyle.fontStyle = UnityEngine.FontStyle.Bold;
			FontStyle.normal.textColor = Color.yellow;
			drawOSD();
        }

		void drawOSD()
        {
			GUI.Label(
				new Rect(Screen.width / 1.2f, Screen.height / 1.1f, Screen.width, Screen.height),
				string.Format("BulletTime: {0}", (int)Meter), FontStyle
			);
		}
	}
}