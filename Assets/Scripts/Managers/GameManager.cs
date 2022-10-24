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

[System.Serializable]
public class TrapContainer
{
    public DataTrap Data;
    public GameObject Trap;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region TRAPS
    [Title("Traps")]
    public List<TrapContainer> Traps = new();
    #endregion

    #region Struct Horde
    [Title("Hordes")]
    public List<Transform> RespawnLocations = new List<Transform>();
   
    public List<EnemySpawnByHorde> EnemySpawnByHordeList;
    #endregion

    #region Game Variables 
    [Title("Game Variables")]
    public static Action OnStartingGame;
    public int Lives = 30;
    public TextMeshProUGUI LivesText;
    public int Money = 3000;
    public TextMeshProUGUI MoneyText;
    public Transform EnemyGoal;
    public PlayerController Player;

    [Title("Info Game")]
    [ReadOnly] public int CurrentEnemies = 0;
    [ReadOnly] public int CurrentHorde = 0;
    public TextMeshProUGUI CurrentHordeText;
    [ReadOnly] public int EnemiesKilled = 0;
    [ReadOnly] public int Score = 0;
    [ReadOnly] public Vector3 _positionPlayer;
    public Vector3 PlayerPosition { get => _positionPlayer; }
    private bool _alreadyStarted = false;
    private bool _alreadyFinished = false;
    #endregion


    #region Sounds
    [Title("Sounds")]
    public AudioClip AmbienceSound;
    public AudioClip NewHordeSound;
    public AudioSource AmbienceAudioSource = new AudioSource();
    public AudioSource HordeAudioSource = new AudioSource();
    #endregion

    #region Final Canvas 

    [Title("Canvas")]
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
        AmbienceAudioSource?.Play();
        
        NewHorde();
    }

    public void EnemyDie(Enemy type, int reward)
    {
        ++EnemiesKilled;
        //type if we wanted to know how many died of this type
        Score += reward;
        Money += reward;
        MoneyText.text = Money.ToString();

        --CurrentEnemies;

        CheckEndHorde();
    }

    public void CheckEndHorde()
    {
        if (CurrentEnemies != 0) return;

        NewHorde();
    }

    public void NewHorde()
    {
        HordeAudioSource?.Play();
        ++CurrentHorde;
        CurrentEnemies = 0;
        int length = EnemySpawnByHordeList.Count;
        for (int i = 0; i < length; ++i)
        {
            CurrentEnemies += EnemySpawnByHordeList[i].SpawnEnemies(EnemyGoal.position);
        }
        CurrentHordeText.text = CurrentHorde.ToString();
    }

    public void EndGame()
    {
        _alreadyFinished = true;
        Timing.RunCoroutine(EndGameCoroutine());
    }

    public void EnemyReachedGoal()
    {
        --Lives;
        LivesText.text = Lives.ToString();
        --CurrentEnemies;


        if (Lives > 0)
        {
            CheckEndHorde();
            return;
        }
        if(Lives < 0 && _alreadyFinished == false) EndGame();

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

    public void UpdatePositionPlayer(Vector3 pos) => _positionPlayer = pos;

    private void PlayerMoved(object sender, UxrAvatarMoveEventArgs e) => UpdatePositionPlayer(e.NewPosition);

    public TypeSurface GetValidSurfaces(TypeTrap type)
    {
        int length = Traps.Count;
        for (int i = 0; i < length; ++i)
        {
            if (Traps[i].Data.Type == type) return Traps[i].Data.Surface;
        }
        return TypeSurface.None;
    }

    public void CheckToPlace(TypeTrap type, Vector3 pos, Quaternion rot)
    {
        int length = Traps.Count;
        GameObject trap = null;
        int price = int.MaxValue;
        for (int i = 0; i < length; ++i)
        {
            if (Traps[i].Data.Type == type)
            {
                trap = Traps[i].Trap;
                price = Traps[i].Data.Price;
                break;
            }
        }
        if (trap == null) return;
        if (Money < price)
        {
            //TODO : Feedback
            return;
        }
        Money -= price;
        MoneyText.text = Money.ToString();
        PlaceTrap(trap, pos, rot);
    }

    public void PlaceTrap(GameObject trap, Vector3 pos, Quaternion rot)
    {
        // TODO : Feedback
        GameObject.Instantiate(trap, pos, rot);
    }

    
    #region UNITY

    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (Player == null) Player = FindObjectOfType<PlayerController>();
            if (AmbienceAudioSource) AmbienceAudioSource.clip = AmbienceSound;
            if (HordeAudioSource) HordeAudioSource.clip = NewHordeSound;

            if (EnemiesKilledText && EnemiesKT_Group == null) EnemiesKT_Group = EnemiesKilledText.GetComponent<CanvasGroup>();
            if (AmountKilledText && AmountK_Group == null) AmountK_Group = AmountKilledText.GetComponent<CanvasGroup>();
            if (ScoreText && Score_Group == null) Score_Group = ScoreText.GetComponent<CanvasGroup>();
            if (AmountScoreText && AmountS_Group == null) AmountS_Group = AmountScoreText.GetComponent<CanvasGroup>();
            if (ExitTimer && Timer_Group == null) Timer_Group = ExitTimer.GetComponent<CanvasGroup>();
        });
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
 
    }

    private void Start()
    {
        UxrManager.AvatarMoved += PlayerMoved;
        _positionPlayer = Player.transform.position;

        MoneyText.text = Money.ToString();
        CurrentHordeText.text = CurrentHorde.ToString();
        LivesText.text = Lives.ToString();
    }

    private void OnDestroy()
    {
        UxrManager.AvatarMoved -= PlayerMoved;
    }

    #endregion

#if UNITY_EDITOR
    [Button("NewHorde")]
    public void TestNewHorde() => NewHorde();

    [Button("SpawnOneEnemy")]
    public void TestSpawnOneEnemy()
    {
        List<Transform> randomSpawn = GameManager.Instance.RespawnLocations;
        var currentSpawn = randomSpawn[0];
        PoolEnemy_Manager.Instance.SpawnEnemy(Enemy.Ghoul, EnemyGoal.position, currentSpawn);
    }
#endif
}

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
        int howMany = AmountOfEnemies(GameManager.Instance.CurrentHorde);
        Timing.RunCoroutine(SpawnEnemiesCoroutine(posTarget, howMany));
        return howMany;
    }

    public IEnumerator<float> SpawnEnemiesCoroutine(Vector3 posTarget, int howMany)
    {
        int length = howMany;
        var randomSpawn = GameManager.Instance.RespawnLocations;
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
