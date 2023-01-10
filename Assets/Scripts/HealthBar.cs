using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _Fill;
    [SerializeField] private Slider _Slider;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _number;
    [SerializeField] private Gradient _Gradient;

    [Header("Animations")]
    [SerializeField] private float _healthUpdateSpeed;
    [SerializeField] private float _textFadeSpeed;
    private Coroutine _AnimateHealth = null;
    private Coroutine _FadingText = null;

    // Update the max health for this health bar
    public void SetMaxHealth (float maxHealth) => _Slider.maxValue = maxHealth;

    // Update the current health for this health bar
    public void UpdateHealth (float health)
    {
        if (health > _Slider.value) {   // Updating health -- need to fade the text back in
            FadeText(true);
        }

        if (health <= 0) {
            if (_AnimateHealth != null) {
                StopCoroutine(_AnimateHealth);
                FadeText(false);
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, 0, _healthUpdateSpeed));
            } else {
                FadeText(false);
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, 0, _healthUpdateSpeed));
            }
        }
        else if (health > _Slider.maxValue) {
            if (_AnimateHealth != null) {
                StopCoroutine(_AnimateHealth);
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, _Slider.maxValue, _healthUpdateSpeed));
            } else {
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, _Slider.maxValue, _healthUpdateSpeed));
            }
        }
        else {
            if (_AnimateHealth != null) {
                StopCoroutine(_AnimateHealth);
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, health, _healthUpdateSpeed));
            } else {
                _AnimateHealth = StartCoroutine(AnimateHealthBar(_Slider.value, health, _healthUpdateSpeed));
            }
        }
        
        // Update the visual display of health amount
        _number.text = health.ToString();
    }

    // Update this health bars name
    public void SetName (string name) => _name.text = name;

    // Fade out the health bar text
    public void FadeText (bool fadeIn)
    {
        if (_FadingText == null)
            StartCoroutine(FadeNameText(fadeIn ? 1 : 0, _textFadeSpeed));   // Fading text in
        else {
            StopCoroutine(_FadingText);
            StartCoroutine(FadeNameText(fadeIn ? 1 : 0, _textFadeSpeed));   // Fading text in
        }
    }

    // This is a much upgraded fading in or out coroutine, which I should really use for the rest of the project
    // But as usual, I'm too scared to change something which is working already
    private IEnumerator FadeNameText (float targetAlpha, float speed)
    {
        float time = 0;
        Color start = _name.color;
        Color end = start;
        end.a = targetAlpha;
        while (time < speed) {
            _name.color = Color.Lerp(start, end, time / speed);
            time += Time.deltaTime;

            yield return null;
        }
        _name.color = end;
    }

    // Animate the health bar going up or down
    private IEnumerator AnimateHealthBar (float currentHealth, float targetHealth, float speed)
    {
        float time = 0;
        Color currentColor = _Fill.color;
        while (time < speed) {
            // Update fill length
            _Slider.value = Mathf.Lerp(currentHealth, targetHealth, time / speed);
            // Color our health bar accordingly
            _Fill.color = Color.Lerp(currentColor, _Gradient.Evaluate(_Slider.value / _Slider.maxValue), time / speed);

            time += Time.deltaTime;
            yield return null;
        }
        _Slider.value = targetHealth;
        _Fill.color = _Gradient.Evaluate(_Slider.value / _Slider.maxValue);
    }
}
