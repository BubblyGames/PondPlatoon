using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyLibrary
{
    [SerializeField]
    public EnemyEntry[] enemyPrefabs;


    public GameObject GetPrefabByIdentificator(string identificator)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].enemyId == identificator)
            {
                return enemyPrefabs[i].enemyPrefab;
            }
        }
        Debug.LogError("Can't find a Enemy with this identificator: " + identificator);
        return null;
    }
}

[System.Serializable]
public class EnemyEntry
{
    public string enemyId;
    public GameObject enemyPrefab;
}