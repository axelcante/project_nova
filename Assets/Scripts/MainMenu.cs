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

    [Header("UI Blocks")]
    [SerializeField] private GameObject _MissionBlock;
    [SerializeField] private GameObject _MentionsBlock;
    [SerializeField] private GameObject _ControlsBlock;
    [SerializeField] private GameObject _TimelessTooltip;

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called before the first frame
    private void Start ()
    {
        StartCoroutine(StartSequence());
    }

    #endregion UNITY

    #region PUBLIC

    // Load the game scene
    public void StartGame (bool isTimeless)
    {
        _TimelessTooltip.SetActive(false);
        MusicPlayer.GetInstance()._isTimelessMode = isTimeless;     // <-- THIS IS CODING HERESY
        StartCoroutine(FadeBlackScreen(true, _fadeSpeed));
    }

    // Toggle Mentions block
    public void ToggleMentionsBlock ()
    {
        _ControlsBlock.SetActive(false);
        _MissionBlock.SetActive(false);
        _MentionsBlock.SetActive(!_MentionsBlock.activeSelf);
    }

    // Toggle Mission block
    public void ToggleMissionBlock ()
    {
        _ControlsBlock.SetActive(false);
        _MissionBlock.SetActive(!_MissionBlock.activeSelf);
        _MentionsBlock.SetActive(false);
    }

    // Toggle Controls block
    public void ToggleControlsBlock ()
    {
        _ControlsBlock.SetActive(!_ControlsBlock.activeSelf);
        _MissionBlock.SetActive(false);
        _MentionsBlock.SetActive(false);
    }

    // Call MusicPlay to toggle music on or off
    public void ToggleMusic ()
    {
        MusicPlayer.GetInstance().PauseMusic();
    }

    // Toggle the timeless mode tooltip on hover
    public void ToggleTimelessTooltip (bool toggleOn)
    {
        if (_UIButtons[_UIButtons.Length - 1] != null && _UIButtons[_UIButtons.Length - 1].interactable)
            _TimelessTooltip.SetActive(toggleOn);
    }

    #endregion PUBLIC

    #region PRIVATE

    // Wait 1 second before the game starts (called from Start)
    public IEnumerator StartSequence ()
    {
        yield return new WaitForSeconds(1);
        yield return FadeBlackScreen(false, _fadeSpeed);
    }

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

        // Arriving in main menu -- buttons need to be set to interactable
        if (!fadeIn) {
            foreach (Button button in _UIButtons) {
                button.interactable = true;
            }
        } else {
            // Play Button pressed, faded to black, now must load scene!
            // But first, let's wait an extra second
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(1);
        }
    }

    #endregion PRIVATE
    #endregion METHODS
}
