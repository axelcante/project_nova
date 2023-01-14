using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static UIManager _instance;
    public static UIManager GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    [Header("Animations")]
    [Header("Flashes & Text")]
    [SerializeField] private Image _BlackScreen;            // A black image covering the whole screen, except final texts
    [SerializeField] private Image _StarScreenUI;           // A black stary image covering the whole screen, except final texts
    [SerializeField] private Image _StarScreenFinal;        // A black stary image covering the whole screen, even final texts
    [SerializeField] private TMP_Text _EndGameText;         // Text to display when game is ended
    [SerializeField] private float _textFadeSpeed;          // Speed at which the end game text fades in/out
    [SerializeField] private float _screenFadeSpeed;        // Speed at which the background/blackscreen fades in/out
    [SerializeField] private float _textWriteSpeed;         // Speed at which the "typewriter" effect aninmates
    [SerializeField] private Color _WhiteStart;             // Black image color when flashing white
    [SerializeField] private Color _WhiteEnd;               // Black image color when flashing white
    [SerializeField] private float _flashSpeed;             // Speed at which the screen flashes white
    [SerializeField] private float _creditsAnimationSpeed;  // Speed at which the credits going up or down are animated
    [Header("UI Items")]
    [SerializeField] private RectTransform _ShopUIPos;      // Position of the Shop UI as a rect transform
    [SerializeField] private Color _StartCreditsColor;      // Start color for the Credits text in the Shop UI
    [SerializeField] private Color _EndCreditsColor;        // End color for the Credits text in the Shop UI
    [SerializeField] private float _creditsTextFlashSpeed;  // Speed at which the credits text flashes red when unable to purchase upgrade
    [SerializeField] private float _shopAnimationSpeed;     // Speed at which the Shop moves in/out of view
    [SerializeField] private float _xPosHidden;             // X position to hide the Shop UI
    [SerializeField] private float _xPosDisplay;            // X position to display the Shop UI
    [Header("EndGameTexts")]
    // Coulda (shoulda) have one element for all texts, but too lazy to change color/font size all the time
    [SerializeField] private TMP_Text _TerminateTextTMP;    // Reference to the TerminateText UI text element
    [SerializeField] private TMP_Text _ResultTextTMP;       // Reference to the ResultText UI text element
    [SerializeField] private TMP_Text _FinalMessage1;       // Reference to the FinalMessage1 UI text element
    [SerializeField] private TMP_Text _FinalMessage2;       // Reference to the FinalMessage2 UI text element
    [SerializeField] private string _pnTermStory;           // "Project Nova Terminated."
    [SerializeField] private string _pnCredsStory;          // "X energy credits sent back to earth"
    [SerializeField] private string _pnEnd1Story;           // "I pray it is enough."
    [SerializeField] private string _pnEnd2Story;           // "Now... I can rest in infinity's cold embrace."
    [SerializeField] private float _timeAroundTypeEffect;   // Time durring which "|" flashes before and after typewriting animation
    [SerializeField] private int _numberOfCursors;          // Number of time "|" flashes before and after typewriting animation
    [SerializeField] private float _timeBeforeFadeOut;      // Time before a text fades out of view during the end game sequence

    private bool _isFading = false;             // Tracks if any Fading screen coroutines are running (to stop Escape menu)
    private bool _isUpdatingCreds = false;      // Tracks if the UI is currently animating amount of credits
    private Coroutine _CreditsCoroutine;        // Tracks the currently running UpdateCredits coroutine

    [Header("Shop Items & Esc Menu")]
    [SerializeField] private TMP_Text _CreditAmountText;    // Shop UI element display current credits
    [SerializeField] private TMP_Text _CreditText;          // Shop UI element display "Credits"
    [SerializeField] private GameObject _ReadyButton;       // Button to mark the end of the "inifinite" first shop phase
    [SerializeField] private GameObject[] _Tooltips;        // Collection of tooltip messages when hovering over buttons
    [SerializeField] private GameObject _EscapeMenu;        // A pannel displaying a button allowing to return to menu
    [SerializeField] private GameObject _WarningMessage;    // Warning tooltip when hovering the forfeit button

    [Header("Time & wave")]
    [SerializeField] private TMP_Text _Minutes;             // Amount of phase minutes since start of game
    [SerializeField] private TMP_Text _Seconds;             // Amount of phase seconds since start of game
    [SerializeField] private TMP_Text _Miliseconds;         // Amount of phase miliseconds since start of game
    [SerializeField] private TMP_Text _WaveDisplay;         // Number of the current wave

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

    // Called every frame
    private void Update ()
    {
        // Return to main menu if game is not already ending
        if (GameManager.GetInstance().GetCurrentPhase() != GameManager.Phase.DEAD &&
            Input.GetKeyUp(KeyCode.Escape) &&
            !_isFading
        )
            ToggleEscapeMenu();
    }

    #endregion UNITY

    #region PUBLIC

    // Update current credits display
    public void UpdateCredits (float current, float target)
    {
        //_CreditAmountText.text = amount.ToString();
        if (_isUpdatingCreds && _CreditsCoroutine != null) {
            StopCoroutine(_CreditsCoroutine);
            _CreditsCoroutine = StartCoroutine(AnimateCredits(current, target));
        } else {
            _CreditsCoroutine = StartCoroutine(AnimateCredits(current, target));
        }
    }

    // Update time display
    public void UpdateTime (string minutes, string seconds, string miliseconds)
    {
        _Minutes.text = minutes;
        _Seconds.text = seconds;
        _Miliseconds.text = miliseconds;
    }

    // Update wave number display
    public void UpdateWaveCounter (string wave) => _WaveDisplay.text = wave;

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

    // Declare ready from first shop phase and disable button
    public void DeclareReady () {
        _ReadyButton.SetActive(false);
        GameManager.GetInstance().DeclareReady();
    }

    // Store energy credits for Earth!
    public void StoreEnergy ()
    {
        GameManager.GetInstance().StoreCredits();
    }

    // Toggle corresponding tooltip on hover
    public void ToggleTooltip (int id = -1)
    {
        if (id < 0) {
            // Toggle all of the tooltips and set them to inactive
            foreach (GameObject tooltip in _Tooltips) {
                tooltip.SetActive(false);
            }
        } else if (id >= _Tooltips.Length) {
            Debug.LogWarning("Attempting to reach a tooltip that does not exist");
        } else {
            // If in timeless mode, don't toggle warning message on marking ready
            // id == 4 is that tooltip. Horrendous coding
            if (!(id == 4 && GameManager.GetInstance() && GameManager.GetInstance().GetIsTimeless()))
                _Tooltips[id].SetActive(!_Tooltips[id].activeSelf);
        }
    }

    // Toggle escape menu
    public void ToggleEscapeMenu ()
    {
        // Pause and play the game depending on if the escape menu is being open or closed
        if (_EscapeMenu.activeSelf)
            Time.timeScale = 1.0f;
        else
            Time.timeScale = 0f
                ;
        _WarningMessage.SetActive(false);
        _EscapeMenu.SetActive(!_EscapeMenu.activeSelf);
    }

    // Toggle escape menu warning message
    public void ToggleWarningMessage () => _WarningMessage.SetActive(!_WarningMessage.activeSelf);

    // End game => explode station and return to menu
    public void ForfeitGame ()
    {
        // Close escape menu and being end sequence
        ToggleEscapeMenu();
        Station.GetInstance().ManualStationExplode();
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
    public IEnumerator ToggleShop (bool forceHide = false)
    {
        if (!forceHide) {
            float time = 0;
            bool moveIn = !_isShopDisplayed;
            _isShopDisplayed = moveIn;
            

            Vector2 startPos = new Vector2(moveIn ? _xPosHidden : _xPosDisplay, _ShopUIPos.anchoredPosition.y);
            Vector2 endPos = new Vector2(moveIn ? _xPosDisplay : _xPosHidden, _ShopUIPos.anchoredPosition.y);
            while (time < _shopAnimationSpeed) {
                _ShopUIPos.anchoredPosition = Vector2.Lerp(startPos, endPos, time / _shopAnimationSpeed);
                time += Time.deltaTime;
                yield return null;
            }
            _ShopUIPos.anchoredPosition = endPos;

            // Reactivate the READY button if in timeless mode and moving out of view
            if (GameManager.GetInstance().GetIsTimeless() && !_isShopDisplayed)
                _ReadyButton.SetActive(true);
        } else {
            _ShopUIPos.gameObject.SetActive(false);
        }
    }

    // End game text animations before return to main menu
    public IEnumerator EndGameAnims ()
    {

        // Fade to star screen and start theme music
        yield return FadeScreen(true, _screenFadeSpeed, _BlackScreen);
        if (MusicPlayer.GetInstance())
            MusicPlayer.GetInstance().PlayNextSong(0);
        yield return new WaitForSeconds(_timeBeforeFadeOut);

        // Get current stored credits
        float sentCreds = GameManager.GetInstance().GetStoredCreds();

        // Add cursor flash effect at start and end of sentence
        // Type texts and fade them out, one after the other
        // First one --> PN TERMINATED
        yield return CursorFlash(_TerminateTextTMP);
        yield return TypewriterEffect(_pnTermStory, _TerminateTextTMP);

        // How many creds sent back to Earth (if any)
        if (sentCreds > 0) {
            // Add sent creds to result string (and animate it!)
            yield return AnimateCredits(0, sentCreds, 2f, _ResultTextTMP);

            // Add it to the story string
            string resultString = " " + _pnCredsStory;
            yield return TypewriterEffect(resultString, _ResultTextTMP);
            
            // Add cursor flash effect at end of sentence
            StartCoroutine(CursorFlash(_ResultTextTMP));
            StartCoroutine(FadeText(_ResultTextTMP, false));
        } else {
            StartCoroutine(CursorFlash(_TerminateTextTMP));
        }
        yield return FadeText(_TerminateTextTMP, false);

        if (sentCreds > 0) {
            // Add cursor flash effect at start and end of sentence
            yield return CursorFlash(_FinalMessage1);
            yield return TypewriterEffect(_pnEnd1Story, _FinalMessage1);
            StartCoroutine(CursorFlash(_FinalMessage1));
            yield return FadeText(_FinalMessage1, false);
        }
        // Add cursor flash effect at start and end of sentence
        yield return CursorFlash(_FinalMessage2);
        yield return TypewriterEffect(_pnEnd2Story, _FinalMessage2, true);
        StartCoroutine(CursorFlash(_FinalMessage2));
        yield return FadeText(_FinalMessage2, false);

        // Fade final star screen then return control to GameManager to go to main menu
        yield return FadeScreen(true, _screenFadeSpeed, _StarScreenFinal);
    }

    // Writes text character by character for typing effect
    public IEnumerator TypewriterEffect (string sentence, TMP_Text displayText, bool specialEffect = false)
    {
        // The special effect is to add a dramatic pause after the "..." of the last ending message
        // This is horrdendous coding, but it's the end of the project and I don't think I will be coming back to this
        // And if I do, I can easily find it and remove it thanks to these AWESOME COMMENTS
        displayText.text += '|';

        int i = 0;
        foreach (char c in sentence) {
            i++;
            if (specialEffect && i == 13) {
                // Remove '|'
                displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);
                yield return CursorFlash(displayText);
            }
            // Remove the last char in the string and replace it with char
            displayText.text = displayText.text.Substring(0, displayText.text.Length - 1) + c;
            displayText.text += '|';
            yield return new WaitForSeconds(_textWriteSpeed);
        }
        // Remove last '|' char
        displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);
    }

    // Flashes a '|' cursor at the end of a sentence when doing typewriting animation
    public IEnumerator CursorFlash (TMP_Text displayText)
    {
        // When done, animate a final "|" for _timeBeforeFadeOut time!
        float time = 0;
        float flashCounter = 0;
        char c = '|';
        displayText.text += c;
        while (time < _timeAroundTypeEffect) {
            if (flashCounter > (_timeAroundTypeEffect / _numberOfCursors)) {
                c = c == '|' ? ' ' : '|';
                displayText.text = displayText.text.Substring(0, displayText.text.Length - 1) + c;
                flashCounter = 0;
            }

            flashCounter += Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }
        displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);
    }

    // Fade in end game text (I should really do a common IEnumerator template for fades, but I'm too scared to mess everything up)
    // Might not be used in favor of typing effect
    public IEnumerator FadeText (TMP_Text textEl, bool fadeIn)
    {
        float time = 0;
        Color currentColor = textEl.color;
        while (time < +_textFadeSpeed) {
            currentColor.a = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, time / _textFadeSpeed);
            textEl.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = fadeIn ? 1 : 0;
        textEl.color = currentColor;
    }

    // Fade to black. Same logic as the Shield "Toggle" coroutine, but felt too convoluted to mix both together
    public IEnumerator FadeScreen (bool fadeIn, float speed, Image screen = null)
    {
        _isFading = true;

        // By default, fade the StarScreenUI
        if (screen == null)
            screen = _StarScreenUI;

        float time = 0;
        Color currentColor = screen.color;
        while (time < speed) {
            currentColor.a = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, time / speed);
            screen.color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = fadeIn ? 1 : 0;
        screen.color = currentColor;

        _isFading = false;
    }

    // Animate credits being updated (when storing or end game screen)
    public IEnumerator AnimateCredits (float curCreds, float tarCreds, float multiplier = 1, TMP_Text txt = null)
    {
        if (!txt)
            txt = _CreditAmountText;

        float time = 0;
        float duration = _creditsAnimationSpeed * multiplier;
        while (time < duration) {
            int creds = Mathf.RoundToInt(Mathf.Lerp(curCreds, tarCreds, time / duration));
            txt.text = creds.ToString();
            
            time += Time.deltaTime;
            yield return null;
        }
        txt.text = tarCreds.ToString();
    }

    #endregion ANIMATIONS
    #endregion PUBLIC

    #region PRIVATE

    #endregion PRIVATE
    #endregion METHODS
}
