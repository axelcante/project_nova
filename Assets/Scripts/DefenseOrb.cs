using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenseOrb : MonoBehaviour
{
    #region VARIABLES

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _Sprite;    // Visual element of this weapon
    [SerializeField] private float _spriteFadeInSpeed;  // Speed at which the sprite is rendered in when purchased
    [SerializeField] private GameObject _LaserPrefab;   // Laser prefab gameobject containing a line renderer
    [SerializeField] private Transform _FirePoint;      // The 2D point where the laser will originate
    [SerializeField] private float _laserFadeSpeed;     // Time before which the laser dissipates
    [SerializeField] private float _delayBetweenShot;   // Time between shots of a same charge

    [Header("UI Shop Elements")]
    [SerializeField] private TMP_Text _PriceDisplay;    // Credit cost for upgrade
    [SerializeField] private TMP_Text _CreditsDisplay;  // "Credits" text to be disabled on max level
    [SerializeField] private TMP_Text _LevelDisplay;    // Current level UI display
    [SerializeField] private Button _BuyButton;         // Shop button to buy an upgrade

    private bool _isActive = false;         // By default, a weapon is inactive at start of the game
    private bool _isFiring = false;         // Tracks if a fire coroutine is already running
    private bool _isEnemyPhase = false;     // Tracks if weapon systems should be online (enemies present)

    // Properties (updated by level)
    private int _levelNb = -1;                  // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel = false;           // Checks if this weapon can still be upgraded
    private Upgrades.DefenseOrbLevel _Level;    // Holds a reference to the current weapon level
    private float _currentRechargeSpeed;        // Time before laser can shoot again
    private int _currentLasersPerCharge;        // Number of lasers fired per charge
    private float _nextUpgradePrice = 0;        // Amount of credits required to purchase next upgrade

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Called before the first frame when this object is set to active
    private void Start ()
    {
        // Initialize the Shop UI
        SetLevelAndPriceUI();
    }

    // Called once per frame
    private void Update ()
    {
        if (_isActive) {
            if (!_isFiring)
                StartCoroutine(Fire());
        }
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
            if (_levelNb == Upgrades.GetInstance()._OrbLevels.Count - 1)
                _isMaxLevel = true;

            if (Upgrades.GetInstance()._OrbLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._OrbLevels[_levelNb];

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
    private void SetLevelProperties (Upgrades.DefenseOrbLevel level)
    {
        _currentRechargeSpeed = level._rechargeSpeed;
        _currentLasersPerCharge = level._lasersPerCharge;

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
            if (Upgrades.GetInstance()._Prices.TryGetValue(Upgrades.Type.DEFENSEORB, out prices)) {
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
