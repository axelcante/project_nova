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
    private bool _isExploding = false;
    private bool _isNova = false;

    [Header("Health & health bar")]
    [SerializeField] private HealthBar _HealthBar;
    [SerializeField] private float _maxStationHQHealth;
    private float _stationHQHealth;

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
        _stationHQHealth = _maxStationHQHealth;
        _HealthBar.gameObject.SetActive(true);
        _HealthBar.SetMaxHealth(_maxStationHQHealth);
        _HealthBar.UpdateHealth(_stationHQHealth);
        _HealthBar.SetName("Station");
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

    // SETTERS
    public void SetMaxStationHQHealth (float health) => _maxStationHQHealth = health;

    #endregion PUBLIC

    #region PRIVATE

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
