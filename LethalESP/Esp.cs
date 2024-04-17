using UnityEngine;
using UnityEngine.AI;
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
        int highlightedObjectIndex;
        bool shouldRender;
        bool shouldRenderMini;
        float nextEnemyScan;
        float nextCamUpdate;
        float nextUpdateHighlight;
        Texture lastCamTexture;
        NavMeshPath pathToObject;
        bool isValidPath;
        private readonly float margin = 100f;
        private readonly float maxSize = 500f;
        public void Start()
        {
            cameras = FindObjectsByType(typeof(Camera), FindObjectsSortMode.InstanceID) as Camera[];
            lever = FindObjectOfType(typeof(StartMatchLever)) as StartMatchLever;
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            Rescan();
            highlightedObjectIndex = 0;
            miniCamIndex = 1;
            shouldRender = true;
            shouldRenderMini = true;
            isValidPath = false;
            pathToObject = new NavMeshPath();
            nextEnemyScan = Time.time;
            nextCamUpdate = Time.time;
            nextUpdateHighlight = Time.time;
            RenderTexture playerTexture = localPlayer.gameplayCamera.activeTexture;
            lastCamTexture = new Texture2D(playerTexture.width, playerTexture.height, TextureFormat.RGBA32, 1, false);
            Graphics.CopyTexture(playerTexture, 0, 0, lastCamTexture, 0, 0);
        }
        public void Update()
        {
            if (Event.current.Equals(Event.KeyboardEvent("delete")))
            {
                if((highlightedObjectIndex >= 0 && highlightedObjectIndex < (objects.Length + doors.Length)) && ((highlightedObjectIndex < objects.Length && objects[highlightedObjectIndex] != null) || (highlightedObjectIndex >= objects.Length && (doors[highlightedObjectIndex - objects.Length] != null))))
                {
                    if(highlightedObjectIndex >= objects.Length)
                    {
                        Vector3 source = RoundManager.Instance.GetNavMeshPosition(localPlayer.transform.position, RoundManager.Instance.navHit, 2.7f, -1);
                        Vector3 destination = RoundManager.Instance.GetNavMeshPosition(doors[highlightedObjectIndex - objects.Length].entrancePoint.position, RoundManager.Instance.navHit, 2.7f, -1);
                        isValidPath = NavMesh.CalculatePath(source, destination, NavMesh.AllAreas, pathToObject);
                    }
                    else
                    {
                        Vector3 source = RoundManager.Instance.GetNavMeshPosition(localPlayer.transform.position, RoundManager.Instance.navHit, 2.7f, -1);
                        Vector3 destination = RoundManager.Instance.GetNavMeshPosition(objects[highlightedObjectIndex].transform.position, RoundManager.Instance.navHit, 2.7f, -1);
                        isValidPath = NavMesh.CalculatePath(source, destination, NavMesh.AllAreas, pathToObject);
                    }
                }
            }
            if (Event.current.Equals(Event.KeyboardEvent("home")))
            {
                Rescan();
            }
            if (Event.current.Equals(Event.KeyboardEvent("insert")))
            {
                shouldRender = !shouldRender;
                isValidPath = false;
            }
            if (Event.current.Equals(Event.KeyboardEvent("end")))
            {
                miniCamIndex++;
                if(miniCamIndex >= cameras.Length)
                {
                    miniCamIndex = 1;
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
            if (nextUpdateHighlight < Time.time)
            {
                int doorObjectLength = objects.Length + doors.Length;
                float[] distances = new float[doorObjectLength];
                int minIndex = 0;
                for(int i = 0; i < doorObjectLength; i++)
                {
                    if(i >= objects.Length)
                    {
                        int doorIndex = i - objects.Length;
                        Vector3 screenPosition = FixedWorldToScreenPoint(doors[doorIndex].entrancePoint.position);
                        distances[i] = Mathf.Abs(screenPosition.x - (Screen.width / 2)) + Mathf.Abs(screenPosition.y - (Screen.height / 2));
                        if (distances[i] < distances[minIndex])
                        {
                            minIndex = i;
                        }
                    }
                    else
                    {
                        Vector3 screenPosition = FixedWorldToScreenPoint(objects[i].transform.position);
                        distances[i] = Mathf.Abs(screenPosition.x - (Screen.width / 2)) + Mathf.Abs(screenPosition.y - (Screen.height / 2));
                        if (distances[i] < distances[minIndex])
                        {
                            minIndex = i;
                        }
                    }
                }
                highlightedObjectIndex = minIndex;
                nextUpdateHighlight = Time.time + 0.07f;
            }
        }
        public void OnGUI()
        {
            //ZatsRenderer.DrawString(new Vector2(100, 50), "found: " + (Shader.Find("Unlit/Texture") != null));
            //ZatsRenderer.DrawString(new Vector2(100, 50), localPlayer.gameplayCamera.pixelHeight + " x " + localPlayer.gameplayCamera.pixelWidth);
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
                    Color color = Color.green;
                    if (i == highlightedObjectIndex) color = Color.yellow;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.transform.position, maxSize), color);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), color, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), objects[i].itemProperties.itemName);
                }

                for(int i = 0; i < doors.Length; i++)
                {
                    if (doors[i] == null) continue;
                    Vector3 loc = doors[i].entrancePoint.position;
                    Vector3 offsetLoc = new Vector3(loc.x, loc.y + 1f, loc.z);
                    Vector3 fixedPos = FixedWorldToScreenPoint(offsetLoc);
                    if (fixedPos.z < 0) continue;
                    Color color = Color.cyan;
                    if (i == (highlightedObjectIndex - objects.Length)) color = Color.yellow;
                    DrawCircle(new Vector2(fixedPos.x, fixedPos.y), 25f, color);
                }

                for(int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null) continue;
                    Vector3 loc = enemies[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    // don't care about manticoils
                    string enemyName = enemies[i].enemyType.enemyName;
                    if (enemyName == "Manticoil" || enemyName == "Docile Locust Bees") continue;
                    if (fixedPos.z < 0) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.transform.position, maxSize), Color.red);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.red, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), enemyName);
                }

                for(int i = 0; i < mines.Length; i++)
                {
                    if (mines[i] == null) continue;
                    Vector3 loc = mines[i].transform.position;
                    Vector3 fixedPos = FixedWorldToScreenPoint(loc);
                    if (fixedPos.z < 0) continue;
                    if (!checkScreenMargins(fixedPos, margin)) continue;
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.transform.position, maxSize), Color.red);
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
                    DrawClearBox(new Vector2(fixedPos.x, fixedPos.y), CalculateSizeBasedOnDistance(loc, localPlayer.transform.position, maxSize), Color.red);
                    ZatsRenderer.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(fixedPos.x, fixedPos.y), Color.red, 1f);
                    ZatsRenderer.DrawString(new Vector2(fixedPos.x, fixedPos.y + 25), "Turret");
                }
                if (shouldRenderMini)
                {
                    Camera miniCam = cameras[miniCamIndex];
                    if(miniCam != null)
                    {
                        if(nextCamUpdate < Time.time)
                        {
                            miniCam.Render();
                            if (miniCam.activeTexture != null)
                            {
                                lastCamTexture = new Texture2D(miniCam.activeTexture.width, miniCam.activeTexture.height, TextureFormat.RGBA32, 1, false);
                                Graphics.CopyTexture(miniCam.activeTexture, 0, 0, lastCamTexture, 0, 0);
                            }
                            nextCamUpdate += 0.07f;
                        }
                        
                        
                        GUI.DrawTexture(new Rect(Screen.width - miniCam.pixelWidth, 0, miniCam.pixelWidth, miniCam.pixelHeight), lastCamTexture);
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
                if (isValidPath)
                {
                    for(int i = 0; i < pathToObject.corners.Length - 1; i++)
                    {
                        Vector3 start = FixedWorldToScreenPoint(pathToObject.corners[i]);
                        Vector3 end = FixedWorldToScreenPoint(pathToObject.corners[i + 1]);
                        if (start.z < 0 || end.z < 0) continue;
                        if (!(checkScreenMargins(start, margin) && checkScreenMargins(end, margin))) continue;
                        ZatsRenderer.DrawLine(start, end, Color.yellow, 2.0f);
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
            // player camera is 520 x 860 pixels
            Vector3 w2s = localPlayer.gameplayCamera.WorldToScreenPoint(worldPosition);
            float widthFactor = Mathf.Pow(10f, Mathf.Log10(Screen.width) - Mathf.Log10(localPlayer.gameplayCamera.pixelWidth));
            float heightFactor = Mathf.Pow(10f, Mathf.Log10(Screen.height) - Mathf.Log10(localPlayer.gameplayCamera.pixelHeight));
            Vector3 fixedPos = new Vector3(w2s.x * widthFactor, (localPlayer.gameplayCamera.pixelHeight - w2s.y) * heightFactor, w2s.z);
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
