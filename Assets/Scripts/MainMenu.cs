using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region VARIABLES

    [Header("Animations")]
    [SerializeField] private Image _BlackScreen;
    [SerializeField] private float _fadeSpeed;
    [SerializeField] private Button[] _UIButtons;

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called before the first frame
    private void Start ()
    {
        StartCoroutine(FadeBlackScreen(false, _fadeSpeed));
    }

    #endregion UNITY

    #region PUBLIC

    // Load the game scene
    public void StartGame ()
    {
        StartCoroutine(FadeBlackScreen(true, _fadeSpeed));
    }

    #endregion PUBLIC

    #region PRIVATE

    // Fade to or from black screen
    public IEnumerator FadeBlackScreen (bool fadeIn, float speed)
    {
        // Fading in means player has clicked play button, in which case we much deactive all buttons while animation runs
        if (fadeIn) {
            foreach(Button button in _UIButtons) {
                button.interactable = false;
            }
        }

        float time = 0;
        Color currentColor = _BlackScreen.color;
        while (time < speed) {
            currentColor.a = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, time / speed);
            _BlackScreen.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = fadeIn ? 1 : 0;
        _BlackScreen.color = currentColor;

        // First launch of game, arriving in main menu buttons need to be set to interactable
        if (!fadeIn) {
            foreach (Button button in _UIButtons) {
                button.interactable = true;
            }
        } else {
            // Play Button pressed, faded to black, now must load scene!
            SceneManager.LoadScene(1);
        }
    }

    #endregion PRIVATE
    #endregion METHODS
}
