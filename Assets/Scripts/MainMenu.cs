using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region VARIABLES

    [Header("Animations")]
    [SerializeField] private Image _BlackScreen;
    [SerializeField] private float _fadeSpeed;
    private bool _isFading = false;

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Load the game scene
    public void StartGame ()
    {
        if (_isFading) {

        }
        SceneManager.LoadScene(1);
    }

    #endregion PUBLIC

    #region PRIVATE

    // Called before the first frame
    private void Start ()
    {
        StartCoroutine(FadeToBlack(false));
    }

    // Fade to or from black. Same logic as the Shield "Toggle" coroutine, but felt too convoluted to mix both together
    public IEnumerator FadeToBlack (bool fadeIn)
    {
        _isFading = true;

        float time = 0;
        Color currentColor = _BlackScreen.color;
        float currentAlpha = currentColor.a;
        while (time < _fadeSpeed) {
            currentColor.a = Mathf.Lerp(currentAlpha, fadeIn ? 1 : 0, time / _fadeSpeed);
            _BlackScreen.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = fadeIn ? 1 : 0;
        _BlackScreen.color = currentColor;

        _isFading = false;
    }

    #endregion PRIVATE
    #endregion METHODS
}
