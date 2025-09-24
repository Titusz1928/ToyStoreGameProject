using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameOverPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button showScoresButton;            // The button that opens the high scores
    [SerializeField] private GlobalHighScoresPanel globalPanel;  // The panel prefab in the scene (can start inactive)

    private void Start()
    {
        if (showScoresButton == null || globalPanel == null)
        {
            Debug.LogError("Please assign ShowScoresButton and GlobalHighScoresPanel in the inspector!");
            return;
        }

        // Assign the button click dynamically
        showScoresButton.onClick.AddListener(OnShowScoresButtonClicked);
    }

    private void OnShowScoresButtonClicked()
    {
        // Fire-and-forget async
        _ = ShowGlobalScoresAsync();
    }

    private async Task ShowGlobalScoresAsync()
    {
        // Ensure panel is active
        globalPanel.gameObject.SetActive(true);

        // Build gameCode from current game settings
        string gameCode = $"Gametype_{GameSettings.GridHeight}rows{GameSettings.MaxAllowed}cards";
        Debug.Log($"Loading global highscores for {gameCode}");

        // Show scores
        await globalPanel.ShowScores(gameCode);
    }
}
