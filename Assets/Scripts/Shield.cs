using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shield : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private Station.Element _stationElement; // Identifier for this station element

    [Header("Animations")]
    [SerializeField] private SpriteRenderer _Sprite;    // A reference to the sprite displaying this shield
    [SerializeField] private float _fadeSpeed;          // The speed at which the shield alpha fades (in or out)
    [SerializeField] private float _colorSwapSpeed;     // The speed at which the shield swaps from two colors
    [SerializeField] private float _hitSpeed;           // The speed at which the shield animates being hit
    [SerializeField] Color _ShieldAliveColor;           // Color when the shield is alive and fully charged
    [SerializeField] Color _ShieldDeadColor;            // Color when the shield is dying
    [SerializeField] float _shieldDeadAlpha;            // Alpha when the shield is dying
    [SerializeField] Color _ShieldHitColor;             // Color when the shield is hit

    private Coroutine _FlashOnHit = null;   // Coroutine tracking
    private Coroutine _Recharge = null;     // Coroutine tracking

    [Header("Health bar")]
    [SerializeField] private HealthBar _HealthBar;      // Reference to the health bar UI element

    [Header("UI Shop Elements")]
    [SerializeField] private TMP_Text _PriceDisplay;    // Credit cost for upgrade
    [SerializeField] private TMP_Text _CreditsDisplay;  // "Credits" text to be disabled on max level
    [SerializeField] private TMP_Text _LevelDisplay;    // Current level UI display
    [SerializeField] private Button _BuyButton;         // Shop button to buy an upgrade

    private bool _isAlive;          // Boolean used by other scripts to determine if this shield is active
    private float _currentHealth;   // Current shield health

    // Properties (can be leveled up)
    private int _levelNb = -1;              // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel;               // Tracks if this shield can still be upgraded
    private Upgrades.ShieldLevel _Level;    // Holds a reference to the current shield level
    private float _currentMaxHealth;        // Maximum possible health
    private float _currentShieldCooldown;   // Time to recharge a shield to full health
    private float _nextUpgradePrice;        // Amount of credits required to purchase next upgrade

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called when the shield is set to active (when the player unlocks it)
    private void Start ()
    {
        _isAlive = false;

        // Initialize the Shop UI prices for this shield
        SetLevelAndPriceUI();
    }

    #endregion UNITY

    #region PUBLIC

    // Update shield level
    public void IncreaseLevel ()
    {
        // Depending on this shield's type (large or small), get the corresponding list of levels from the Upgrades script
        bool lshield = _stationElement == Station.Element.LargeShield;
        List<Upgrades.ShieldLevel> levels = lshield ? Upgrades.GetInstance()._LShieldLevels :
            Upgrades.GetInstance()._SShieldLevels;

        // Only increase level if max level has not already been reached
        if (!_isMaxLevel) {
            _levelNb++;

            _isAlive = true;

            // Enable the sprite renderer
            _Sprite.enabled = true;

            // If we've reached max level, mark it so
            if (_levelNb == levels.Count - 1)
                _isMaxLevel = true;

            // Get the corresponding level
            if (levels.Count > 0 && _levelNb >= 0) {
                _Level = levels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.Log("Either there are no levels specified for this weapon, or current level is below 0");
            }
        } else {
            Debug.LogWarning("This shield is already max level!");
        }
    }

    // Reduce current shield health by x amount
    public void TakeDamange (float damage)
    {
        _currentHealth -= damage;
        _HealthBar.UpdateHealth(_currentHealth);

        // Shield takes damage
        if (_currentHealth > 0) {
            // Coroutine management
            if (_FlashOnHit == null) {
                _FlashOnHit = StartCoroutine(FlashOnHit());
            } else {
                StopCoroutine(_FlashOnHit);
                _FlashOnHit = StartCoroutine(FlashOnHit());
            }
        } else {
            // Shield deactivates
            _isAlive = false;
            if (_FlashOnHit != null) {
                StopCoroutine(_FlashOnHit);
                _FlashOnHit = null;
            }

            _Recharge = StartCoroutine(Recharge());
        }
    }

    // Station down, so disable all shields and stop them from recharging
    public void StationDown ()
    {
        // Stop coroutines if they are running
        if (_Recharge != null)
            StopCoroutine(_Recharge);
        if (_FlashOnHit != null)
            StopCoroutine(_FlashOnHit);

        StartCoroutine(Toggle(false));
    }

    // GETTERS
    public bool IsMaxLevel () => _isMaxLevel;
    public bool IsAlive () => _isAlive;
    public float GetUpgreadePrice () => _nextUpgradePrice;

    #endregion PUBLIC

    #region PRIVATE

    // Update the shield properties based on current upgrade level
    private void SetLevelProperties (Upgrades.ShieldLevel level)
    {
        _currentMaxHealth = level._maxHealth;
        _currentShieldCooldown = level._cooldown;

        // Also increase the health bar max display accordingly
        if (!_HealthBar.gameObject.activeSelf)
            _HealthBar.gameObject.SetActive(true);

        // Leveling up cause shield to recharge to full! Hurray!
        _currentHealth = _currentMaxHealth;
        _HealthBar.SetMaxHealth(_currentMaxHealth);
        _HealthBar.UpdateHealth(_currentHealth);

        // Update the Shop UI
        SetLevelAndPriceUI();
    }

    // Updates the Shop UI for this weapon's next level and price
    // SHOULD BE INHERITED AAAARGH WON'T HAVE TIME TO REFACTOR
    private void SetLevelAndPriceUI ()
    {
        // Determine shield type to get the corresponding upgrade levels
        Upgrades.Type shieldType = _stationElement == Station.Element.LargeShield ?
            Upgrades.Type.L_SHIELD :
            Upgrades.Type.S_SHIELD;

        if (!_isMaxLevel) {
            // Display the next level
            _LevelDisplay.text = (_levelNb + 1).ToString();

            // Update the price for the next upgrade
            float[] prices;
            if (Upgrades.GetInstance()._Prices.TryGetValue(shieldType, out prices)) {
                _nextUpgradePrice = prices[_levelNb + 1];
                _PriceDisplay.text = _nextUpgradePrice.ToString();
            } else
                Debug.LogWarning("Couldn't find a price for the given item's next upgrade");
        } else {
            // Max level; can't upgrade anymore
            _LevelDisplay.text = "Max";
            _BuyButton.interactable = false;
            _PriceDisplay.gameObject.SetActive(false);
            _CreditsDisplay.gameObject.SetActive(false);
        }

    }

    // When shields run out of health, call this coroutine to set a timer as they recharge
    private IEnumerator Recharge ()
    {
        // 1. Deactivate shield, then fade it out
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldDeadColor, _colorSwapSpeed);
        yield return Toggle(false);
        // 2. Wait for shield to recharge
        yield return new WaitForSeconds(_currentShieldCooldown);
        // 3. When shield is recharged, fade it in, then deactivate it
        yield return Toggle(true);
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldAliveColor, _colorSwapSpeed);
        // 4. Recharge this shield's health
        _currentHealth = _currentMaxHealth;
        _HealthBar.UpdateHealth(_currentHealth);
        _isAlive = true;
    }

    // Call this coroutine to "flash" the shield color (simulating hit)
    private IEnumerator FlashOnHit ()
    {
        Color currentColor = GetComponent<SpriteRenderer>().color;
        yield return TransitionColors(currentColor, _ShieldHitColor, _hitSpeed / 3);
        yield return TransitionColors(_ShieldHitColor, _ShieldAliveColor, _hitSpeed);
        _FlashOnHit = null;
    }

    // Call this coroutine to switch between shield dead and shield alive colors
    private IEnumerator TransitionColors (Color start, Color end, float speed)
    {
        float time = 0;
        while (time < speed) {
            GetComponent<SpriteRenderer>().color = Color.Lerp(start, end, time / speed);
            time += Time.deltaTime;
            yield return null;
        }
        GetComponent<SpriteRenderer>().color = end;
    }

    // Call this coroutine to either fade in or fade out a shield (when it is activated or destroyed)
    private IEnumerator Toggle (bool toActivate)
    {
        float time = 0;
        Color currentColor = GetComponent<SpriteRenderer>().color;
        float currentAlpha = currentColor.a;
        while (time < _fadeSpeed) {
            currentColor.a = Mathf.Lerp(currentAlpha, toActivate ? _shieldDeadAlpha / 255 : 0, time / _fadeSpeed);
            GetComponent<SpriteRenderer>().color = currentColor;
            time += Time.deltaTime;
            yield return null;
        }
        currentColor.a = toActivate ? _shieldDeadAlpha / 255 : 0;
        GetComponent<SpriteRenderer>().color = currentColor;
    }

    #endregion PRIVATE
    #endregion METHODS
}
