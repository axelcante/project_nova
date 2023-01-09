using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private bool _isLargeShield;   // Editor toggle to differentiate large from small shield
    private Station.StationElement _stationElement; // Identifier for this station element
    private float _health;                          // Current shield helth
    private bool _isAlive;                          // Boolean used by other scripts to determine if this shield is active
    private Station _Station;                       // Reference to the singleton Station.cs script

    [Header("Animations")]
    [SerializeField] private float _fadeSpeed;      // The speed at which the shield alpha fades (in or out)
    [SerializeField] private float _colorSwapSpeed; // The speed at which the shield swaps from two colors
    [SerializeField] private float _hitSpeed;       // The speed at which the shield animates being hit
    [SerializeField] Color _ShieldAliveColor;       // Color when the shield is alive and fully charged
    [SerializeField] Color _ShieldDeadColor;        // Color when the shield is dying
    [SerializeField] float _shieldDeadAlpha;        // Alpha when the shield is dying
    [SerializeField] Color _ShieldHitColor;         // Color when the shield is hit
    //private bool isAnimating = false;               // Coroutine manipulation boolean

    private Coroutine _FlashOnHit = null;   // Coroutine tracking
    private Coroutine _Recharge = null;     // Coroutine tracking

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Reduce current shield health by x amount
    public void TakeDamange (float damage)
    {
        _health -= damage;

        if (_health <= 0) {
            _isAlive = false;
            if (_FlashOnHit != null) {
                StopCoroutine(_FlashOnHit);
                _FlashOnHit = null;
            }
            StartCoroutine(Recharge());
        } else {
            if (_FlashOnHit == null) {
                _FlashOnHit = StartCoroutine(FlashOnHit());
            }
        }
    }

    // GETTERS
    public bool IsAlive () => _isAlive;

    // SETTERS
    public void SetHealth (float health) => _health = health;

    #endregion PUBLIC

    #region PRIVATE

    // Start is called just before the first frame
    private void Start ()
    {
        _Station = Station.GetInstance();
        _stationElement = _isLargeShield ? Station.StationElement.LargeShield : Station.StationElement.SmallShield;

        // Initialize health
        _health = _isLargeShield ? _Station.GetMaxLargeShieldHealth() : _Station.GetMaxSmallShieldHealth();
        _isAlive = _health > 0;
    }

    // When shields run out of health, call this coroutine to set a timer as they recharge
    private IEnumerator Recharge ()
    {
        // 1. Deactivate shield, then fade it out
        _isAlive = false;
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldDeadColor, _colorSwapSpeed);
        yield return Toggle(false);
        // 2. Wait for shield to recharge
        yield return new WaitForSeconds(_Station.GetShieldCooldown());
        // 3. When shield is recharged, fade it in, then deactivate it
        yield return Toggle(true);
        yield return TransitionColors(GetComponent<SpriteRenderer>().color, _ShieldAliveColor, _colorSwapSpeed);
        // 4. Recharge this shield's health
        _Station.RechargedShieldHealth(this, _stationElement);
        _isAlive = true;
    }

    // Call this coroutine to "flash" the shield color (simulating hit)
    private IEnumerator FlashOnHit ()
    {
        Color currentColor = GetComponent<SpriteRenderer>().color;
        yield return TransitionColors(currentColor, _ShieldHitColor, _hitSpeed / 3);
        yield return TransitionColors(_ShieldHitColor, currentColor, _hitSpeed);
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
        while (time < _fadeSpeed) {
            currentColor.a = Mathf.Lerp(toActivate ? 0 : _shieldDeadAlpha / 255, toActivate ? _shieldDeadAlpha / 255 : 0, time / _fadeSpeed);
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
