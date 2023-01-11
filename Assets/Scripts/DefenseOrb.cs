using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseOrb : MonoBehaviour
{
    #region VARIABLES

    [Header("Visuals")]
    [SerializeField] private GameObject _LaserPrefab;   // Laser prefab gameobject containing a line renderer
    [SerializeField] private Transform _FirePoint;      // The 2D point where the laser will originate
    [SerializeField] private float _laserFadeSpeed;     // Time before which the laser dissipates
    [SerializeField] private float _delayBetweenShot;   // Time between shots of a same charge

    private bool _isFiring = false;     // Tracks if a fire coroutine is already running

    // Properties (updated by level)
    private int _levelNb = -1;                  // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel = false;           // Checks if this weapon can still be upgraded
    private Upgrades.DefenseOrbLevel _Level;    // Holds a reference to the current weapon level
    private float _currentRechargeSpeed;        // Time before laser can shoot again
    private int _currentLasersPerCharge;        // Number of lasers fired per charge

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called before the first frame when this object is set to active
    private void Start ()
    {
        // Increase the level by one (starts at -1) at the start of this object's lifetime
        IncreaseLevel();
    }

    // Called once per frame
    private void Update ()
    {
        if (!_isFiring)
            StartCoroutine(Fire());
    }

    #endregion UNITY

    #region PUBLIC

    // Update weapon level
    public void IncreaseLevel ()
    {
        if (!_isMaxLevel) {
            _levelNb++;

            // If we've reached max level, mark it so
            if (_levelNb == Upgrades.GetInstance()._OrbLevels.Count - 1)
                _isMaxLevel = true;

            if (Upgrades.GetInstance()._OrbLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._OrbLevels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.Log("Either there are no levels specified for this weapon, or current level is below 0");
            }
        }
    }

    // Deactivate this weapon
    public void StopFiring ()
    {
        // Stop currently running coroutines and any subsequent ones
        if (_isFiring)
            StopCoroutine(Fire());
        _isFiring = true;
    }

    // GETTERS
    public bool IsMaxLevel () => _isMaxLevel;

    #endregion PUBLIC

    #region PRIVATE

    // Update the weapon properties based on current upgrade level
    private void SetLevelProperties (Upgrades.DefenseOrbLevel level)
    {
        _currentRechargeSpeed = level._rechargeSpeed;
        _currentLasersPerCharge = level._lasersPerCharge;
    }

    // Fires at the first enemy sighted
    private IEnumerator Fire ()
    {
        _isFiring = true;

        // Do the fire loop for as many lasers as we can shoot before recharging
        for (int i = 0; i < _currentLasersPerCharge; i++) {
            // Get the oldest added enemy in the game to target
            Enemy enemy = GameManager.GetInstance().GetEnemy();

            if (enemy) {
                // Create a laser containing a line renderer
                LineRenderer laser = Instantiate(_LaserPrefab, _FirePoint.position, Quaternion.identity, transform)
                    .GetComponent<LineRenderer>();

                // Set the line renderer's vertices
                laser.SetPosition(0, _FirePoint.position);
                laser.SetPosition(1, enemy.transform.position);

                // Destroy the enemy GameObject that took the hit
                enemy.ManualExplode();

                // Fade out the laser before destroying it
                StartCoroutine(FadeOutLaser(laser));

                // Wait _delayBetweenShots seconds before firing the second laser (of the same charge)
                yield return new WaitForSeconds(_delayBetweenShot);
            }
        }
        
        // Wait before being able to call this method again
        yield return new WaitForSeconds(_currentRechargeSpeed);

        _isFiring = false;
    }

    // Fades out laser before destroying it
    private IEnumerator FadeOutLaser (LineRenderer laser)
    {
        Color current = laser.material.color;
        float time = 0;
        while (time < _laserFadeSpeed) {
            current.a = Mathf.Lerp(1, 0, time / _laserFadeSpeed);
            laser.material.color = current;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(laser.gameObject);
    }

    #endregion PRIVATE
    #endregion METHODS
}
