using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text title, stats;

    void Awake() => panel.SetActive(false);
    void OnEnable() => GameManager.MatchFinished += Show;
    void OnDisable() => GameManager.MatchFinished -= Show;

    void Show(MatchResult r)
    {
        panel.SetActive(true);
        title.text = r.Title(GameManager.Instance.PlayerTeam);
        stats.text = r.Summary();
    }

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
