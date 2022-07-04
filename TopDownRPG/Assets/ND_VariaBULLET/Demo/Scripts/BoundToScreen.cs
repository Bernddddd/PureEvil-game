#region Script Synopsis
	//A demo script for clamping an object to a predetermined boundary. EG: preventing player from traveling past edges of the screen.
#endregion

using UnityEngine;

namespace ND_VariaBULLET.Demo
{
	public class BoundToScreen : MonoBehaviour
	{
		public Rect ScreenBounds;
		public bool DrawBoundary;

		public Vector2 SpriteEdgeAdjust;
		private Vector2 spriteSize;

		void Awake()
		{
			Sprite sprite = GetComponent<SpriteRenderer>().sprite;
			spriteSize.x = sprite.bounds.size.y / 2;
			spriteSize.y = sprite.bounds.size.x / 2;
		}

		void LateUpdate()
		{
			clampToBoundary();
		}

		private void clampToBoundary()
		{
			Rect boundary = ScreenBounds;

			transform.position = new Vector3(
				Mathf.Clamp(transform.position.x, boundary.xMin + spriteSize.x + SpriteEdgeAdjust.x, boundary.xMax - spriteSize.x - SpriteEdgeAdjust.x),
				Mathf.Clamp(transform.position.y, boundary.yMin + spriteSize.y + SpriteEdgeAdjust.y, boundary.yMax - spriteSize.y - SpriteEdgeAdjust.y),
				transform.position.z
			);
		}

		void OnDrawGizmos()
		{
			#if UNITY_EDITOR
			if (!DrawBoundary)
				return;

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(new Vector2(ScreenBounds.xMin, ScreenBounds.yMin), new Vector2(ScreenBounds.xMax, ScreenBounds.yMin));
			Gizmos.DrawLine(new Vector2(ScreenBounds.xMax, ScreenBounds.yMin), new Vector2(ScreenBounds.xMax, ScreenBounds.yMax));

			Gizmos.DrawLine(new Vector2(ScreenBounds.xMin, ScreenBounds.yMax), new Vector2(ScreenBounds.xMin, ScreenBounds.yMin));
			Gizmos.DrawLine(new Vector2(ScreenBounds.xMax, ScreenBounds.yMax), new Vector2(ScreenBounds.xMin, ScreenBounds.yMax));
			#endif
		}

	}
}