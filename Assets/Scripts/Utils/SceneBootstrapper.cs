using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place in every scene. Ensures singleton managers exist even when
/// a scene is opened directly during development.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    void Awake()
    {
        EnsureManager<CardsManager>("CardsManager");
        EnsureManager<AudioManager>("AudioManager");
        EnsureManager<CardDealAnimator>("CardDealAnimator");

        // Play scene music
        var audio = AudioManager.Instance;
        if (audio != null)
        {
            string scene = SceneManager.GetActiveScene().name;
            switch (scene)
            {
                case "MainMenu":     audio.PlayMainMenuMusic(); break;
                case "GameScene":    audio.PlayGameMusic();     break;
                case "GalleryScene": audio.PlayGalleryMusic();  break;
            }
        }
    }

    static void EnsureManager<T>(string goName) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>(true) != null) return;
        new GameObject(goName).AddComponent<T>();
        Debug.Log($"[Bootstrapper] Created {goName}");
    }
}
