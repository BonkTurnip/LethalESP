using UnityEngine;
using UnityEngine.Rendering;
using GameNetcodeStuff;

namespace LethalESP
{
    class Esp : MonoBehaviour
    {
        Camera[] cameras;
        GrabbableObject[] objects;
        EntranceTeleport[] doors;
        EnemyAI[] enemies;
        Landmine[] mines;
        Turret[] turrets;
        StartMatchLever lever;
        PlayerControllerB localPlayer;
        int miniCamIndex;
        bool shouldRender;
        bool shouldRenderMini;
        float nextEnemyScan;
        private readonly float margin = 100f;
        private readonly float maxSize = 500f;
        public void Start()
        {
            cameras = FindObjectsByType(typeof(Camera), FindObjectsSortMode.InstanceID) as Camera[];
            lever = FindObjectOfType(typeof(StartMatchLever)) as StartMatchLever;
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            Rescan();
            miniCamIndex = 0;
            shouldRender = true;
            shouldRenderMini = true;
            nextEnemyScan = Time.time;
        }
        public void Update()
        {
            if (Event.current.Equals(Event.KeyboardEvent("home")))
            {
                Rescan();
            }
            if (Event.current.Equals(Event.KeyboardEvent("insert")))
            {
                shouldRender = !shouldRender;
            }
            if (Event.current.Equals(Event.KeyboardEvent("end")))
            {
                miniCamIndex++;
                if(miniCamIndex >= cameras.Length)
                {
                    miniCamIndex = 0;
                }
            }
            if (Event.current.Equals(Event.KeyboardEvent("page down")))
            {
                shouldRenderMini = !shouldRenderMini;
            }
            if (nextEnemyScan < Time.time)
            {
                enemies = FindObjectsByType(typeof(EnemyAI), FindObjectsSortMode.None) as EnemyAI[];
                for(int i = 0; i < enemies.Length; i++)
                {
                    SkinnedMeshRenderer[] skins = enemies[i].skinnedMeshRenderers;
                    for(int j = 0; j < skins.Length; j++)
                    {
                        skins[j].material.shader = Shader.Find("Unlit/Texture");
                        skins[j].material.mainTexture = Texture2D.redTexture;
                        skins[j].material.renderQueue = (int)RenderQueue.Transparent;
                    }
                }
                nextEnemyScan = Time.time + 5f;
            }
        }
        public void OnGUI()
        {
            //ZatsRenderer.DrawString(new Vector2(100, 50), "found: " + (Shader.Find("Unlit/Texture") != null));
            if (shouldRender)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null) continue;
                    Vector3 loc = objects[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (fixedPos.z < 0) continue;
                    if (objects[i].isInShipRoom) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;

                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.gameplayCamera.transform.position, maxSize), Color.green);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.green, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), objects[i].itemProperties.itemName);
                }

                for(int i = 0; i < doors.Length; i++)
                {
                    if (doors[i] == null) continue;
                    Vector3 loc = doors[i].entrancePoint.position;
                    Vector3 offsetLoc = new Vector3(loc.x, loc.y + 1f, loc.z);
                    Vector3 fixedPos = FixedWorldToScreenPoint(offsetLoc);
                    if (fixedPos.z < 0) continue;
                    DrawCircle(new Vector2(fixedPos.x, fixedPos.y), 25f, Color.cyan);
                }

                for(int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null) continue;
                    Vector3 loc = enemies[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (fixedPos.z < 0) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.gameplayCamera.transform.position, maxSize), Color.red);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.red, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), enemies[i].enemyType.enemyName);
                }

                for(int i = 0; i < mines.Length; i++)
                {
                    if (mines[i] == null) continue;
                    Vector3 loc = mines[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (fixedPos.z < 0) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.gameplayCamera.transform.position, maxSize), Color.red);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.red, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), "Landmine");
                }

                for (int i = 0; i < turrets.Length; i++)
                {
                    if (turrets[i] == null) continue;
                    Vector3 loc = turrets[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (fixedPos.z < 0) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.gameplayCamera.transform.position, maxSize), Color.red);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.red, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), "Turret");
                }
                if (shouldRenderMini)
                {
                    Camera miniCam = cameras[miniCamIndex];
                    if(miniCam != null)
                    {
                        miniCam.Render();
                        if(miniCam.activeTexture != null)
                            GUI.DrawTexture(new Rect(Screen.width - miniCam.pixelWidth, 0, miniCam.pixelWidth, miniCam.pixelHeight), miniCam.activeTexture);
                    }
                }
                if(lever != null)
                {
                    Vector3 loc = lever.transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (!(fixedPos.z < 0) && checkScreenMargins(fixedPos, margin))
                    {
                        DrawTriangle(fixedPos, new Vector2(50, 50), Color.magenta);
                    }
                }
            }
        }

        private bool checkScreenMargins(Vector3 screenPosition, float margin)
        {
            if((screenPosition.x > Screen.width + margin) || screenPosition.x < -margin)
            {
                return false;
            }
            else if((screenPosition.y > Screen.height + margin) || screenPosition.y < -margin)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Vector3 FixedWorldToScreenPoint(Vector3 worldPosition)
        {
            Vector3 w2s = localPlayer.gameplayCamera.WorldToScreenPoint(worldPosition);
            Vector3 fixedPos = new Vector3(w2s.x * 2.232558140f, (localPlayer.gameplayCamera.pixelHeight - w2s.y) * 2.076923077f, w2s.z);
            return fixedPos;
        }

        private void Rescan()
        {
            objects = FindObjectsByType(typeof(GrabbableObject), FindObjectsSortMode.None) as GrabbableObject[];
            doors = FindObjectsByType(typeof(EntranceTeleport), FindObjectsSortMode.None) as EntranceTeleport[];
            enemies = FindObjectsByType(typeof(EnemyAI), FindObjectsSortMode.None) as EnemyAI[];
            mines = FindObjectsByType(typeof(Landmine), FindObjectsSortMode.None) as Landmine[];
            turrets = FindObjectsByType(typeof(Turret), FindObjectsSortMode.None) as Turret[];
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

        private void DrawCircle(Vector2 position, float radius, Color color)
        {
            Vector2[] points = new Vector2[18];
            int theta = 0;
            for(int i = 0; i < points.Length; i++)
            {
                points[i].x = radius * Mathf.Cos(theta * Mathf.Deg2Rad) + position.x;
                points[i].y = radius * Mathf.Sin(theta * Mathf.Deg2Rad) + position.y;
                theta += 20;
            }

            for(int i = 0; i < points.Length - 1; i++)
            {
                ZatsRenderer.DrawLine(points[i], points[i + 1], color, 1f);
            }
            ZatsRenderer.DrawLine(points[0], points[points.Length - 1], color, 1f);
        }

        private void DrawTriangle(Vector2 position, Vector2 size, Color color)
        {
            //  0
            // 1 2
            Vector2[] points = new Vector2[3];
            points[0].x = position.x;
            points[0].y = position.y - (size.y / 2f);
            points[1].x = position.x - (size.x / 2f);
            points[1].y = position.y + (size.y / 2f);
            points[2].x = position.x + (size.x / 2f);
            points[2].y = position.y + (size.y / 2f);
            ZatsRenderer.DrawLine(points[0], points[1], color, 1f);
            ZatsRenderer.DrawLine(points[2], points[1], color, 1f);
            ZatsRenderer.DrawLine(points[0], points[2], color, 1f);
        }

        private Vector2 CalculateSizeBasedOnDistance(Vector3 object0, Vector3 object1, Vector2 maxSize)
        {
            float manDist = Mathf.Abs(object0.x - object1.x) + Mathf.Abs(object0.y - object1.y) + Mathf.Abs(object0.z - object1.z);
            return new Vector2(maxSize.x / manDist, maxSize.y / manDist);
        }

        private Vector2 CalculateSizeBasedOnDistance(Vector3 object0, Vector3 object1, float maxSize)
        {
            float manDist = Mathf.Abs(object0.x - object1.x) + Mathf.Abs(object0.y - object1.y) + Mathf.Abs(object0.z - object1.z);
            return new Vector2(maxSize / manDist, maxSize / manDist);
        }
    }
}
