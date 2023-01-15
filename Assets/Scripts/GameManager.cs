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
    [SerializeField] private float _beforeFirstPhase;   // The time before the first (infinite) shoppint phase
    [SerializeField] private float _timeBetweenPhases;  // The time between enemy phase and buy phase
    [SerializeField] private float _spawnPhaseLength;   // Time during which enemies spawn (10 SECONDS)
    [SerializeField] private float _shopPhaseLength;    // Time during which the player can buy new upgrades (10 SECONDS)
    private float _timeElapsed = 0;                     // Phases time measured since the game started
    private Phase _currentPhase = Phase.WAVE;           // Tracks which phase the game is currently in
    private Coroutine _PlayCoroutine;                   // Keeps track of the PlayGame coroutine, to stop it if game is over
    private bool _isReady = false;                      // Marks the first game loop and any other shop loop if in timeless mode
    private bool _isMidGame = false;                    // Marks when enemies start to be stronger
    private bool _isEndGame = false;                    // Marks when enemies start to be stronger (and eventually overrrun defenses)
    private bool _isTimelessMode = false;               // This game mode means purchase phases are infinite (until player sets ready)

    [Header("Enemy Spawning")]
    [SerializeField] private BoxCollider2D _SpawningArea;               // A rectangular 2D box along the edges of which enemies spawn
    [SerializeField] private GameObject _EnemyHierarchyContainer;       // An empty GameObject holding all Enemy clones
    [SerializeField] private GameObject _EnemyPrefab;                   // The enemy prefab spawned
    [SerializeField] private GameObject _EnemyStrongerPrefab;           // A stronger version of the enemy prefab
    [SerializeField] private int _enemiesToSpawnPerSecond;              // How many enemies to spawn per second of the Wave phase
    [SerializeField] private int _maxEnemySpawnRate;                    // This limits the amount of enemies spawned to save framerate
    [SerializeField] private int _midGameWave;                          // Wave at which game enters Mid Game (stonger enemies chance)
    [SerializeField] private float _chanceToSpawnStronger;              // One spawn cap is reached, have chance to spawn stronger enemies
    [SerializeField] private float _strongChanceIncrease;               // Slow increase to stronger spawn chance over time
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

    // Originally, you received credits for destroying enemies
    // But I thought I'd do it differently for once and not make it about money
    // Now you receive energy - fighting the ongoing crisis!
    // (I just like the word "credits", don't judge me please)
    [Header("Credits")]
    [SerializeField] private int _startingCreds;    // Amount of credits the player starts the game with
    [SerializeField] private int _credsPerKill;     // Amount of credits gained per enemy destroyed
    [SerializeField] private int _storePerWave;     // How many stored credits you add every wave (automatically)
    [SerializeField] private int _credsPerWave;     // A fixed amount of credits added per wave (for early game balancing)
    private float _currentCreds = 0;                // Current held credits by the player
    private float _storedCreds = 0;                 // Current energy credits sent back to earth

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
        // Initialize current credits and display on UI
        UpdateCredits(_startingCreds);

        if (MusicPlayer.GetInstance())
            _isTimelessMode = MusicPlayer.GetInstance()._isTimelessMode;

        // Start the game!
        _PlayCoroutine = StartCoroutine(PlayGameLoop(_isTimelessMode));
    }

    // Update is called once per frame
    private void Update ()
    {
        // If the station died on this frame, stop the Play coroutine from continuing
        if (_currentPhase == Phase.DEAD && _PlayCoroutine != null)
            StopCoroutine(_PlayCoroutine);

        // DEBUG: Spawn enemies on Spacebar press (REMOVE)
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    StartCoroutine(SpawnEnemiesEachSecond(_enemiesToSpawnPerSecond));
        //}

        // DEBUG KEY (REMOVE)
        //if (Input.GetKeyDown(KeyCode.Q)) {
        //    //UIManager.GetInstance().ToggleShop();
        //    int j = 0;
        //    int i = 0;

        //    while (i < 3) {
        //        while (j < 4) {
        //            AttemptUpgrade(Station.Element.DefenseOrb, i);
        //            j++;
        //        }
        //        i++;
        //        j = 0;
        //    }
        //    i = 0;
        //    j = 0;
        //    while (i < 4) {
        //        while (j < 4) {
        //            AttemptUpgrade(Station.Element.Complexity, i);
        //            j++;
        //        }
        //        i++;
        //        j = 0;
        //    }
        //    i = 0;
        //    j = 0;
        //    while (i < 2) {
        //        while (j < 4) {
        //            AttemptUpgrade(Station.Element.Pulsar, i);
        //            j++;
        //        }
        //        i++;
        //        j = 0;
        //    }
        //    j = 0;
        //    while (j < 4) {
        //        AttemptUpgrade(Station.Element.LargeShield);
        //        j++;
        //    }
        //    j = 0;
        //    while (j < 4) {
        //        AttemptUpgrade(Station.Element.SmallShield);
        //        j++;
        //    }
        //    j = 0;
        //    while (j < 3) {
        //        AttemptUpgrade(Station.Element.StationHQ);
        //        j++;
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //    StartCoroutine(EndGame());
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
    public void DeclareReady () => _isReady = true;

    #endregion UI CALLBACKS

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
        // Ask UIManager to start end game UI animations before reloading main menu
        yield return UIManager.GetInstance().EndGameAnims();

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

        // Check if there are more enemies in play
        // Only animate credits if this was last enemy in play
        if (_Enemies.Count == 0) {
            _enemiesInPlay = false;
            // Increase credits! $$$$
            UpdateCredits(_credsPerKill);
        } else
            UpdateCredits(_credsPerKill, false);
    }

    // Store all current credits to send them back to Earth!
    public void StoreCredits (int amount = -1)
    {
        float prev = _storedCreds;

        if (amount > 0) {
            _storedCreds += _storePerWave;
        } else {
            _storedCreds += _currentCreds;
            UpdateCredits(-_currentCreds);
        }

        // Update the UI display and animate it!
        UIManager.GetInstance().UpdateStoredDisplay(prev, _storedCreds);
    }

    // GETTERS
    public float GetCurrentCreds () => _currentCreds;
    public float GetStoredCreds () => _storedCreds;
    public Phase GetCurrentPhase () => _currentPhase;
    public bool GetIsTimeless () => _isTimelessMode;
    
    // SETTERS
    public void SetGameOverState () => _currentPhase = Phase.DEAD;

    #endregion PUBLIC

    #region PRIVATE

    // THE GAME LOOP
    // This coroutine is crucial as it manages the flow between phases
    private IEnumerator PlayGameLoop (bool isTimless)
    {   
        // Start phase is always shopping
        _currentPhase = Phase.SHOP;

        // Fade from black to transition from main menu fade
        yield return UIManager.GetInstance().FadeScreen(false, _blackScreenFadeSpeed);

        // On game start, give the players some time to get familiar with their surroundings
        yield return new WaitForSeconds(_beforeFirstPhase);
        
        // First phase is an infinite shop phase that players have to opt out of
        StartCoroutine(UIManager.GetInstance().ToggleShop());
        while (!_isReady) {
            yield return null;
        }
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
                    // Every time you enter shop mode after the first one, store energy credits automatically
                    StoreCredits(_storePerWave);

                    // You also gain a fixed amount of credits per turn (to help the early game)
                    UpdateCredits(_credsPerWave);

                    // If in timeless mode, mark the phase as not ready to move until player decides
                    if (isTimless) {
                        _isReady = false;
                    } else {
                        // Start time and display it, but only in normal mode
                        StartCoroutine(PhaseTimer(_shopPhaseLength));
                    }

                    // Heal shields manually
                    _LargeShield.ManualShieldRecharge();
                    _SmallShield.ManualShieldRecharge();

                    // Display the shop for _shopPhaseLength (10 SECONDS) or infinitely in timeless mode
                    StartCoroutine(UIManager.GetInstance().ToggleShop());
                    if (isTimless) {
                        // Wait for player to press the ready button
                        while (!_isReady)
                            yield return null;
                    } else {
                        yield return new WaitForSeconds(_shopPhaseLength);
                    }
                    // Hide the shop again
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
        if (!_isEndGame) {
            float newSpawnRate = _enemiesToSpawnPerSecond;
            newSpawnRate += _fixedSpawnIncrease * (1 + UnityEngine.Random.Range(0, _spawnIncreaseRate));

            // If enemies to spawn are more than 150/second, this will cause frames to drop exponentially
            // Instead, limit the enemies spawned but make them stronger by marking end game
            if (newSpawnRate > _maxEnemySpawnRate) {
                _enemiesToSpawnPerSecond = _maxEnemySpawnRate;
                _isEndGame = true;
            } else {
                _enemiesToSpawnPerSecond = Mathf.RoundToInt(newSpawnRate);
            }
        } else {
            // Instead of spawning more enemies, increase their strength over time
            _chanceToSpawnStronger *= 1 + _strongChanceIncrease;
        }
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
        // If max spawn rate achieved, start spawning stronger enemies instead
        bool isStronger;
        if (_isMidGame || _isEndGame) {
            isStronger = _chanceToSpawnStronger >= UnityEngine.Random.Range(0f, 1f);
        } else {
            isStronger = false;
        }

        GameObject enemy = Instantiate(
            isStronger ? _EnemyStrongerPrefab : _EnemyPrefab,
            spawnPosition,
            Quaternion.identity,
            _EnemyHierarchyContainer.transform
        );

        _Enemies.Add(enemy.GetComponent<Enemy>());
    }

    // Spawn enemy waves. Used Coroutine to try and reduce computational load per frame
    private IEnumerator SpawnEnemiesEachSecond (int amount)
    {
        // Increase wave counter and notify UI Manager;
        _waveCounter++;
        string txt = "Wave " + _waveCounter;
        UIManager.GetInstance().UpdateWaveCounter(txt);

        // If we've reached the mid game point, mark it
        if (_waveCounter == _midGameWave)
            _isMidGame = true;

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
    private void UpdateCredits (float amount, bool animate = true)
    {
        float oldCreds = _currentCreds;
        _currentCreds += amount;
        if (animate)
            UIManager.GetInstance().UpdateCredits(oldCreds, _currentCreds);
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
