using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[CreateAssetMenu()]
public class MonsterSpawnData : ScriptableObject
{
    [Serializable]
    public class MonsterType
    {
        public string Name;
        public MonsterEntity Prefab;
        [Min(10)]
        public int Points = 10;
    }

    [SerializeField]
    private List<MonsterType> _monsters;
    private int _cheapestMonster;

    private void OnValidate()
    {
        _cheapestMonster = int.MaxValue;
        foreach(MonsterType type in _monsters)
        {
            if(type == null) continue;
            if (type.Points < _cheapestMonster) _cheapestMonster = type.Points;
        }
    }

    public void SpawnMonsters(Vector3 position, Transform parent, int points)
    {
        if (_monsters.Count == 0 | _cheapestMonster > points) return;

        // Filter monsters based on the given point cost
        List<MonsterType> validMonsters = _monsters.Where(monster => monster.Points <= points).ToList();

        // Calculate probabilities based on point costs
        int totalPoints = validMonsters.Sum(monster => monster.Points);
        List<int> probabilities = validMonsters.Select(monster => monster.Points).ToList();

        // Randomly select monsters based on probabilities until the total point cost is reached
        List<MonsterType> selectedMonsters = new List<MonsterType>();
        int remainingPoints = points;

        while (remainingPoints > _cheapestMonster & remainingPoints > points / 5 & validMonsters.Count > 0)
        {
            int index;
            int rand = UnityEngine.Random.Range(0, totalPoints);
            int cumulativeProbability = 0;

            for (index = 0; index < probabilities.Count; index++)
            {
                cumulativeProbability += probabilities[index];

                if (rand <= cumulativeProbability)
                    break;
            }

            // Retrieve the chosen monster
            MonsterType chosenMonster = validMonsters[index];

            // Check if adding this monster exceeds the remaining points
            if (chosenMonster.Points <= remainingPoints)
            {
                selectedMonsters.Add(chosenMonster);
                remainingPoints -= chosenMonster.Points;
            }
        }

        foreach(MonsterType monsterType in selectedMonsters) 
        {
            MonsterEntity monster = GameObject.Instantiate(monsterType.Prefab.gameObject, parent).GetComponent<MonsterEntity>();

            monster.SpawnAt(position);
        }
    }
}
