using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Station : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static Station _instance;
    public static Station GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    public enum Element
    {
        LargeShield,
        SmallShield,
        StationHQ,
        DefenseOrb,
        Complexity,
        Pulsar,
    }

    [Header("StationELements")]
    [SerializeField] private Shield _LargeShield;       // Reference to the Large Shield defense
    [SerializeField] private Shield _SmallShield;       // Reference to the Small Shield defense

    [Header("Animations")]
    [SerializeField] private float _shieldFadeSpeed;    // Speed at which the shield fades when destroyed
    [SerializeField] private Animator _Animator;        // Reference to the station animator (for explode sequence)
    [SerializeField] private float _flashTiming;        // When the flash occurs (in station explode sequence)
    [SerializeField] private float _novaAnimationTime;  // Max time to animate explosion nova on game end
    
    private bool _isRepairing = false;  // Keeps track of if the station is currently in the repair coroutine  
    private bool _isExploding = false;  // Indicates that the explosion sequence has started
    private bool _isNova = false;       // Indicates that the nova sequence has started

    [Header("Health bar & repair")]
    [SerializeField] private HealthBar _HealthBar;      // Reference to the UI Health bar
    [SerializeField] private float _paidRepairAmount;   // Amount of health gained when purchasing a repair from the Shop
    [SerializeField] private float _repairPriceIncrease;// The repair price icreases after each use
    private float _stationHQHealth;                     // Current station health

    [Header("UI Shop Elements")]
    [SerializeField] private TMP_Text _PriceDisplay;    // Credit cost for upgrade
    [SerializeField] private TMP_Text _CreditsDisplay;  // "Credits" text to be disabled on max level
    [SerializeField] private TMP_Text _LevelDisplay;    // Current level UI display
    [SerializeField] private Button _BuyButton;         // Shop button to buy an upgrade
    [SerializeField] private Button _RepairButton;      // Shop button to repair station
    [SerializeField] private TMP_Text _RepPriceDisplay; // Credit cost for repairing

    // Properties (can be leveled up)
    private int _levelNb = -1;              // Tracks the current upgrade level for this station
    private bool _isMaxLevel = false;       // Checks if this station can still be upgraded
    private Upgrades.StationLevel _Level;   // Holds a reference to the current station level
    private float _currentMaxHealth;        // Maximum possible health
    private float _currentRepairAmount;     // Amount of health gained for each health tick
    private float _currentRepairSpeed;      // Time before each health tick
    private float _nextUpgradePrice;        // Amount of credits required to purchase next upgrade
    private float _repairPrice;             // Amount of credits required to repair the station

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // When Station is instantiated (scene load)
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of Station script running");
        }
        _instance = this;
    }

    // Before first frame
    private void Start ()
    {
        _HealthBar.gameObject.SetActive(true);

        // Only need to touch shop repair UI once
        InitializeShopRepairUI();
        
        // Increase level by one (station starts level 1)
        IncreaseLevel();
    }

    // Called once per frame
    private void Update ()
    {
        // Only heal during the WAVE phase (when enemies are closing in)
        if (!_isRepairing && GameManager.GetInstance().GetCurrentPhase() == GameManager.Phase.WAVE)
            StartCoroutine(RepairOverTime());
    }

    #endregion UNITY

    #region PUBLIC

    // Update station level (and disable UI element if max level)
    public void IncreaseLevel ()
    {
        if (!_isMaxLevel) {
            _levelNb++;

            // If we've reached max level, mark it so
            if (_levelNb == Upgrades.GetInstance()._StationLevels.Count - 1) {
                _isMaxLevel = true;
            }

            if (Upgrades.GetInstance()._StationLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._StationLevels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.LogWarning("Either there are no levels specified for this weapon, or current level is below 0");
            }
        } else {
            Debug.LogWarning("This station is already max level");
        }
    }

    // Increase current health (repair)
    public void RepairStation (float amount = -1)
    {
        float healAmount = 0;

        if (amount < 0) {
            // By default, when this function is called it heals for the paid amount
            healAmount = _paidRepairAmount;

            // In which case we also increase the price for subsequent repairs
            float prevPrice = _repairPrice;
            _repairPrice += _repairPriceIncrease;
            UpdateShopRepairUI(prevPrice, _repairPrice);
        } else {
            healAmount = amount;
        }

        if (_stationHQHealth + healAmount > _currentMaxHealth) {
            _stationHQHealth = _currentMaxHealth;
        } else {
            _stationHQHealth += healAmount;
        }

        // Update health UI
        _HealthBar.UpdateHealth(_stationHQHealth);

        // Disable repair button if at max health, or enable it on health lost
        if (_stationHQHealth >= _currentMaxHealth)
            _RepairButton.interactable = false;
        else
            _RepairButton.interactable = true;
    }

    // Return true if a specific Station Element (shields) is active or not
    public bool CheckElementState (Element el)
    {
        switch (el) {
            case Element.LargeShield:
                return _LargeShield.gameObject.activeSelf && _LargeShield.IsAlive();
            case Element.SmallShield:
                return _SmallShield.gameObject.activeSelf && _SmallShield.IsAlive();
            case Element.StationHQ:
                return gameObject.activeSelf;
            default:
                Debug.LogWarning("Outside of StationElements enum case!");
                return false;
        }
    }

    // Handles collisions with enemies
    public void HandleCollision (Element el, float damage)
    {
        switch (el) {
            case Element.LargeShield:
                // Loose health & disable
                _LargeShield.TakeDamange(damage);
                break;
            case Element.SmallShield:
                // Loose health & disable
                _SmallShield.TakeDamange(damage);
                break;
            case Element.StationHQ:
                TakeDamage(damage);
                break;
            default:
                Debug.LogWarning("Outside of StationElements enum case!");
                break;
        }
    }

    // Manual station detonation (when exiting game)
    public void ManualStationExplode () => StartCoroutine(StationExplode());

    // GETTERS
    public bool GetIsNova () => _isNova;
    public bool IsMaxLevel () => _isMaxLevel;
    public float GetUpgreadePrice () => _nextUpgradePrice;
    public float GetRepairPrice () => _repairPrice;

    #endregion PUBLIC

    #region PRIVATE

    // Update the repair price display (only done once on game start -- NOT OPTIMAL)
    private void InitializeShopRepairUI ()
    {
        float[] repPrices;

        if (Upgrades.GetInstance()._Prices.TryGetValue(Upgrades.Type.REPAIR, out repPrices)) {
            // There should only be one repair price!
            _repairPrice = repPrices[0];
            _RepPriceDisplay.text = _repairPrice.ToString();
        } else
            Debug.LogWarning("Couldn't find a repair price");
    }

    // Update the repair price display (only done once)
    private void UpdateShopRepairUI (float startPrice, float endPrice)
    {
        // Call UI to animate price update!
        StartCoroutine(UIManager.GetInstance().AnimateCredits(startPrice, endPrice, 0.3f, _RepPriceDisplay));
    }

    // Update the station properties based on current upgrade level
    private void SetLevelProperties (Upgrades.StationLevel level)
    {
        // Measure the difference in max health between the two levels if beyond level 0
        float healthDifference = _levelNb > 0 ? level._maxHealth - _currentMaxHealth : level._maxHealth;

        // Update properties
        _currentMaxHealth = level._maxHealth;
        _currentRepairAmount = level._repairAmount;
        _currentRepairSpeed = level._repairSpeed;

        // Increase the UI max health & heal for difference
        _HealthBar.SetMaxHealth(_currentMaxHealth);
        RepairStation(healthDifference);

        // Update Shop UI
        SetLevelAndPriceUI();
    }

    // Updates the Shop UI for this weapon's next level and price
    // SHOULD BE INHERITED AAAARGH WON'T HAVE TIME TO REFACTOR
    private void SetLevelAndPriceUI ()
    {
        if (!_isMaxLevel) {
            // Display the next level
            _LevelDisplay.text = (_levelNb + 1).ToString();

            // Update the price for the next upgrade
            float[] prices;
            if (Upgrades.GetInstance()._Prices.TryGetValue(Upgrades.Type.STATION, out prices)) {
                _nextUpgradePrice = prices[_levelNb + 1];
                _PriceDisplay.text = _nextUpgradePrice.ToString();
            } else
                Debug.LogWarning("Couldn't find a price for the given item's next upgrade");
        } else {
            // Max level; can't upgrade anymore
            _LevelDisplay.text = "Max";
            _BuyButton.interactable = false;
            StartCoroutine(UIManager.GetInstance().AnimateCredits(_nextUpgradePrice, 0, 0.3f, _PriceDisplay, true));
        }

    }

    // Regenerate health over time based on current repair speed
    private IEnumerator RepairOverTime ()
    {
        _isRepairing = true;

        RepairStation(_currentRepairAmount);
        yield return new WaitForSeconds(_currentRepairSpeed);

        _isRepairing = false;
    }

    // Calculate damage taken
    private void TakeDamage (float damage)
    {
        if (!_isExploding) {
            // Loose health
            _stationHQHealth -= damage;
            _HealthBar.UpdateHealth(_stationHQHealth);

            // DED? XPLODE
            if (_stationHQHealth <= 0) {
                _isExploding = true;
                StartCoroutine(StationExplode());
            }
        }
    }

    // NOW I AM BECOME DEATH, THE DESTROYER OF WORLDS
    private IEnumerator StationExplode ()
    {
        // Set health to 0 (if it isn't already)
        _HealthBar.UpdateHealth(0);

        // Tell the GameManager.cs to stop the play loop
        GameManager.GetInstance().SetGameOverState();

        // Hide shop UI if open
        StartCoroutine(UIManager.GetInstance().ToggleShop(true));

        // Stop any music currently playing for SILENCE AND ISOLATION EFFECT WOOWZERS
        if (MusicPlayer.GetInstance() && !MusicPlayer.GetInstance().GetIsPaused()) {
            MusicPlayer.GetInstance().TrackedFadeMusic(false, true);
        }
        MusicPlayer.GetInstance().DisableMusicButton();

        // Deactivate both shields and stop them from recharging (way too late for that now!)
        if (_LargeShield.gameObject.activeSelf)
            _LargeShield.StationDown();
        if (_SmallShield.gameObject.activeSelf)
            _SmallShield.StationDown();

        // Generate 5 impulses randomly seperated by a time interval. Could probably have done this differently?
        GameManager.GetInstance().GenerateImpulse();
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        GameManager.GetInstance().GenerateImpulse();
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        GameManager.GetInstance().GenerateImpulse();
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        GameManager.GetInstance().GenerateImpulse();
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        GameManager.GetInstance().GenerateImpulse();
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));

        // NOOOOOOOOOOVVVVVVVVVAAAAAAAAA
        _Animator.Play("Nova");
        yield return new WaitForSeconds(+_flashTiming);  // Length of the first part of the "Nova" animation
        StartCoroutine(UIManager.GetInstance().QuickFlash());
        _isNova = true;
        yield return new WaitForSeconds(_novaAnimationTime);
        StartCoroutine(GameManager.GetInstance().EndGame());
    }

    #endregion PRIVATE
    #endregion METHODS
}
