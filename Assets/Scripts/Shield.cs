using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private bool _isLargeShield;
    private Station.StationElement _stationElement;
    private float _health;
    private bool _isAlive;
    private Station _Station;

    [Header("Animations")]
    [SerializeField] private float _fadeSpeed;
    [SerializeField] private float _colorSwapSpeed;
    [SerializeField] private float _hitSpeed;
    [SerializeField] Color _ShieldAliveColor;
    [SerializeField] Color _ShieldDeadColor;
    [SerializeField] Color _ShieldHitColor;
    [SerializeField] float _shieldDeadAlpha;
    private bool isAnimating = false;

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Reduce current shield health by x amount
    public void TakeDamange (float damage)
    {
        _health -= damage;
        StartCoroutine(FlashOnHit());
        if (_health <= 0) {
            _isAlive = false;
            StartCoroutine(Recharge());
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
        yield return TransitionColors(_ShieldAliveColor, _ShieldDeadColor, _colorSwapSpeed);
        yield return Toggle(false);
        // 2. Wait for shield to recharge
        yield return new WaitForSeconds(_Station.GetShieldCooldown());
        // 3. When shield is recharged, fade it in, then deactivate it
        yield return Toggle(true);
        yield return TransitionColors(_ShieldDeadColor, _ShieldAliveColor, _colorSwapSpeed);
        _isAlive = true;
        // 4. Recharge this shield's health
        _Station.RechargedShieldHealth(this, _stationElement);
    }

    // Call this coroutine to "flash" the shield color (simulating hit)
    private IEnumerator FlashOnHit ()
    {
        yield return TransitionColors(_ShieldAliveColor, _ShieldHitColor, _hitSpeed);
        yield return TransitionColors(_ShieldHitColor, _ShieldAliveColor, _hitSpeed);
    }

    // Call this coroutine to switch between shield dead and shield alive colors
    private IEnumerator TransitionColors (Color start, Color end, float speed)
    {
        while (isAnimating)
            yield return new WaitForEndOfFrame();

        isAnimating = true;

        float time = 0;
        while (time < speed) {
            GetComponent<SpriteRenderer>().color = Color.Lerp(start, end, time / speed);
            time += Time.deltaTime;
            yield return null;
        }
        GetComponent<SpriteRenderer>().color = end;

        isAnimating = false;
    }

    // Call this coroutine to either fade in or fade out a shield (when it is activated or destroyed)
    private IEnumerator Toggle (bool toActivate)
    {
        while (isAnimating)
            yield return new WaitForEndOfFrame();

        isAnimating = true;

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

        isAnimating = false;
    }

    #endregion PRIVATE
    #endregion METHODS
}
