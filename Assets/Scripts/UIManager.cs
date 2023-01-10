using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static UIManager _instance;
    public static UIManager GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    [Header("Animations")]
    [SerializeField] private Image _BlackScreen;
    [SerializeField] private TMP_Text _EndGameText;
    [SerializeField] private Color _WhiteStart;
    [SerializeField] private Color _WhiteEnd;
    [SerializeField] private float _flashSpeed;
    [SerializeField] private float _textFadeSpeed;

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Flashes screen white and then fades it back to normal
    public IEnumerator QuickFlash ()
    {
        Color screenColorRef = _BlackScreen.color;

        yield return FlashScreenWhite(_WhiteStart, _WhiteEnd, _flashSpeed / 3);
        yield return FlashScreenWhite(_WhiteEnd, _WhiteStart, 2 * _flashSpeed / 3);

        _BlackScreen.color = screenColorRef;
    }

    // Flash screen white slightly
    public IEnumerator FlashScreenWhite (Color start, Color end, float speed)
    {
        float time = 0;
        // Quick flash of white
        while (time < speed) {
            _BlackScreen.color = Color.Lerp(start, end, time / speed);
            time += Time.deltaTime;
            yield return null;
        }
    }

    // Fade to black. Same logic as the Shield "Toggle" coroutine, but felt too convoluted to mix both together
    public IEnumerator FadeToBlack (float speed)
    {
        float time = 0;
        Color currentColor = _BlackScreen.color;
        while (time < speed) {
            currentColor.a = Mathf.Lerp(0, 1, time / speed);
            _BlackScreen.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = 1;
        _BlackScreen.color = currentColor;
    }

    // Fade in end game text (I should really do a common IEnumerator template for fades, but I'm too scared to mess everything up)
    public IEnumerator FadeEndText (bool fadeIn)
    {
        float time = 0;
        Color currentColor = _EndGameText.color;
        while (time < +_textFadeSpeed) {
            currentColor.a = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn? 1 : 0, time / _textFadeSpeed);
            _EndGameText.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = fadeIn ? 1 : 0;
        _EndGameText.color = currentColor;
    }

    #endregion

    #region PRIVATE

    // Called on script load
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of UIManager script running");
        }
        _instance = this;
    }

    #endregion
    #endregion
}
