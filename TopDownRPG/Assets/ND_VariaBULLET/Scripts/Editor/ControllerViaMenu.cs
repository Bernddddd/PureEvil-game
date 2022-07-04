#region Script Synopsis
    //Adds a menu item that creates a Controller via Hierarchy (right-click) or via Menu (GameObject)
#endregion

using UnityEditor;
using UnityEngine;

namespace ND_VariaBULLET.EditorGUI
{
    public class ControllerViaMenu : MonoBehaviour
    {
        [MenuItem("GameObject/VariaBULLET2D/Controller", false, 1)]
        static void Instantiate()
        {
            GameObject gO = Instantiate(Resources.Load<GameObject>("ND_VariaBullet/ControllerMenuItem/Controller/Origin"));

            if (Selection.activeGameObject != null)
                gO.transform.parent = Selection.activeGameObject.transform;

            gO.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}