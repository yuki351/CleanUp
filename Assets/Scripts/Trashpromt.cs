using UnityEngine;

public class TrashPrompt : MonoBehaviour
{
    public float showRange = 3f;
    private Transform player;
    private Camera mainCam;
    private bool playerNearby = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        mainCam = Camera.main;
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector3.Distance(
                         transform.position, player.position);
        playerNearby = distance <= showRange;
    }

    void OnGUI()
    {
        if (!playerNearby || mainCam == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(
                            transform.position + Vector3.up * 1.5f);
        if (screenPos.z < 0) return;

        float x = screenPos.x;
        float y = Screen.height - screenPos.y;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(x - 55, y - 20, 110, 35), "");

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        style.alignment = TextAnchor.MiddleCenter;

        GUI.color = Color.white;
        GUI.Label(new Rect(x - 55, y - 20, 110, 35), 
                  "Press E to Pick Up", style);
    }
}