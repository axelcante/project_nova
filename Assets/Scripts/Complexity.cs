using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Complexity : MonoBehaviour
{
    #region VARIABLES

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _Sprite;        // Visual element of this weapon
    [SerializeField] private float _spriteFadeInSpeed;      // Speed at which the sprite first fades into view when purchased
    [SerializeField] private GameObject _LaserBeamPrefab;   // Reference to the laser beam prefab to be instantiated
    [SerializeField] private float _laserFadeInSpeed;       // Animation time to fade in laser beam
    [SerializeField] private float _laserFadeOutSpeed;      // Animation time to fade out laser beam
    [SerializeField] private Transform _firePoint;          // Point at which the laser spawns
    [SerializeField] private Transform _fireTarget;         // Point to which the laser goes

    [Header("Raycasting")]
    [SerializeField] private LayerMask _EnemyLayer;     // Complexity weapon fires in a line (raycast) hitting only enemies

    [Header("UI Shop Elements")]
    [SerializeField] private TMP_Text _PriceDisplay;    // Credit cost for upgrade
    [SerializeField] private TMP_Text _CreditsDisplay;  // "Credits" text to be disabled on max level
    [SerializeField] private TMP_Text _LevelDisplay;    // Current level UI display
    [SerializeField] private Button _BuyButton;         // Shop button to buy an upgrade

    private bool _isActive = false;         // By default, a weapon is inactive at start of the game
    private bool _isFiring = false;         // Tracks if a fire coroutine is already running
    private bool _raycast = false;          // Tells the UpdateFixed method to raycast
    private bool _isEnemyPhase = false;     // Tracks if weapon systems should be online (enemies present)

    // Properties (updated by level)
    private int _levelNb = -1;                  // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel = false;           // Checks if this weapon can still be upgraded
    private Upgrades.ComplexityLevel _Level;    // Holds a reference to the current weapon level
    private float _currentRechargeSpeed;        // Time before laser can shoot again
    private float _currentLaserDuration;        // Time to fire laser before it dissipates
    private float _nextUpgradePrice = 0;        // Amount of credits required to purchase next upgrade

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called just before the first frame when this script is instantiated
    private void Start ()
    {
        // Initialize the Shop UI
        SetLevelAndPriceUI();
    }

    // Called once per frame, manages the auto shoot coroutine
    private void Update ()
    {
        // Only fire when enemies are being spawned (not while in shop)
        if (_isActive && GameManager.GetInstance().GetCurrentPhase() == GameManager.Phase.WAVE) {
            if (!_isFiring)
                StartCoroutine(Fire());
        }
    }

    // Called a regular intervals no matter frames displayed (for physics calculations)
    private void FixedUpdate ()
    {
        // Detect ray hits when laser beam is firing
        if (_raycast)
            CastRay();
    }

    #endregion UNITY

    #region PUBLIC

    // Update weapon level
    public void IncreaseLevel ()
    {
        if (!_isMaxLevel) {
            _isActive = true;

            // Show the weapon (if it is not already shown)
            if (!_Sprite.enabled) {
                _Sprite.enabled = true;
                StartCoroutine(FadeSpriteIn(_spriteFadeInSpeed));
            }
            
            _levelNb++;

            // If we've reached max level, mark it so
            if (_levelNb == Upgrades.GetInstance()._ComplexityLevels.Count - 1)
                _isMaxLevel = true;

            if (Upgrades.GetInstance()._ComplexityLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._ComplexityLevels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.LogWarning("Either there are no levels specified for this weapon, or current level is below 0");
            }
        } else {
            Debug.LogWarning("This weapon is already max level!");
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
    public bool IsEnemyPhase () => _isEnemyPhase;
    public float GetUpgreadePrice () => _nextUpgradePrice;

    // SETTERS
    public void SetIsEnemyPhase (bool isEnemyPhase) => _isEnemyPhase = isEnemyPhase;

    #endregion PUBLIC

    #region PRIVATE

    // Update the weapon properties based on current upgrade level
    private void SetLevelProperties (Upgrades.ComplexityLevel level)
    {
        _currentRechargeSpeed = level._rechargeSpeed;
        _currentLaserDuration = level._laserBeamDuration;

        // Upgrade Shop UI
        SetLevelAndPriceUI();
    }

    // Updates the Shop UI for this weapon's next level and price
    // Can be called on Start to initialize shop prices for level 0
    private void SetLevelAndPriceUI ()
    {
        if (!_isMaxLevel) {
            // Display the next level
            _LevelDisplay.text = (_levelNb + 1).ToString();

            // Update the price for the next upgrade
            float[] prices;
            if (Upgrades.GetInstance()._Prices.TryGetValue(Upgrades.Type.COMPLEXITY, out prices)) {
                // Animate credits going up
                float currentPrice = _nextUpgradePrice;
                _nextUpgradePrice = prices[_levelNb + 1];
                StartCoroutine(UIManager.GetInstance().AnimateCredits(currentPrice, _nextUpgradePrice, 0.3f, _PriceDisplay));
            } else
                Debug.LogWarning("Couldn't find a price for the given item's next upgrade");
        } else {
            // Max level; can't upgrade anymore
            _LevelDisplay.text = "Max";
            _BuyButton.interactable = false;
            StartCoroutine(UIManager.GetInstance().AnimateCredits(_nextUpgradePrice, 0, 0.3f, _PriceDisplay, true));
        }
    }

    // Cast rays to destroy enemies in sight
    private void CastRay ()
    {
        // Calculate direction and distance between _firePoint and _fireTarget
        Vector3 direction = _fireTarget.position - _firePoint.position;
        direction.Normalize();
        float distance = Vector3.Distance(_firePoint.position, _fireTarget.position);

        // Cast a ray from the fire point to the target point
        RaycastHit2D[] hits = Physics2D.RaycastAll(_firePoint.position, direction, distance, _EnemyLayer);

        // Go through each target hit by the ray and destroy them
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider && hit.collider.GetComponent<Enemy>())
                hit.collider.GetComponent<Enemy>().ManualExplode();
        }

        // Debug
        //Debug.DrawRay(_firePoint.position, direction * distance, Color.red);
    }

    // Automatically shoot laser every few seconds, the laser lasts a few seconds
    private IEnumerator Fire ()
    {
        _isFiring = true;

        // Create a laser containing a line renderer
        LineRenderer laser = Instantiate(_LaserBeamPrefab, _firePoint.position, Quaternion.identity, _firePoint)
            .GetComponent<LineRenderer>();

        // Fade laser in!
        yield return FadeLaser(laser, true, _laserFadeInSpeed);

        float time = 0;
        while (time < _currentLaserDuration) {
            // Set the line renderer's vertices
            laser.SetPosition(0, _firePoint.position);
            laser.SetPosition(1, _fireTarget.position);

            // Also detect hit enemies and destroy them!
            _raycast = true;

            time += Time.deltaTime;
            yield return null;
        }

        // Stop detecting ray collisions
        _raycast = false;

        // Also fade out the laser before destroying it
        StartCoroutine(FadeLaser(laser, false, _laserFadeOutSpeed));

        // Wait before being able to call this method again
        yield return new WaitForSeconds(_currentRechargeSpeed);

        _isFiring = false;
    }

    // Fade out laser when it is done shooting (and destroy it)
    private IEnumerator FadeLaser (LineRenderer laser, bool fadeIn, float fadeSpeed)
    {
        Color current = laser.material.color;
        float time = 0;
        while (time < fadeSpeed) {
            // Keep updated positions while laser is fading
            laser.SetPosition(0, _firePoint.position);
            laser.SetPosition(1, _fireTarget.position);

            current.a = Mathf.Lerp(fadeIn ? 0 : 1, fadeIn ? 1 : 0, time / fadeSpeed);
            laser.material.color = current;
            time += Time.deltaTime;
            yield return null;
        }
        if (!fadeIn)
            Destroy(laser.gameObject);
        else {
            current.a = 1;
            laser.material.color = current;
        }
    }

    // Fade in sprite when weapon first purchased
    private IEnumerator FadeSpriteIn (float speed)
    {
        Color current = _Sprite.color;
        float time = 0;
        while (time < speed) {
            current.a = Mathf.Lerp(0, 1, time / speed);
            _Sprite.color = current;

            time += Time.deltaTime;
            yield return null;
        }
        current.a = 1;
        _Sprite.color = current;
    }

    #endregion PRIVATE
    #endregion METHODS
}
