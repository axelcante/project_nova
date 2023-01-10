using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static GameManager _instance;
    public static GameManager GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    [Header("Animations")]
    [SerializeField] private CinemachineImpulseSource _Impulse;
    [SerializeField] private float _textFadeDelay;

    [Header("Enemy Spawning")]
    [SerializeField] private BoxCollider2D _SpawningArea;
    [SerializeField] private GameObject _EnemyHierarchyContainer;
    [SerializeField] private GameObject _EnemyPrefab;
    [SerializeField] private int _numberOfEnemiesPerWave;
    [Range(0f, 0.99f)]
    public float sideSpawnWeight;

    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Generates an impulse for our camera shake
    public void GenerateImpulse ()
    {
        _Impulse.GenerateImpulse();
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

    #endregion PUBLIC

    #region PRIVATE

    // Awake is called upon instantiation
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of GameManager script running");
        }
        _instance = this;
    }

    // Update is called once per frame
    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(SpawnNumberOfEnemies(_numberOfEnemiesPerWave));
        }
    }

    // Select a random point along the edges of a 2D Capsule Collider, just outside of camera range
    private void SpawnEnemy ()
    {
        // Get the bounds of the collider
        Bounds spawnBounds = _SpawningArea.bounds;

        // Generate random values to determine the position on the collider's edges and direction (left or right, top or bottom)
        float position = Random.Range(0f, 1f);
        float direction = Random.Range(-1f, 1f);

        // Decides if the spawn point will be on the right/left of the collider bounds, or the top/bottom
        // Since the screen is rectangular, it makes more sense to add "weight" to this and try and spawn more enemies on the sides
        float H_V_spawn = Random.Range(0f, 1f);

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

        // Create an instance of the prefab at the random position
        Instantiate(_EnemyPrefab, spawnPosition, Quaternion.identity, _EnemyHierarchyContainer.transform);
    }

    // Spawn enemy waves. Used Coroutine to try and reduce computational load per frame
    private IEnumerator SpawnNumberOfEnemies (int amount)
    {
        int count = 1;
        while (count <= amount) {
            SpawnEnemy();
            count++;

            if (count == 100)
                // Attempt at load balancing by instantiating a maximum of 100 enemies per frame
                // PS: I have no idea if this is useful, or works as expected
                yield return null;
        }
    }

    #endregion PRIVATE
    #endregion METHODS
}