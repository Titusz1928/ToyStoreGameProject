using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject gameMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject globalRecordsPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private SceneSwitcher SceneManager;
    [SerializeField] private GridManager gridManager;


    // Countdown UI for confirmation panel
    [SerializeField] private GameObject gameMenuCountdownObject;
    [SerializeField] private TMPro.TextMeshProUGUI gameMenuCountdownText;


    private bool isPaused = false;
    private GameObject activePausePanel = null;
    private GameObject activeCountdownObject = null;
    private TMPro.TextMeshProUGUI activeCountdownText = null;

    public void OpenGameMenuPanel()
    {
        gameMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        activePausePanel = gameMenuPanel;
        activeCountdownObject = gameMenuCountdownObject;
        activeCountdownText = gameMenuCountdownText;

        ApplyPauseState();
    }

    public void OpenSettingsPanel()
    {
        gameMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenGlobalRecordsPanel()
    {
        globalRecordsPanel.SetActive(true);
    }

    public void CloseGlobalRecordsPanel()
    {
        globalRecordsPanel.SetActive(false);
    }



    private void ApplyPauseState()
    {
        CanvasGroup group = mainPanel.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (gridManager != null)
            gridManager.PauseSpawning();

        isPaused = true;
    }

    public void ResumeGameAfterDelay()
    {
        StartCoroutine(ResumeAfterDelay(3f));
    }

    private IEnumerator ResumeAfterDelay(float delay)
    {
        int seconds = Mathf.CeilToInt(delay);

        if (activeCountdownObject != null)
            activeCountdownObject.SetActive(true);

        while (seconds > 0)
        {
            if (activeCountdownText != null)
                activeCountdownText.text = seconds.ToString();

            Debug.Log("Countdown: " + seconds);
            yield return new WaitForSeconds(1f);
            seconds--;
        }

        if (activeCountdownObject != null)
            activeCountdownObject.SetActive(false);

        if (activePausePanel != null)
            activePausePanel.SetActive(false);

        CanvasGroup group = mainPanel.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (gridManager != null)
            gridManager.ResumeSpawning();

        isPaused = false;

        // Reset active references
        activePausePanel = null;
        activeCountdownObject = null;
        activeCountdownText = null;
    }

    public void ConfirmExit()
    {
        SceneManager.LoadSceneByIndex(0);
    }
}
