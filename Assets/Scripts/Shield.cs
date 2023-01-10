using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private Station.StationElement _stationElement; // Identifier for this station element
    private Station _Station;                       // Reference to the singleton Station.cs script

    [Header("Animations")]
    [SerializeField] private float _fadeSpeed;      // The speed at which the shield alpha fades (in or out)
    [SerializeField] private float _colorSwapSpeed; // The speed at which the shield swaps from two colors
    [SerializeField] private float _hitSpeed;       // The speed at which the shield animates being hit
    [SerializeField] Color _ShieldAliveColor;       // Color when the shield is alive and fully charged
    [SerializeField] Color _ShieldDeadColor;        // Color when the shield is dying
    [SerializeField] float _shieldDeadAlpha;        // Alpha when the shield is dying
    [SerializeField] Color _ShieldHitColor;         // Color when the shield is hit

    private Coroutine _FlashOnHit = null;   // Coroutine tracking
    private Coroutine _Recharge = null;     // Coroutine tracking

    [Header("Health & health bar")]
    [SerializeField] private float _maxHealth;          // Maximum possible health (can be increased)
    [SerializeField] private float _shieldCooldown;     // Time to recharge a shield to full health
    [SerializeField] private HealthBar _HealthBar;      // Reference to the health bar UI element
    private float _health;                              // Current shield helth
    private bool _isAlive;                              // Boolean used by other scripts to determine if this shield is active

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Reduce current shield health by x amount
    public void TakeDamange (float damage)
    {
        _health -= damage;
        _HealthBar.UpdateHealth(_health);

        // Shield takes damage
        if (_health > 0) {
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
    public bool IsAlive () => _isAlive;

    #endregion PUBLIC

    #region PRIVATE

    // Called when the shield is set to active (when the player unlocks it)
    private void OnEnable ()
    {
        _isAlive = true;

        // Get references to Station instance & set this shield's type
        _Station = Station.GetInstance();

        // Initialize health to maximum possible
        _health = _maxHealth;

        // Set the corresponding health bar to active
        _HealthBar.gameObject.SetActive(true);
        _HealthBar.SetMaxHealth(_maxHealth);
        if (_stationElement == Station.StationElement.LargeShield)
            _HealthBar.SetName("L. Shield");
        else
            _HealthBar.SetName("S. Shield");

        // Update the health bar accordingly
        _HealthBar.UpdateHealth(_health);
    }

    // When shields run out of health, call this coroutine to set a timer as they recharge
    private IEnumerator Recharge ()
    {
        // 1. Deactivate shield, then fade it out
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldDeadColor, _colorSwapSpeed);
        yield return Toggle(false);
        // 2. Wait for shield to recharge
        yield return new WaitForSeconds(_shieldCooldown);
        // 3. When shield is recharged, fade it in, then deactivate it
        yield return Toggle(true);
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldAliveColor, _colorSwapSpeed);
        // 4. Recharge this shield's health
        _health = _maxHealth;
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
