using UnityEngine;

namespace LethalESP
{
    class Esp : MonoBehaviour
    {
        Object[] cameras;
        Object[] objects;
        int camIndex;
        bool shouldRender;
        public void Start()
        {
            cameras = FindObjectsByType(typeof(Camera), FindObjectsSortMode.InstanceID);
            objects = FindObjectsByType(typeof(GrabbableObject), FindObjectsSortMode.None);
            camIndex = 9;
            shouldRender = true;
        }
        public void Update()
        {
            if (Event.current.Equals(Event.KeyboardEvent("delete")))
            {
                camIndex++;
                if(camIndex >= cameras.Length)
                {
                    camIndex = 0;
                }
            }
            if (Event.current.Equals(Event.KeyboardEvent("home")))
            {
                objects = FindObjectsByType(typeof(GrabbableObject), FindObjectsSortMode.None);
            }
            if (Event.current.Equals(Event.KeyboardEvent("insert")))
            {
                shouldRender = !shouldRender;
            }
        }
        public void OnGUI()
        {
            if (shouldRender)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    Vector3 loc = ((GrabbableObject)objects[i]).transform.position;
                    Vector3 w2s = ((Camera)cameras[camIndex]).WorldToScreenPoint(loc);
                    if (w2s.z < 0) continue;
                    //ZatsRenderer.DrawString(new Vector2(100, 50), "camIndex: " + camIndex);
                    //ZatsRenderer.DrawString(new Vector2(100, 25), "pos: " + ((Camera)cameras[camIndex]).transform.position);
                    //ZatsRenderer.DrawString(new Vector2(100, 75), "objpos: " + ((GrabbableObject)objects[0]).transform.position);
                    //ZatsRenderer.DrawString(new Vector2(100, 100), "scrpos: " + ((Camera)cameras[camIndex]).WorldToScreenPoint(((GrabbableObject)objects[0]).transform.position));
                    //ZatsRenderer.DrawString(new Vector2(100, 125), "scrdim: " + ((Camera)cameras[camIndex]).pixelWidth + " x " + ((Camera)cameras[camIndex]).pixelHeight);
                    Vector2 fixedPos = new Vector2(w2s.x * 2.232558140f, (((Camera)cameras[camIndex]).pixelHeight - w2s.y) * 2.076923077f);
                    DrawClearBox(fixedPos, new Vector2(50, 50), Color.red);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), ((GrabbableObject)objects[i]).itemProperties.itemName);
                }
            }
        }

        private void DrawClearBox(Vector2 position, Vector2 size, Color color)
        {
            // 0 1
            // 2 3
            Vector2[] corners = new Vector2[4];
            corners[0] = new Vector2(position.x - (size.x / 2f), position.y - (size.y / 2f));
            corners[1] = new Vector2(corners[0].x + size.x, corners[0].y);
            corners[2] = new Vector2(corners[0].x, corners[0].y + size.y);
            corners[3] = new Vector2(corners[2].x + size.x, corners[2].y);
            ZatsRenderer.DrawLine(corners[0], corners[1], color, 1f);
            ZatsRenderer.DrawLine(corners[0], corners[2], color, 1f);
            ZatsRenderer.DrawLine(corners[2], corners[3], color, 1f);
            ZatsRenderer.DrawLine(corners[3], corners[1], color, 1f);
        }
    }
}
