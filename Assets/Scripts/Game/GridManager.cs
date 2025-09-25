using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GridManager : MonoBehaviour
{
    private int gridWidth;
    private int gridHeight;
    private int maxAllowed;

    public GameObject slotPrefab;  // empty slot placeholder (Image, can be invisible)
    public GameObject cardPrefab;  // the actual card prefab
    public Transform gridParent;

    private List<int> freeSlots = new List<int>();
    private Transform[] slotTransforms;  // store references to all slot placeholders
    private GameObject[] cardInstances;

    private Card selectedCard = null;

    [SerializeField] private List<Sprite> cardSprites;
    private Dictionary<(CardType, CardVariant), Sprite> spriteMap;

    //SCORE
    public TextMeshProUGUI scoreText;
    private int score = 0;

    //GAME OVER
    public SceneSwitcher sceneSwitcher;
    public GameObject gameOverPanel;
    public TextMeshProUGUI currentHighScoreText;
    public GameObject newHighScoreText;

    private Coroutine spawnRoutine;
    private bool isPaused = false;
    private bool gameOver = false;

    [SerializeField] private GameObject settingsPanel;

    public Slider musicVolumeSlider;
    public Toggle musicVolumeToggle;
    public Slider SFXVolumeSlider;
    public Toggle SFXVolumeToggle;
    [SerializeField] private AudioClip grabSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip highscoreSound;


    void Awake()
    {
        // Initialize sprite map (same as before)
        spriteMap = new Dictionary<(CardType, CardVariant), Sprite>
        {
            { (CardType.Teddy, CardVariant.toy), cardSprites[0] },
            { (CardType.Teddy, CardVariant.box), cardSprites[1] },
            { (CardType.Doll, CardVariant.toy), cardSprites[2] },
            { (CardType.Doll, CardVariant.box), cardSprites[3] },
            { (CardType.Clown, CardVariant.toy), cardSprites[4] },
            { (CardType.Clown, CardVariant.box), cardSprites[5] },
            { (CardType.Blocks, CardVariant.toy), cardSprites[6] },
            { (CardType.Blocks, CardVariant.box), cardSprites[7] },
            { (CardType.JackBox, CardVariant.toy), cardSprites[8] },
            { (CardType.JackBox, CardVariant.box), cardSprites[9] },
            { (CardType.BlocksB, CardVariant.toy), cardSprites[10] },
            { (CardType.BlocksB, CardVariant.box), cardSprites[11] },
            { (CardType.Train, CardVariant.toy), cardSprites[12] },
            { (CardType.Train, CardVariant.box), cardSprites[13] },
            { (CardType.Car, CardVariant.toy), cardSprites[14] },
            { (CardType.Car, CardVariant.box), cardSprites[15] },
        };
    }

    void Start()
    {
        gridWidth = GameSettings.GridWidth;
        gridHeight = GameSettings.GridHeight;
        maxAllowed = GameSettings.MaxAllowed;

        string gameCode="Gametype_" + GameSettings.GridHeight.ToString() + "rows" + GameSettings.MaxAllowed.ToString() + "cards";


        currentHighScoreText.text= LocalizationManager.Instance.GetLocalizedValue("highscore")+": "+ PlayerPrefs.GetInt(gameCode, 0).ToString();

        musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        if (AudioManager.Instance != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.GetCurrentVolume();
            musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

            musicVolumeToggle.onValueChanged.AddListener(isChecked => AudioManager.Instance.ToggleMusic(!isChecked));
            musicVolumeToggle.isOn = !AudioManager.Instance.IsMusicOn();

            SFXVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            SFXVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);

            SFXVolumeToggle.onValueChanged.AddListener(isChecked => AudioManager.Instance.ToggleSFX(!isChecked));
            SFXVolumeToggle.isOn = !AudioManager.Instance.IsSFXOn();
        }

        int totalSlots = gridWidth * gridHeight;
        Debug.Log(gridHeight+" "+gridWidth);
        slotTransforms = new Transform[totalSlots];
        cardInstances = new GameObject[totalSlots];

        // Fill grid with empty slots
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, gridParent);
            slotTransforms[i] = slot.transform;
            freeSlots.Add(i);
        }

        spawnRoutine = StartCoroutine(SpawnCardsEverySecond());
    }

    float CalculateSpawnInterval()
    {
        if (score < 20)
        {
            // Linear decrease from 2.0 to 1.5 seconds as score goes from 0 to 20
            float value = 2.0f - (0.5f / 20f) * score;
            Debug.Log("Linear interval: " + value);
            return value;
        }
        else
        {
            // Logarithmic decay from 1.5 to 0.8 seconds starting at score 20
            float adjustedScore = score - 20;

            // Use a formula that starts at 1.5 when adjustedScore=0, approaches 0.8 as adjustedScore -> infinity:
            float value = 0.8f + (1.5f - 0.8f) / (1f + Mathf.Log10(adjustedScore + 1));

            Debug.Log("Logarithmic interval: " + value);
            return value;
        }
    }

    IEnumerator SpawnCardsEverySecond()
    {
        while (true)
        {
            if (isPaused)
            {
                yield return null; // wait until unpaused
                continue;
            }
            if (freeSlots.Count == 0)
            {


                string gameCode = "Gametype_"+GameSettings.GridHeight.ToString()+"rows"+GameSettings.MaxAllowed.ToString()+"cards";
                //Debug.Log(gameCode);

                int storedHighScore = PlayerPrefs.GetInt(gameCode, 0);
                Debug.Log("storedhighscore for " + gameCode + " =" + storedHighScore);

                if (score > storedHighScore)
                {
                    newHighScoreText.SetActive(true);
                    PlayerPrefs.SetInt(gameCode, score);
                    PlayerPrefs.Save();
                    Debug.Log("New high score saved: " + score);
                    AudioManager.Instance.PlaySoundEffect(highscoreSound);

                    _ = FirestoreScoreManager.Instance.SaveScoreGeneric(score, gameCode, false);
                    // _ = FirestoreScoreManager.Instance.SaveWeeklyScore(score, gameCode);
                }
                else{
                    Debug.Log("No new high score: " + score);
                    AudioManager.Instance.PlaySoundEffect(gameOverSound);
                }

                //_ = FirestoreScoreManager.Instance.SaveScore(score, gameCode);
                _ = FirestoreScoreManager.Instance.SaveScoreGeneric(score, gameCode, true);



                gameOver = true;
                Debug.Log("Game Over — no slots available.");

                if (gameOverPanel != null)
                    gameOverPanel.SetActive(true);

                yield break;
            }

            SpawnCardInRandomSlot();
            yield return new WaitForSeconds(CalculateSpawnInterval());
        }
    }

    public void PauseSpawning()
    {
        isPaused = true;
    }

    public void ResumeSpawning()
    {
        isPaused = false;
    }

    void SpawnCardInRandomSlot()
    {
        int randomIndex = Random.Range(0, freeSlots.Count);
        int slotIndex = freeSlots[randomIndex];
        freeSlots.RemoveAt(randomIndex);

        Transform slotTransform = slotTransforms[slotIndex];

        // Create card as child of slot
        GameObject cardGO = Instantiate(cardPrefab, slotTransform);
        cardGO.transform.localPosition = Vector3.zero;
        cardGO.transform.localRotation = Quaternion.identity;
        cardGO.transform.localScale = Vector3.one;

        cardInstances[slotIndex] = cardGO;

        int gameUnlockedTypes = Mathf.Min(2 + score / 8, System.Enum.GetNames(typeof(CardType)).Length);
        int maxUnlockedTypes = Mathf.Min(gameUnlockedTypes, maxAllowed);

        CardType type = (CardType)Random.Range(0, maxUnlockedTypes);

        CardVariant variant = (CardVariant)Random.Range(0, 2);
        Sprite sprite = spriteMap[(type, variant)];

        Card card = cardGO.GetComponent<Card>();
        card.Initialize(type, variant, sprite, slotIndex, this);

        Button btn = cardGO.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => card.OnClick());
    }


    public void FreeSlot(int slotIndex)
    {
        if (cardInstances[slotIndex] != null)
        {
            Destroy(cardInstances[slotIndex]);
            cardInstances[slotIndex] = null;

            freeSlots.Add(slotIndex);
        }
    }

    public void HandleCardClick(Card currentCard)
    {
        if (selectedCard == null)
        {
            // First card selected
            AudioManager.Instance.PlaySoundEffect(grabSound);
            selectedCard = currentCard;
            Debug.Log("First card selected.");
            return;
        }

        // Compare with the previous card
        if (selectedCard != currentCard &&
            selectedCard.type == currentCard.type &&
            selectedCard.variant != currentCard.variant &&
            !gameOver)
        {
            AudioManager.Instance.PlaySoundEffect(placeSound);
            Debug.Log($"Match found! {selectedCard.type} - {selectedCard.variant} + {currentCard.variant}");

            int firstIndex = selectedCard.slotIndex;
            int secondIndex = currentCard.slotIndex;

            Destroy(cardInstances[firstIndex]);
            Destroy(cardInstances[secondIndex]);

            cardInstances[firstIndex] = null;
            cardInstances[secondIndex] = null;

            freeSlots.Add(firstIndex);
            freeSlots.Add(secondIndex);

            score++;
            UpdateScoreUI();

            // Only reset if match was found
            selectedCard = null;
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect(grabSound);
            Debug.Log("Cards cant be collected");
            // Save the most recent card as the new selected one
            selectedCard = currentCard;
        }
    }


    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }


}

