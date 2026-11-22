using UnityEngine;

public class BowlingLauncher : MonoBehaviour
{
    public BowlingBall ballPrefab;
    public KeyCode key = KeyCode.F;
    public float cooldown = 4f;
    public int strikeThreshold = 3;

    public int Strikes { get; private set; }
    public int TotalHits { get; private set; }

    Health health;
    float nextTime;
    string message = "";
    float messageUntil;
    GUIStyle style;

    void Awake() => health = GetComponent<Health>();

    void Update()
    {
        if (!GameManager.IsPlaying || !Input.GetKeyDown(key)) return;

        if (Time.time < nextTime)
        {
            Show($"Ball: wait {nextTime - Time.time:0.0} s");
            return;
        }
        if (!TryGetAim(out Vector3 dir))
        {
            Show("Ball: aim at the ground");
            return;
        }

        nextTime = Time.time + cooldown;
        Vector3 pos = transform.position;
        pos.y = 0.5f;
        pos += dir * 1.2f;

        BowlingBall ball = Instantiate(ballPrefab, pos, Quaternion.identity);
        ball.Init(health, dir, OnBallFinished);
    }

    bool TryGetAim(out Vector3 dir)
    {
        dir = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null) return false;

        var ground = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!ground.Raycast(ray, out float distance)) return false;

        Vector3 flat = ray.GetPoint(distance) - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.01f) return false;

        dir = flat.normalized;
        return true;
    }

    void OnBallFinished(int hits)
    {
        TotalHits += hits;
        if (hits >= strikeThreshold)
        {
            Strikes++;
            Show($"STRIKE! x{hits}");
        }
        else if (hits > 0)
        {
            Show($"Hit x{hits}");
        }
        else
        {
            Show("Miss");
        }
    }

    void Show(string text)
    {
        message = text;
        messageUntil = Time.time + 2f;
    }

    void OnGUI()
    {
        if (style == null)
            style = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };

        float left = Mathf.Max(0f, nextTime - Time.time);
        string state = left > 0f ? $"Ball [F]: {left:0.0} s" : "Ball [F]: ready";
        GUI.Label(new Rect(20, Screen.height - 100, 400, 30), state, style);
        GUI.Label(new Rect(20, Screen.height - 70, 400, 30), $"Strikes: {Strikes}   Hits: {TotalHits}", style);

        if (Time.time < messageUntil)
            GUI.Label(new Rect(Screen.width / 2 - 150, 80, 300, 40), message, style);
    }
}
