//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//TODO: End wave system
[RequireComponent(typeof(EnemySpawner))]
public class WaveController : MonoBehaviour
{
    public static WaveController instance;

    public List<EnemyBehaviour> enemies;
    public int activeEnemies => enemies.Count;

    private Coroutine spawncoroutine;
    public Wave[] waves;
    public int currentWave; // Wave that's being played
    private float waveTimer;

    [Header("State Machine")]
    public float timeBetweenWaves = 5f;
    public float timeBeforeRoundStarts = 3f;
    [HideInInspector]
    public float timeVariable;

    public bool isGameOver = false;
    public bool isWaveActive;
    public bool isBetweenWaves;
    public bool allWavesCleared;


    private float waveEndThreshold = 2f;
    private float waveEndTimer;


    EnemySpawner enemySpawner;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        enemySpawner = GetComponent<EnemySpawner>();
        enemies = new List<EnemyBehaviour>();

        if (GameManager.instance != null)
        {
            WorldInfo worldInfo = GameManager.instance.GetCurrentWorld();
            waves = worldInfo.waves;
        }
    }

    public void Start()
    {

        isWaveActive = false;
        //isBetweenWaves = false;

        currentWave = 0;
        //TODO: Unhardcode pre rounds start timer
        timeVariable = Time.time + (timeBeforeRoundStarts * 3);

        //LevelManager.OnGameLost += StopSpawning;
        LevelManager.OnGameCompleted += LevelCompleted;

    }

    private void OnEnable()
    {

        LevelManager.OnGameStart += StartWaves;
    }

    private void StartWaves()
    {
        this.isBetweenWaves = true;
    }

    private void LevelCompleted()
    {
        if (LevelStats.instance.CurrentBaseHealthPoints > 0)
        {
            Debug.Log("Level Complete");
            allWavesCleared = true;
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            isBetweenWaves = false;
            isWaveActive = false;
            allWavesCleared = false;

        }
        else if (allWavesCleared)
        {
            isBetweenWaves = false;
            isWaveActive = false;


        }
        else if (isBetweenWaves)
        {
            if (Time.time >= timeVariable)
            {
                isBetweenWaves = false;
                isWaveActive = true;
                waveEndTimer = 0f;
                waveTimer = 0f;
                //spawncoroutine = StartCoroutine(SpawnWave());
                return;
            }
        }
        else if (isWaveActive)
        {

            waveTimer += Time.deltaTime;

            //if spawn is not enabled timer for next wave should not run
            if (CheatManager.instance != null && !CheatManager.instance.enableEnemySpawn)
            {
                return;
            }

            CheckSpawn();

            if (activeEnemies <= 0)
            {
                waveEndTimer += Time.deltaTime;
                if (waveEndTimer >= waveEndThreshold)
                {
                    currentWave++;

                    if (currentWave + 1 > waves.Length)
                    {
                        LevelManager.instance.LevelCompleted();
                        allWavesCleared = true;
                        return;
                    }
                    LevelManager.instance.WaveCleared();
                    isBetweenWaves = true;
                    isWaveActive = false;
                    timeVariable = Time.time + timeBetweenWaves;
                }


                //broadcast to levelManager for it to dispatch wave completed event

            }
        }

    }

    private void CheckSpawn()
    {
        Wave wave = waves[currentWave];
        foreach (Pack pack in wave.packs)
        {
            if (!pack.spawned && waveTimer >= pack.spawnTime)
            {
                spawncoroutine = StartCoroutine(SpawnPack(wave, pack));
            }
        }
    }

    public void AddToActiveEnemies(EnemyBehaviour enemy)
    {
        //activeEnemies++;
        enemies.Add(enemy);
        //Debug.Log("Enemy added: " + activeEnemies);
    }

    public void ReduceActiveEnemies(EnemyBehaviour enemy)
    {
        //activeEnemies--;
        //activeEnemies = Mathf.Max(activeEnemies, 0);
        enemies.Remove(enemy);
        //Debug.Log("Enemy reduced: " + activeEnemies);

    }

    /*IEnumerator SpawnWave()
    {
        Wave currentWave = new Wave();
        currentWave = waves[waveCount];

        while (!LevelManager.instance.ready)
        {
            yield return new WaitForSeconds(1);
        }

        for (int i = 0; i < currentWave.packs.Length; i++)
        {
            Pack p = currentWave.packs[i];
            for (int j = 0; j < p.enemyAmount; j++)
            {
                //loop used to stop spawining for testing
                while (CheatManager.instance != null && !CheatManager.instance.enableEnemySpawn)
                {
                    yield return null;
                }
                int pathId = Random.Range(0, WorldManager.instance.nPaths);
                if (p.enemyType.Equals("SkyEnemy"))
                {
                    Debug.Log("a");
                }
                if (!WorldManager.instance.paths[pathId].initiated)
                    continue;
                enemySpawner.SpawnEnemy(p.enemyType, WorldManager.instance.paths[pathId]);
                waveEndTimer = 0;

                yield return new WaitForSeconds((1f / currentWave.spawnRate) + Random.Range(0f, randomRange)); //randomness between 
            }
        }
    }*/

    /*void StopSpawning()
    {
        StopCoroutine("SpawnWave");

    }*/

    IEnumerator SpawnPack(Wave wave, Pack pack)
    {
        pack.spawned = true;
        for (int i = 0; i < pack.enemyAmount; i++)
        {
            while (CheatManager.instance != null && !CheatManager.instance.enableEnemySpawn)
            {
                yield return null;
            }

            int pathId = Random.Range(0, WorldManager.instance.nPaths);
            if (!WorldManager.instance.paths[pathId].initiated)
                continue;
            enemySpawner.SpawnEnemy(pack.enemyType, WorldManager.instance.paths[pathId]);
            waveEndTimer = 0;

            yield return new WaitForSeconds(1f / pack.spawnRate);
        }
    }


    public void TestSpawnEnemy(EnemyType enemyType)
    {
        int pathId = Random.Range(0, WorldManager.instance.nPaths);
        enemySpawner.SpawnEnemy(enemyType, WorldManager.instance.paths[pathId]);
    }


    private void OnDestroy()
    {
        LevelManager.OnGameStart -= StartWaves;
        LevelManager.OnGameCompleted -= LevelCompleted;

        foreach (Wave wave in waves)
        {
            foreach (Pack pack in wave.packs)
            {
                pack.spawned = false;
            }
        }
    }

}
