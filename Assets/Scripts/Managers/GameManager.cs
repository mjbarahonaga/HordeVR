using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MEC;
using System;
using UltimateXR.Core;
using UltimateXR.Avatar;
using TMPro;
using UltimateXR.Animation.UI;
using UnityEngine.SceneManagement;

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
    private bool _alreadyStarted = false;
    #endregion


    #region Sounds
    public AudioClip AmbienceSound;
    public AudioClip NewHordeSound;
    public AudioSource AmbienceAudioSource = new AudioSource();
    public AudioSource HordeAudioSource = new AudioSource();
    #endregion

    #region Final Canvas 
    
    [FoldoutGroup("FinalCanvas")] public Canvas CanvasContainer;
    [FoldoutGroup("FinalCanvas")] public TextMeshProUGUI EnemiesKilledText;
    [FoldoutGroup("FinalCanvas")] public TextMeshProUGUI AmountKilledText;
    [FoldoutGroup("FinalCanvas")] public TextMeshProUGUI ScoreText;
    [FoldoutGroup("FinalCanvas")] public TextMeshProUGUI AmountScoreText;
    [FoldoutGroup("FinalCanvas")] public TextMeshProUGUI ExitTimer;

    [FoldoutGroup("FinalCanvas")] [SerializeField, ReadOnly] private CanvasGroup EnemiesKT_Group;
    [FoldoutGroup("FinalCanvas")] [SerializeField, ReadOnly] private CanvasGroup AmountK_Group;
    [FoldoutGroup("FinalCanvas")] [SerializeField, ReadOnly] private CanvasGroup Score_Group;
    [FoldoutGroup("FinalCanvas")] [SerializeField, ReadOnly] private CanvasGroup AmountS_Group;
    [FoldoutGroup("FinalCanvas")] [SerializeField, ReadOnly] private CanvasGroup Timer_Group;
    #endregion
    private CoroutineHandle _coroutine;

    public void StartGame()
    {
        if (_alreadyStarted) return;
        _alreadyStarted = true;
        OnStartingGame?.Invoke();
        AmbienceAudioSource.Play();
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
        HordeAudioSource.Play();
        ++_currentHorde;
        _currentEnemies = 0;
        int length = EnemySpawnByHordeList.Count;
        for (int i = 0; i < length; ++i)
        {
            _currentEnemies += EnemySpawnByHordeList[i].SpawnEnemies(RefPlayer.transform.position);
        }
    }

    public void EndGame()
    {
        Timing.RunCoroutine(EndGameCoroutine());
    }

    public IEnumerator<float> EndGameCoroutine()
    {
        CanvasContainer.enabled = true;

        UxrCanvasAlphaTween.FadeIn(EnemiesKT_Group, 1.5f);
        yield return Timing.WaitForSeconds(1.5f);

        UxrCanvasAlphaTween.FadeIn(AmountK_Group, 1.5f);
        yield return Timing.WaitForSeconds(1.5f);

        UxrCanvasAlphaTween.FadeIn(Score_Group, 1.5f);
        yield return Timing.WaitForSeconds(1.5f);

        UxrCanvasAlphaTween.FadeIn(AmountS_Group, 1.5f);
        yield return Timing.WaitForSeconds(1.5f);

        UxrCanvasAlphaTween.FadeIn(Timer_Group, 0.5f);
        for (int i = 5; i > 0; --i)
        {
            ExitTimer.text = "Output in " + i.ToString();
            yield return Timing.WaitForSeconds(1);
        }

        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
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

            if (EnemiesKilledText && EnemiesKT_Group == null) EnemiesKT_Group = EnemiesKilledText.GetComponent<CanvasGroup>();
            if (AmountKilledText && AmountK_Group == null) AmountK_Group = AmountKilledText.GetComponent<CanvasGroup>();
            if (ScoreText && Score_Group == null) Score_Group = ScoreText.GetComponent<CanvasGroup>();
            if (AmountScoreText && AmountS_Group == null) AmountS_Group = AmountScoreText.GetComponent<CanvasGroup>();
            if (ExitTimer && Timer_Group == null) Timer_Group = ExitTimer.GetComponent<CanvasGroup>();
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
