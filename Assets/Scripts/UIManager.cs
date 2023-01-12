using System.Collections;
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
    [Header("Flashes & Text")]
    [SerializeField] private Image _BlackScreen;            // A black image covering the whole screen
    [SerializeField] private TMP_Text _EndGameText;         // Text to display when game is ended
    [SerializeField] private float _textFadeSpeed;          // Speed at which the end game text fades in/out
    [SerializeField] private Color _WhiteStart;             // Black image color when flashing white
    [SerializeField] private Color _WhiteEnd;               // Black image color when flashing white
    [SerializeField] private float _flashSpeed;             // Speed at which the screen flashes white
    [Header("UI Items")]
    [SerializeField] private Transform _ShopUITransform;    // Position of the Shop UI
    [SerializeField] private Color _StartCreditsColor;      // Start color for the Credits text in the Shop UI
    [SerializeField] private Color _EndCreditsColor;        // End color for the Credits text in the Shop UI
    [SerializeField] private float _creditsTextFlashSpeed;  // Speed at which the credits text flashes red when unable to purchase upgrade
    [SerializeField] private float _shopAnimationSpeed;     // Speed at which the Shop moves in/out of view
    [SerializeField] private float _xPosHidden;             // X position to hide the Shop UI
    [SerializeField] private float _xPosDisplay;            // X position to display the Shop UI

    [Header("Shop Items")]
    [SerializeField] private TMP_Text _CreditAmountText;    // Shop UI element display current credits
    [SerializeField] private TMP_Text _CreditText;          // Shop UI element display "Credits"

    [Header("Time & wave")]
    [SerializeField] private TMP_Text _Minutes;             // Amount of minutes since start of game
    [SerializeField] private TMP_Text _Seconds;             // Amount of seconds since start of game
    [SerializeField] private TMP_Text _Miliseconds;         // Amount of miliseconds since start of game
    [SerializeField] private TMP_Text _WaveNumber;          // Number of the current wave

    private bool _isShopDisplayed = false;

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called on script load
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of UIManager script running");
        }
        _instance = this;
    }

    #endregion UNITY

    #region PUBLIC

    // Update current credits display
    public void UpdateCredits (float amount)
    {
        _CreditAmountText.text = amount.ToString();
    }

    // Update time display
    public void UpdateTime (int minutes, float seconds, float miliseconds)
    {
        _Minutes.text = minutes.ToString();
        _Seconds.text = seconds.ToString();
        _Miliseconds.text = miliseconds.ToString();
    }

    // Update wave number display
    public void UpdateWaveNumber (int number) => _WaveNumber.text = number.ToString();

    // This region contains all functions called by UI elements (such as buttons)
    #region CALLBACKS

    // Attempt to purchase an orb level (with an id to select the right orb in GameManager.cs)
    // Unfortunately, you can't assign custom enums in the Unity editor, so I need one function for each Station.Element
    public void AttemptPurchaseOrbLevel (int id)
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.DefenseOrb, id);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a complexity level (with an id to select the right orb in GameManager.cs)
    public void AttemptPurchaseComplexityLevel (int id)
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.Complexity, id);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a pulsar level (with an id to select the right orb in GameManager.cs)
    public void AttemptPurchasePulsarLevel (int id)
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.Pulsar, id);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a large shield level
    public void AttemptPurchaseLShieldLevel ()
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.LargeShield);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a small shield level
    public void AttemptPurchaseSShieldLevel ()
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.SmallShield);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a station level
    public void AttemptPurchaseStationLevel ()
    {
        bool success = GameManager.GetInstance().AttemptUpgrade(Station.Element.StationHQ);

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    // Attempt to purchase a station repair
    public void AttemptPurchaseStationRepair ()
    {
        bool success = GameManager.GetInstance().AttemptStationRepair();

        if (!success) {
            StartCoroutine(FlashCreditsText(_StartCreditsColor, _EndCreditsColor, _creditsTextFlashSpeed));
        }
    }

    #endregion CALLBACKS

    #region ANIMATIONS

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
    public IEnumerator FadeScreenBlack (bool fadeIn, float speed)
    {
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

    // Flash credits text red (when trying to purchase an item for which player does not have the credits for)
    public IEnumerator FlashCreditsText (Color start, Color end, float speed)
    {
        float time = 0;
        // Quick flash text red
        while (time < speed) {
            _CreditAmountText.color = Color.Lerp(start, end, time / (speed / 3));
            _CreditText.color = Color.Lerp(start, end, time / (speed / 3));

            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        // Quick flash text back to original color
        while (time < speed) {
            _CreditAmountText.color = Color.Lerp(end, start, time / speed);
            _CreditText.color = Color.Lerp(end, start, time / speed);

            time += Time.deltaTime;
            yield return null;
        }

        _CreditAmountText.color = start;
        _CreditText.color = start;
    }

    // Moves the UI to the left of the screen or back into play
    public IEnumerator ToggleShop ()
    {
        float time = 0;
        bool moveIn = !_isShopDisplayed;

        Vector2 startPos = new Vector2(moveIn ? _xPosHidden : _xPosDisplay, _ShopUITransform.position.y);
        Vector2 endPos = new Vector2(moveIn ? _xPosDisplay : _xPosHidden, _ShopUITransform.position.y);
        while (time < _shopAnimationSpeed) {
            _ShopUITransform.position = Vector2.Lerp(startPos, endPos, time / _shopAnimationSpeed);
            time += Time.deltaTime;
            yield return null;
        }
        _ShopUITransform.position = endPos;
        _isShopDisplayed = moveIn;
    }

    #endregion ANIMATIONS
    #endregion PUBLIC

    #region PRIVATE

    #endregion PRIVATE
    #endregion METHODS
}
