using System.Collections;
using UnityEngine;

public class Pulsar : MonoBehaviour
{
    #region VARIABLES

    [Header("Visuals")]
    [SerializeField] private Animator _Animator;        // Animator will play "pulse" animation and update rotation speed
    [SerializeField] private SpriteRenderer _Sprite;    // Reference to the sprite renderer displaying this object
    [SerializeField] private float _fadeSpeed;    // Speed at which the weapon fades out (when game is over)

    [Header("Physics")]
    [SerializeField] private LayerMask _EnemyLayer;     // The LayerMask containing all enemies

    public int _id = 0;                 // Differentiate this pulsar from the other one (1 or 2), for the animator
    private bool _isPulsing = false;    // Tracks if a pulse coroutine is already running

    // Properties (updated by level)
    private int _levelNb = -1;              // Tracks the current upgrade level for this weapon
    private bool _isMaxLevel = false;       // Checks if this weapon can still be upgraded
    private Upgrades.PulsarLevel _Level;    // Holds a reference to the current weapon level
    private float _currentRechargeSpeed;    // Time before laser can shoot again
    private float _currentRotationSpeed;    // Number of lasers fired per charge
    private float _currentBlastRadius;      // The radius of the OverlapCircleAll method used to detect colliders

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
        if (!_isPulsing)
            StartCoroutine(Pulse());
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
