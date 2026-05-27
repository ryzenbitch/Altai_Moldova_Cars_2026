using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Labels")]
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI winnerNameText;
    public Image           winnerAvatarImage;
    public TextMeshProUGUI statsText;

    [Header("Buttons")]
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Fade")]
    public CanvasGroup canvasGroup;
    public float       fadeInDuration = 0.7f;

    [Header("FX")]
    public ParticleSystem confettiEffect;

    void Awake()
    {
        gameObject.SetActive(false);
        if (playAgainButton) playAgainButton.onClick.AddListener(() => {
            if (AudioManager.Instance) AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("GameScene");
        });
        if (mainMenuButton)  mainMenuButton.onClick.AddListener(() => {
            if (AudioManager.Instance) AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
        });
    }

    public void Show(PlayerData winner, PlayerData[] allPlayers,
                     int totalRounds, bool localPlayerWon)
    {
        gameObject.SetActive(true);
        if (canvasGroup) canvasGroup.alpha = 0f;
        StartCoroutine(FadeIn());

        if (headlineText)
            headlineText.text = localPlayerWon ? "🏆 ВЫ ПОБЕДИЛИ!" : $"Победитель:\n{winner.nickname}";

        if (winnerNameText)   winnerNameText.text   = winner.nickname;
        if (winnerAvatarImage && winner.avatar) winnerAvatarImage.sprite = winner.avatar;
        if (statsText)        statsText.text        = $"Сыграно раундов: {totalRounds}";

        if (confettiEffect)
        {
            if (localPlayerWon) confettiEffect.Play();
            else confettiEffect.Stop();
        }

        if (AudioManager.Instance)
        {
            if (localPlayerWon) AudioManager.Instance.PlayGameWin();
            else                AudioManager.Instance.PlayGameLose();
        }
    }

    IEnumerator FadeIn()
    {
        if (!canvasGroup) yield break;
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
