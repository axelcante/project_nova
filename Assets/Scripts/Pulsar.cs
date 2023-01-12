using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Pulsar : MonoBehaviour
{
    #region VARIABLES

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _Sprite;    // Visual element of this weapon
    [SerializeField] private Animator _Animator;        // Animator will play "pulse" animation and update rotation speed
    [SerializeField] private float _fadeSpeed;          // Speed at which the weapon fades out (when game is over)

    [Header("Physics")]
    [SerializeField] private LayerMask _EnemyLayer;     // The LayerMask containing all enemies

    [Header("UI Shop Elements")]
    [SerializeField] private TMP_Text _PriceDisplay;    // Credit cost for upgrade
    [SerializeField] private TMP_Text _CreditsDisplay;  // "Credits" text to be disabled on max level
    [SerializeField] private TMP_Text _LevelDisplay;    // Current level UI display
    [SerializeField] private Button _BuyButton;         // Shop button to buy an upgrade

    public int _id = 0;                     // Differentiate this pulsar from the other one (1 or 2), for the animator
    private bool _isActive = false;         // By default, a weapon is inactive at start of the game
    private bool _isPulsing = false;        // Tracks if a pulse coroutine is already running
    private bool _isEnemyPhase = false;     // Tracks if weapon systems should be online (enemies present)

    // Properties (updated by level)
    private int _levelNb = -1;              // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel = false;       // Checks if this weapon can still be upgraded
    private Upgrades.PulsarLevel _Level;    // Holds a reference to the current weapon level
    private float _currentRechargeSpeed;    // Time before laser can shoot again
    private float _currentRotationSpeed;    // Number of lasers fired per charge
    private float _currentBlastRadius;      // The radius of the OverlapCircleAll method used to detect colliders
    private float _nextUpgradePrice;        // Amount of credits required to purchase next upgrade

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
            if (!_isPulsing)
                StartCoroutine(Pulse());
        }
    }

    // DEBUG
    //private void FixedUpdate ()
    //{
    //    CatchEnemiesInPulse();
    //}

    #endregion UNITY

    #region PUBLIC

    // Update weapon level
    public void IncreaseLevel ()
    {
        if (!_isMaxLevel) {
            _isActive = true;

            // Show the weapon (if it is not already shown)
            _Sprite.enabled = true;

            _levelNb++;

            // If we've reached max level, mark it so
            if (_levelNb == Upgrades.GetInstance()._PulsarLevels.Count - 1)
                _isMaxLevel = true;

            if (Upgrades.GetInstance()._PulsarLevels.Count > 0 && _levelNb >= 0) {
                _Level = Upgrades.GetInstance()._PulsarLevels[_levelNb];

                // Set current properties based on this level
                SetLevelProperties(_Level);
            } else {
                Debug.Log("Either there are no levels specified for this weapon, or current level is below 0");
            }
        } else {
            Debug.LogWarning("This weapon is already max level");
        }
    }

    // Deactivate this weapon
    public void StopFiring ()
    {
        // Stop currently running coroutines and any subsequent ones
        if (_isPulsing)
            StopCoroutine(Pulse());
        _isPulsing = true;
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
    private void SetLevelProperties (Upgrades.PulsarLevel level)
    {
        _currentRechargeSpeed = level._rechargeSpeed;
        _currentRotationSpeed = level._rotationSpeed;
        _currentBlastRadius = level._blastRadius;

        // For _rotationSpeed to work, we need to update Animator playback speed multiplier
        _Animator.SetFloat("RotationSpeed", _currentRotationSpeed);

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
            if (Upgrades.GetInstance()._Prices.TryGetValue(Upgrades.Type.PULSAR, out prices)) {
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

    // Check if any enemies are caught by the pulse's blast, and delete them
    private void CatchEnemiesInPulse ()
    {
        // Check if any colliders are within a circle drawn around the Pulsar's current position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _currentBlastRadius, _EnemyLayer);

        // Destroy all the enemies!
        foreach (Collider2D collider in colliders) {
            if (collider && collider.GetComponent<Enemy>())
                collider.GetComponent<Enemy>().ManualExplode();
        }

        // DEBUG: Draw overlap area
        for (int i = 0; i <= 360; i++) {
            float angle = i;
            Vector3 pointA = new Vector3(transform.position.x + _currentBlastRadius * Mathf.Sin(angle), transform.position.y + _currentBlastRadius * Mathf.Cos(angle), 0);
            Vector3 pointB = new Vector3(transform.position.x + _currentBlastRadius * Mathf.Sin(angle + 1), transform.position.y + _currentBlastRadius * Mathf.Cos(angle + 1), 0);
            Debug.DrawLine(pointA, pointB, Color.red);
        }
    }

    // Emit a pulse that destroys enemy in range
    private IEnumerator Pulse ()
    {
        _isPulsing = true;

        // Play the "pulse" animation on another layer (to not stop the constant rotation)
        _Animator.Play("Pulse" + _id, _id);

        // Check for any enemies caught in the pulse
        CatchEnemiesInPulse();

        // Wait before being able to call this method again
        yield return new WaitForSeconds(_currentRechargeSpeed);

        _isPulsing = false;
    }

    #endregion PRIVATE
    #endregion METHODS
}
