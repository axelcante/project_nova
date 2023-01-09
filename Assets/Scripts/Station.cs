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

    [Header("Station variables")]
    [SerializeField] private float _maxLargeShieldHealth;
    [SerializeField] private float _maxSmallShieldHealth;
    [SerializeField] private float _maxStationHQHealth;
    private float _stationHQHealth;
    [SerializeField] private float _ShieldCooldown;

    [Header("Animations")]
    [SerializeField] private float _shieldFadeSpeed;

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Return true if a specific Station Element (shields) is active or not
    public bool CheckElementState (StationElement el)
    {
        switch (el) {
            case StationElement.LargeShield:
                return _LargeShield.IsAlive();
            case StationElement.SmallShield:
                return _SmallShield.IsAlive();
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
                // Loose health & end game
                _stationHQHealth -= damage;
                if (_stationHQHealth < 0)
                    Debug.Log("GAME OVER");
                break;
            default:
                Debug.LogWarning("Outside of StationElements enum case!");
                break;
        }
    }

    // Recharges the shields' health based on current max health
    public void RechargedShieldHealth (Shield shield, StationElement el)
    {
        if (el == StationElement.LargeShield)
            shield.SetHealth(_maxLargeShieldHealth);
        else
            shield.SetHealth(_maxSmallShieldHealth);
    }

    // GETTERS
    public float GetMaxLargeShieldHealth() => _maxLargeShieldHealth;
    public float GetMaxSmallShieldHealth() => _maxSmallShieldHealth;
    public float GetShieldCooldown () => _ShieldCooldown;

    #endregion PUBLIC

    #region PRIVATE

    // When Station is instantiated (scene load)
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of Station script running");
        }
        _instance = this;
    }

    #endregion PRIVATE
    #endregion METHODS
}
