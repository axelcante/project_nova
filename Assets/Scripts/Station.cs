using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Station : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static Station _instance;
    public static Station GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    public enum StationElement
    {
        LargeShield,
        SmallShield,
        StationHQ
    }

    [Header("StationELements")]
    [SerializeField] private Shield _LargeShield;
    [SerializeField] private Shield _SmallShield;

    [Header("Animations")]
    [SerializeField] private float _shieldFadeSpeed;
    [SerializeField] private Animator _Animator;
    [SerializeField] private float _flashTiming;
    [SerializeField] private float _novaAnimationTime;
    private bool _isRepairing = false;
    private bool _isExploding = false;
    private bool _isNova = false;

    [Header("Health bar")]
    [SerializeField] private HealthBar _HealthBar;
    private float _stationHQHealth;

    // Properties (can be leveled up)
    private int _levelNb = -1;              // Tracks the current upgrade level for this station
    private bool _isMaxLevel = false;       // Checks if this station can still be upgraded
    private Upgrades.StationLevel _Level;   // Holds a reference to the current station level
    private float _currentMaxHealth;        // Maximum possible health
    private float _currentRepairAmount;     // Amount of health gained for each health tick
    private float _currentRepairSpeed;      // Time before each health tick

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
        IncreaseLevel();
    }

    // Called once per frame
    private void Update ()
    {
        if (!_isRepairing)
            StartCoroutine(RepairOverTime());
    }

    //// Update is called once per frame
    //private void Update ()
    //{
    //    // DEBUG - REMOVE
    //    if (Input.GetKeyDown(KeyCode.F)) {
    //        StartCoroutine(StationExplode());
    //    }
    //}

    #endregion UNITY

    #region PUBLIC

    // Update station level
    public void IncreaseLevel ()
    {
        if (!_isMaxLevel) {
            _levelNb++;

            // If we've reached max level, mark it so
            if (_levelNb == Upgrades.GetInstance()._StationLevels.Count - 1)
                _isMaxLevel = true;

            if (Upgrades.GetInstance()._StationLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._StationLevels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.Log("Either there are no levels specified for this weapon, or current level is below 0");
            }
        }
    }

    // Return true if a specific Station Element (shields) is active or not
    public bool CheckElementState (StationElement el)
    {
        switch (el) {
            case StationElement.LargeShield:
                return _LargeShield.gameObject.activeSelf && _LargeShield.IsAlive();
            case StationElement.SmallShield:
                return _SmallShield.gameObject.activeSelf && _SmallShield.IsAlive();
            case StationElement.StationHQ:
                return gameObject.activeSelf;
            default:
                Debug.LogWarning("Outside of StationElements enum case!");
                return false;
        }
    }

    // Handles collisions with enemies
    public void HandleCollision (StationElement el, float damage)
    {
        switch (el) {
            case StationElement.LargeShield:
                // Loose health & disable
                _LargeShield.TakeDamange(damage);
                break;
            case StationElement.SmallShield:
                // Loose health & disable
                _SmallShield.TakeDamange(damage);
                break;
            case StationElement.StationHQ:
                if (!_isExploding) {
                    // Loose health & end game
                    _stationHQHealth -= damage;
                    _HealthBar.UpdateHealth(_stationHQHealth);
                    if (_stationHQHealth <= 0) {
                        _isExploding = true;
                        StartCoroutine(StationExplode());
                    }
                }
                break;
            default:
                Debug.LogWarning("Outside of StationElements enum case!");
                break;
        }
    }

    // GETTERS
    public bool GetIsNova () => _isNova;

    #endregion PUBLIC

    #region PRIVATE

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
    }

    // Increase current health
    private void RepairStation (float amount)
    {
        if (_stationHQHealth + amount > _currentMaxHealth)
            _stationHQHealth = _currentMaxHealth;
        else
            _stationHQHealth += amount;

        // Update health UI
        _HealthBar.UpdateHealth(_stationHQHealth);
    }

    // Regenerate health over time based on current repair speed
    private IEnumerator RepairOverTime ()
    {
        _isRepairing = true;

        RepairStation(_currentRepairAmount);
        yield return new WaitForSeconds(_currentRepairSpeed);

        _isRepairing = false;
    }

    // NOW I AM BECOME DEATH, THE DESTROYER OF WORLDS
    private IEnumerator StationExplode ()
    {
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
