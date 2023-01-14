using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    #region SINGLETON DECLARATION

    // Shoulda probably called this MusicManager... consistency is key!
    private static MusicPlayer _instance;
    public static MusicPlayer GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    // I SHOULD NOT BE DOING THIS. I SHOULD NOT BE DOING THIS. I SHOULD NOT BE DOING THIS.
    // I need a way to pass a variable from one to the next, and this is the only script with isn't destroyed
    // I know I should create a new script, but time' running out!
    // Sorry in advance to whomever reads this code
    public bool _isTimelessMode = false;        // Marks if the game should be launched in Timeless mode or not

    // I do a bit of UI here because it persists between scenes and the MainMenu/Game UI are different
    [Header("Music icon UI")]
    [SerializeField] private Image _MusicButtonImage;           // Current image being displayed as the toggle music button
    [SerializeField] private TMP_Text _MusicIconText;           // "M" symbol for the music button
    [SerializeField] private Sprite _MusicOnSprite;             // Sprite when music is on
    [SerializeField] private Sprite _MusicOffSprite;            // Sprite when music is off
    [SerializeField] private Color _MusicOnColor;               // Color when music is on (for text)
    [SerializeField] private Color _MusicOffColor;              // Color when music is off (for text)
    [SerializeField] private Color _MusicHighlightedColor;      // Color when music icon is highlighted (for text)
    private Color _CurrentTextColor;                            // Tracks the current icon text color
    private bool _isHighlighted;                                // Tracks if button is currently highligted


    [Header("Audio")]
    [SerializeField] private AudioSource _AudioSource;      // A reference to the component playing the music
    [SerializeField] private AudioClip[] _Songs;            // A collection of songs to play on a loop
    [SerializeField] private float _musicFadeSpeed;         // Speed at which music is faded out when paused or stopped
    private int _currentSong = 0;                           // Holds the array id of the currently played song
    private bool _isPaused = false;                         // Tracks if music was manually paused or not
    private bool _isFading = false;                         // Tracks if the fading coroutine is currently in play (ha)
    private Coroutine FadeCoroutine;                        // Coroutine tracker

    // Called when this script is initialized
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            // This happens when you load back into the main menu, as we already have a music player
            Destroy(this);
        } else {
            _instance = this;
        }

        // We want this gameobject (and script) to persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // At the start of the game, music is on and icon shows this
        _CurrentTextColor = _MusicOnColor;

        PlayNextSong();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_AudioSource.isPlaying && !_isPaused)
            PlayNextSong();
    }

    // Plays next song when the previous one ends (or play manually set one)
    public void PlayNextSong (int id = -1)
    {
        if (id < 0) {
            _AudioSource.clip = _Songs[_currentSong];
            _AudioSource.Play();
            _currentSong += 1;

            // Reset to 0 if last song is played
            if (_currentSong == _Songs.Length - 1)
                _currentSong = 0;
        } else if (id <= _Songs.Length) {
            // If manually setting a new song, check if one is playing; if so, fade it out
            if (_AudioSource.isPlaying)
                StartCoroutine(FadeMusic(false, true));
            _currentSong = id;
            _AudioSource.clip = _Songs[id];
            _AudioSource.Play();
        } else {
            Debug.LogWarning("Trying to play a song that does not exist.");
        }
    }

    // Pauses or plays music on button press
    public void PauseMusic ()
    {
        if (_AudioSource.isPlaying) {
            _MusicButtonImage.sprite = _MusicOffSprite;
            _CurrentTextColor = _MusicOffColor;
            if(!_isHighlighted)
                _MusicIconText.color = _MusicOffColor;

            // Fade out music before stoping it
            if (_isFading && FadeCoroutine != null)
                StopCoroutine(FadeCoroutine);
            StartCoroutine(FadeMusic(false));
        } else {
            _MusicButtonImage.sprite = _MusicOnSprite;
            _CurrentTextColor = _MusicOnColor;
            if (!_isHighlighted)
                _MusicIconText.color = _MusicOnColor;

            // Fade in music after playing it
            if (_isFading && FadeCoroutine != null)
                StopCoroutine(FadeCoroutine);
            StartCoroutine(FadeMusic(true));
        }
    }

    // Fade music in or out when paused the music or manually stopping it (through script)
    private IEnumerator FadeMusic (bool fadeIn, bool toStop = false)
    {
        _isFading = true;
        float time = 0;
        
        // If we're fading in, play before it is faded in for the effect to work
        if (fadeIn) {
            _isPaused = false;
            _AudioSource.Play();
        }

        // The CLASSIC coroutine and LERPENING
        while (time < _musicFadeSpeed) {
            _AudioSource.volume = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, time / _musicFadeSpeed);
            time += Time.deltaTime;

            yield return null;
        }
        _AudioSource.volume = fadeIn ? 1 : 0;

        // Stop or Pause the music (PLEASE DON'T STOP THE MUUUUUUSIC)
        if (!fadeIn) {
            _isPaused = true;
            if (toStop) {
                _AudioSource.Stop();
                // Set volume back to one so we don't need to fade it back in when stop/starting
                _AudioSource.volume = 1;
            }
            else
                _AudioSource.Pause();
        }

        _isFading = false;
    }

    // When calling this instance to start the coroutine, use this to track the Coroutine object
    public void TrackedFadeMusic (bool fadeIn, bool toStop = false)
    {
        if (_isFading && FadeCoroutine != null)
            StopCoroutine(FadeCoroutine);

        FadeCoroutine = StartCoroutine(FadeMusic(fadeIn, toStop));
    }

    // Changes the music button text color on highlight
    public void ToggleIconColorOnHighlight (bool isEnter)
    {
        if (isEnter) {
            _isHighlighted = true;
            _MusicIconText.color = _MusicHighlightedColor;
        } else {
            _isHighlighted = false;
            _MusicIconText.color = _CurrentTextColor;
        }
    }
}
