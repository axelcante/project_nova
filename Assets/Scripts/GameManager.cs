using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static GameManager _instance;
    public static GameManager GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    // Used to mark which phase the game is currently in
    public enum Phase
    {
        SHOP,
        WAVE,
        DEAD
    }

    [Header("Game loop flow")]
    [SerializeField] private float _beforeFirstPhase;  // The time before the first (infinite) shoppint phase
    [SerializeField] private float _timeBetweenPhases;  // The time between enemy phase and buy phase
    [SerializeField] private float _spawnPhaseLength;   // Time during which enemies spawn (10 SECONDS)
    [SerializeField] private float _shopPhaseLength;    // Time during which the player can buy new upgrades (10 SECONDS)
    private float _timeElapsed = 0;                     // Phases time measured since the game started
    private Phase _currentPhase = Phase.WAVE;           // Tracks which phase the game is currently in
    private Coroutine _PlayCoroutine;                   // Keeps track of the PlayGame coroutine, to stop it if game is over
    private bool _isStart = true;                       // Marks the first game loop

    [Header("Enemy Spawning")]
    [SerializeField] private BoxCollider2D _SpawningArea;               // A rectangular 2D box along the edges of which enemies spawn
    [SerializeField] private GameObject _EnemyHierarchyContainer;       // An empty GameObject holding all Enemy clones
    [SerializeField] private GameObject _EnemyPrefab;                   // The enemy prefab spawned
    [SerializeField] private int _enemiesToSpawnPerSecond;              // How many enemies to spawn per second of the Wave phase
    [SerializeField] private int _fixedSpawnIncrease;                   // A fixed number of additional enemies per wave
    [SerializeField] [Range(0, 1f)] private float _spawnIncreaseRate;   // Percentage increase to enemy spawn rate
    private int _waveCounter = 0;                                       // How many waves of enemies have been spawned since game start
    private List<Enemy> _Enemies = new List<Enemy>();                   // List containing all enemies currently alive
    private bool _enemiesInPlay = false;                                // Checks if there are enemies in the game
    [Range(0f, 0.99f)] public float sideSpawnWeight;                    // Increase chance to spawn on sides of screen (instead of on top or below)

    [Header("Weapons")]
    [SerializeField] private GameObject _WeaponsContainer;  // Container with all Station weapons
    [SerializeField] private DefenseOrb[] _DefenseOrbs;     // Array of the Defense Orb weapons
    [SerializeField] private Complexity[] _Complexities;    // Array of the Complexity weapons
    [SerializeField] private Pulsar[] _Pulsars;             // Array of the Pulsar weapons

    [Header("Shields & Station")]
    [SerializeField] private Shield _SmallShield;           // Reference to the Small Shield
    [SerializeField] private Shield _LargeShield;           // Reference to the Large Shield
    [SerializeField] private Station _Station;              // Refernce to the Station

    [Header("Credits")]
    [SerializeField] private int _startingCreds;    // Amount of credits the player starts the game with
    [SerializeField] private int _credsPerKill;     // Amount of credits gained per enemy destroyed
    private float _currentCreds = 0;                // Current held credits by the player

    [Header("Animations")]
    [SerializeField] private CinemachineImpulseSource _Impulse; // Shakes the camera when the station explodes (and game ends)
    [SerializeField] private float _blackScreenFadeSpeed;       // Speed at which the black screen covering the screen fades in/out
    [SerializeField] private float _textFadeDelay;              // End game text fade in/out
    [SerializeField] private float _weaponFadeSpeed;            // Weapon sprites fade out when station expldoes

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Awake is called upon instantiation
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of GameManager script running");
        }
        _instance = this;
    }

    // Start is called just before the first frame in which this script is initialized, and after Awake
    private void Start ()
    {
        // Initialize current credits
        UpdateCredits(_startingCreds);

        // Start the game!
        _PlayCoroutine = StartCoroutine(PlayGameLoop());
    }

    // Update is called once per frame
    private void Update ()
    {
        // If the station died on this frame, stop the Play coroutine from continuing
        if (_currentPhase == Phase.DEAD)
            StopCoroutine(_PlayCoroutine);

        // DEBUG: Spawn enemies on Spacebar press (REMOVE)
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    StartCoroutine(SpawnEnemiesEachSecond(_enemiesToSpawnPerSecond));
        //}

        // DEBUG KEY (REMOVE)
        if (Input.GetKeyDown(KeyCode.Q)) {
            //UIManager.GetInstance().ToggleShop();
            bool couldUpgrade = true;
            while (couldUpgrade) {
                int i = 0;
                couldUpgrade = AttemptUpgrade(Station.Element.DefenseOrb, i);
                i++;
            }
            couldUpgrade = true;
            while (couldUpgrade) {
                int i = 0;
                couldUpgrade = AttemptUpgrade(Station.Element.Complexity, i);
                i++;
            }
            couldUpgrade = true;
            while (couldUpgrade) {
                int i = 0;
                couldUpgrade = AttemptUpgrade(Station.Element.Pulsar, i);
                i++;
            }
            couldUpgrade = true;
            while (couldUpgrade) {
                couldUpgrade = AttemptUpgrade(Station.Element.LargeShield);
            }
            couldUpgrade = true;
            while (couldUpgrade) {
                couldUpgrade = AttemptUpgrade(Station.Element.SmallShield);
            }
            couldUpgrade = true;
            while (couldUpgrade) {
                couldUpgrade = AttemptUpgrade(Station.Element.StationHQ);
            }
        }
    }

    #endregion UNITY

    #region PUBLIC

    // All methods called by the UIManager.cs
    #region UI CALLBACKS

    // By giving a station element (defensive or weapon) + id, we can increase level on the correct gameobject
    // Returns true if this was possible (enough CA$H MONEY)
    public bool AttemptUpgrade (Station.Element item, int id = -1)
    {
        switch (item) {
            case Station.Element.StationHQ:
                // Have enough CA$H?
                if (_currentCreds >= _Station.GetUpgreadePrice()) {
                    UpdateCredits(-1 * _Station.GetUpgreadePrice());
                    _Station.IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            case Station.Element.SmallShield:
                // Have enough CA$H?
                if (_currentCreds >= _SmallShield.GetUpgreadePrice()) {
                    UpdateCredits(-1 * _SmallShield.GetUpgreadePrice());
                    _SmallShield.IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            case Station.Element.LargeShield:
                // Have enough CA$H?
                if (_currentCreds >= _LargeShield.GetUpgreadePrice()) {
                    UpdateCredits(-1 * _LargeShield.GetUpgreadePrice());
                    _LargeShield.IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            case Station.Element.DefenseOrb:
                // Have enough CA$H?
                if (_currentCreds >= _DefenseOrbs[id].GetUpgreadePrice()) {
                    UpdateCredits(-1 * _DefenseOrbs[id].GetUpgreadePrice());
                    _DefenseOrbs[id].IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            case Station.Element.Complexity:
                // Have enough CA$H?
                if (_currentCreds >= _Complexities[id].GetUpgreadePrice()) {
                    UpdateCredits(-1 * _Complexities[id].GetUpgreadePrice());
                    _Complexities[id].IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            case Station.Element.Pulsar:
                // Have enough CA$H?
                if (_currentCreds >= _Pulsars[id].GetUpgreadePrice()) {
                    UpdateCredits(-1 * _Pulsars[id].GetUpgreadePrice());
                    _Pulsars[id].IncreaseLevel();
                    return true;
                } else {
                    return false;
                }
            default:
                Debug.LogWarning("No corresponding Element");
                return false;
        }
    }

    // Attempt to pay for station to heal itself
    public bool AttemptStationRepair ()
    {
        // Have enough CA$H?
        if (_currentCreds >= _Station.GetRepairPrice()) {
            UpdateCredits(-1 * _Station.GetRepairPrice());
            _Station.RepairStation();
            return true;
        } else {
            return false;
        }
    }

    // Declare ready to start the game
    public void DeclareReady ()
    {
        _isStart = false;
    }

    #endregion UI CALLBACKS

    // Set the current game state to DEAD
    public void SetGameOverState () => _currentPhase = Phase.DEAD;

    // Generates an impulse for our camera shake. Also hides and disables weapons
    public void GenerateImpulse ()
    {
        _Impulse.GenerateImpulse();

        // Fade out weapons and disable them
        StartCoroutine(DeactiveAllWeapons());
    }

    // Deactivates all weapons (when station is down)
    // This method should be on the weapons themselves,
    // but since I didn't create a common inherited class, I shot myself in the foot
    public IEnumerator DeactiveAllWeapons ()
    {
        // Fade all Defense Orbs out
        foreach (DefenseOrb orb in _DefenseOrbs) {
            if (orb.gameObject.activeSelf)
                StartCoroutine(DeactivateWeapon(orb.GetComponent<SpriteRenderer>(), _weaponFadeSpeed));
        }
        
        // Fade all Complexities out
        foreach (Complexity comp in _Complexities) {
            if (comp.gameObject.activeSelf)
                StartCoroutine(DeactivateWeapon(comp.GetComponent<SpriteRenderer>(), _weaponFadeSpeed));
        }

        // Fade all Pulsars out
        foreach (Pulsar pulsar in _Pulsars) {
            if (pulsar.gameObject.activeSelf)
                StartCoroutine(DeactivateWeapon(pulsar.GetComponent<SpriteRenderer>(), _weaponFadeSpeed));
        }

        // Wait for all fade animations to be over
        yield return new WaitForSeconds(_weaponFadeSpeed);

        // Deactivate all the weapon gameobjects
        _WeaponsContainer.SetActive(false);
    }

    // Fade out a weapon using its sprite renderer
    private IEnumerator DeactivateWeapon (SpriteRenderer sprite, float speed)
    {
        // Lerp the color from it's current one to one with no alpha (invisible)
        float time = 0;
        Color startColor = sprite.color;
        Color targetColor = startColor;
        targetColor.a = 0;

        while (time < speed) {
            sprite.color = Color.Lerp(startColor, targetColor, time / speed);
            time += Time.deltaTime;

            yield return null;
        }
    }

    // Start the end game coroutines
    public IEnumerator EndGame ()
    {
        // Fade in and out end text
        yield return UIManager.GetInstance().FadeEndText(true);
        yield return new WaitForSeconds(_textFadeDelay);
        yield return UIManager.GetInstance() .FadeEndText(false);

        // Return to main menu
        SceneManager.LoadScene(0);
    }

    // Returns the first enemy in the list for auto-targetting
    public Enemy GetEnemy ()
    {
        if (_Enemies.Count > 0)
            return _Enemies[0];
        else
            return null;
    }

    // Removes a enemy from the list (when they are destroyed) and increase credits
    public void RemoveEnemy (Enemy enemy)
    {
        _Enemies.Remove(enemy);

        // Increase credits! $$$$
        UpdateCredits(_credsPerKill);

        // Check if there are more enemies in play
        // COULD BE PERFORMANCE ISSUE HERE
        if (_Enemies.Count == 0)
            _enemiesInPlay = false;
    }

    // GETTERS
    public float GetCurrentCreds () => _currentCreds;
    public Phase GetCurrentPhase () => _currentPhase;

    #endregion PUBLIC

    #region PRIVATE

    // THE GAME LOOP
    // This coroutine is crucial as it manages the flow between phases
    private IEnumerator PlayGameLoop ()
    {
        // Fade from black to transition from main menu fade
        yield return UIManager.GetInstance().FadeBlackScreen(false, _blackScreenFadeSpeed);

        // On game start, give the players some time to get familiar with their surroundings
        yield return new WaitForSeconds(_beforeFirstPhase);
        
        // First phase is an infinite shop phase that players have to opt out of
        StartCoroutine(UIManager.GetInstance().ToggleShop());
        while (_isStart)
            yield return null;
        StartCoroutine(UIManager.GetInstance().ToggleShop());

        // Pause the game before next phase
        yield return new WaitForSeconds(_timeBetweenPhases);

        // First phase in the loop - WAVE
        _currentPhase = Phase.WAVE;


        // As long as the player/station is not destroyed ==> GAME LOOP
        while (_currentPhase != Phase.DEAD) {
            switch (_currentPhase) {
                case Phase.WAVE:
                    // Start time and display it
                    StartCoroutine(PhaseTimer(_spawnPhaseLength));

                    // Spawn enemies over the duration of the phase (10 SECONDS)
                    StartCoroutine(SpawnEnemiesEachSecond(_enemiesToSpawnPerSecond));
                    yield return new WaitForSeconds(_spawnPhaseLength);

                    // As long as there are still enemies in play, don't move to new phase
                    // Check every second -- Bad practice?
                    while (_enemiesInPlay)
                        yield return new WaitForSeconds(1);

                    // Increase next wave enemy amount
                    IncreaseSpawnRate();

                    // Pause the game before next phase
                    yield return new WaitForSeconds(_timeBetweenPhases);

                    _currentPhase = Phase.SHOP;
                    break;

                case Phase.SHOP:
                    // Start time and display it
                    StartCoroutine(PhaseTimer(_shopPhaseLength));

                    // Heal shields manually
                    _LargeShield.ManualShieldRecharge();
                    _SmallShield.ManualShieldRecharge();

                    // Display the shop for _shopPhaseLength (10 SECONDS)
                    StartCoroutine(UIManager.GetInstance().ToggleShop());
                    yield return new WaitForSeconds(_shopPhaseLength);
                    StartCoroutine(UIManager.GetInstance().ToggleShop());

                    // Pause the game before next phase
                    yield return new WaitForSeconds(_timeBetweenPhases);
                    _currentPhase = Phase.WAVE;
                    break;
            }
        }

        // PLAYER DEAD -- END GAME?
    }

    // Increase the amount of enemies per wave over time
    private void IncreaseSpawnRate ()
    {
        float newSpawnRate = _enemiesToSpawnPerSecond;
        newSpawnRate += _fixedSpawnIncrease * (1 + UnityEngine.Random.Range(0, _spawnIncreaseRate));
        _enemiesToSpawnPerSecond = Mathf.RoundToInt(newSpawnRate);
    }

    // Select a random point along the edges of a 2D Capsule Collider, just outside of camera range
    private void SpawnEnemy ()
    {
        // Get the bounds of the collider
        Bounds spawnBounds = _SpawningArea.bounds;

        // Generate random values to determine the position on the collider's edges and direction (left or right, top or bottom)
        float position = UnityEngine.Random.Range(0f, 1f);
        float direction = UnityEngine.Random.Range(-1f, 1f);

        // Decides if the spawn point will be on the right/left of the collider bounds, or the top/bottom
        // Since the screen is rectangular, it makes more sense to add "weight" to this and try and spawn more enemies on the sides
        float H_V_spawn = UnityEngine.Random.Range(0f, 1f);

        // Calculate the spawn position
        Vector2 spawnPosition;
        if (direction < 0) {
            if (H_V_spawn < sideSpawnWeight) {
                // Spawn along the left edge
                spawnPosition = new Vector2(spawnBounds.min.x, spawnBounds.min.y + position * spawnBounds.size.y);
            } else {
                // Spawn along the top edge
                spawnPosition = new Vector2(spawnBounds.min.x + position * spawnBounds.size.x, spawnBounds.max.y);
            }
        } else {
            if (H_V_spawn < sideSpawnWeight) {
                // Spawn along the right edge
                spawnPosition = new Vector2(spawnBounds.max.x, spawnBounds.min.y + position * spawnBounds.size.y);
            } else {
                // Spawn along the bottom edge
                spawnPosition = new Vector2(spawnBounds.min.x + position * spawnBounds.size.x, spawnBounds.min.y);
            }
        }

        // Create an instance of the prefab at the random position and add it to the list of enemies
        GameObject enemy = Instantiate(_EnemyPrefab, spawnPosition, Quaternion.identity, _EnemyHierarchyContainer.transform);
        _Enemies.Add(enemy.GetComponent<Enemy>());
    }

    // Spawn enemy waves. Used Coroutine to try and reduce computational load per frame
    private IEnumerator SpawnEnemiesEachSecond (int amount)
    {
        // Increase wave counter and notify UI Manager;
        _waveCounter++;
        string txt = "Wave " + _waveCounter;
        UIManager.GetInstance().UpdateWaveCounter(txt);

        int spawned = 0;
        int loadCount = 0;

        // Do this loop once every second until _spawnPhaseLength (10 SECONDS) - 10 times
        for (int i = 0; i < 10; i++) {
            while (spawned < amount) {
                _enemiesInPlay = true;
                SpawnEnemy();
                spawned++;
                loadCount++;

                if (loadCount == 100) {
                    // Attempt at load balancing by instantiating a maximum of 100 enemies per frame
                    // PS: I have no idea if this is useful, or works as expected
                    loadCount = 1;
                    yield return null;
                }
            }

            // Wait 1 second before spawning next wave
            yield return new WaitForSeconds(1);

            // Reset amount of enemies spawned for next loop
            spawned = 0;
        }
        
    }

    // Update credits
    private void UpdateCredits (float amount)
    {
        _currentCreds += amount;
        UIManager.GetInstance().UpdateCredits(_currentCreds);
    }

    // Calculate time left per phase (to display)
    private IEnumerator PhaseTimer (float duration)
    {
        UIManager UIinstance = UIManager.GetInstance();
        float endTime = _timeElapsed + duration;
        string minutes = "";
        string seconds = "";
        string miliseconds = "";

        // Run a timer from current _timeElapsed to _timeElapsed + duration, displaying time in UI
        while (_timeElapsed <= endTime) {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_timeElapsed);

            // Parse minutes, seconds and miliseconds into string format
            minutes = string.Format("{0:D3}", timeSpan.Minutes);
            seconds = string.Format("{0:D3}", timeSpan.Seconds);
            miliseconds = string.Format("{0:D3}", timeSpan.Milliseconds);
            UIinstance.UpdateTime(minutes, seconds, miliseconds);
            
            _timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _timeElapsed = endTime;
        TimeSpan ts = TimeSpan.FromSeconds(_timeElapsed);
        minutes = string.Format("{0:D3}", ts.Minutes);
        seconds = string.Format("{0:D3}", ts.Seconds);
        miliseconds = string.Format("{0:D3}", ts.Milliseconds);
        UIinstance.UpdateTime(minutes, seconds, miliseconds);
    }

    #endregion PRIVATE
    #endregion METHODS
}
