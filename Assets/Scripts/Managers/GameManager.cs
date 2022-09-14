using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MEC;
using System;
using UltimateXR.Core;
using UltimateXR.Avatar;

public class GameManager : Singleton<GameManager>
{
    #region Struct Horde

    [System.Serializable]
    public class EnemySpawnByHorde
    {
        public Enemy Type;
        public int HowMany;
        public int PerHorde;
        public float DelayBetweenSpawn;

        public EnemySpawnByHorde()
        {
            Type = Enemy.Ghoul;
            HowMany = 5;
            PerHorde = 1;
            DelayBetweenSpawn = 1f;
        }

        public int AmountOfEnemies(int currentHorde) => HowMany * (currentHorde / PerHorde);

        public int SpawnEnemies(Vector3 posTarget)
        {
            int howMany = AmountOfEnemies(GameManager.Instance._currentHorde);
            Timing.RunCoroutine(SpawnEnemiesCoroutine(posTarget, howMany));
            return howMany;
        }
        public IEnumerator<float> SpawnEnemiesCoroutine(Vector3 posTarget, int howMany)
        {
            int length = howMany;
            List<Transform> randomSpawn = GameManager.Instance.GetPossibleRespawns();
            Transform currentSpawn;
            int lengthSpawns = randomSpawn.Count;
            for (int i = 0; i < length; ++i)
            {
                currentSpawn = randomSpawn[i % lengthSpawns];
                PoolEnemy_Manager.Instance.SpawnEnemy(Type, posTarget, currentSpawn);
                yield return Timing.WaitForSeconds(DelayBetweenSpawn);
            }
            yield return 0f;
        }

    }
    public List<EnemySpawnByHorde> EnemySpawnByHordeList;
    #endregion

    #region Game Variables 
    public static Action OnStartingGame;
    public UxrAvatar RefPlayer;
    public List<Transform> RespawnLocations = new List<Transform>();
    public float DistanceBetweenRespawnNeeded = 200f;

    [SerializeField] private int _currentEnemies = 0;
    [SerializeField] private int _currentHorde = 0;
    [SerializeField] private int _enemiesKilled = 0;
    [SerializeField] private int _score = 0;
    [SerializeField] private Vector3 _positionPlayer;
    #endregion


    #region Sounds
    public AudioClip AmbienceSound;
    public AudioClip NewHordeSound;
    public AudioSource AmbienceAudioSource = new AudioSource();
    public AudioSource HordeAudioSource = new AudioSource();
    #endregion
    private CoroutineHandle _coroutine;

    public void StartGame()
    {
        OnStartingGame?.Invoke();
        // Play AmbienceSound
        NewHorde();
    }

    public void EnemyDie(Enemy type, int reward)
    {
        ++_enemiesKilled;
        //type if we wanted to know how many died of this type
        _score += reward;

        --_currentEnemies;

        CheckEndHorde();
    }

    public void CheckEndHorde()
    {
        if (_currentEnemies != 0) return;

        NewHorde();
    }
    public void NewHorde()
    {
        // Play NewHordeSound
        ++_currentHorde;
        _currentEnemies = 0;
        int length = EnemySpawnByHordeList.Count;
        for (int i = 0; i < length; ++i)
        {
            _currentEnemies += EnemySpawnByHordeList[i].SpawnEnemies(RefPlayer.transform.position);
        }
    }

    public List<Transform> GetPossibleRespawns()
    {
        List<Transform> respawns = new List<Transform>();   
        int length = RespawnLocations.Count;
        for (int i = 0; i < length; i++)
        {
            if((_positionPlayer - RespawnLocations[i].position).sqrMagnitude 
                > DistanceBetweenRespawnNeeded)
            {
                respawns.Add(RespawnLocations[i]);
            }
        }
        return respawns;
    }

    public void UpdatePositionPlayer(Vector3 pos) => _positionPlayer = pos;
    private void PlayerMoved(object sender, UxrAvatarMoveEventArgs e) => UpdatePositionPlayer(e.NewPosition);

    #region UNITY

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (RefPlayer == null) RefPlayer = FindObjectOfType<UxrAvatar>();
            if (AmbienceAudioSource) AmbienceAudioSource.clip = AmbienceSound;
            if (HordeAudioSource) HordeAudioSource.clip = NewHordeSound;
        });
    }

    private void Start()
    {
        UxrManager.AvatarMoved += PlayerMoved;
        _positionPlayer = RefPlayer.transform.position;
    }

    private void OnDestroy()
    {
        UxrManager.AvatarMoved -= PlayerMoved;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(RefPlayer.transform.position, DistanceBetweenRespawnNeeded  * 0.01f);
    }
    #endregion

#if UNITY_EDITOR
    [Button("NewHorde")]
    public void TestNewHorde() => NewHorde();
#endif
}
