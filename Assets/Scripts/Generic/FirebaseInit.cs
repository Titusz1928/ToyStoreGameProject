using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseInit Instance;

    public static FirebaseAuth Auth;
    public static FirebaseUser User;
    public static FirebaseFirestore Db;

    private async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            return;
        }

        Auth = FirebaseAuth.DefaultInstance;
        Db = FirebaseFirestore.DefaultInstance;

        // If user already signed in before, reuse it
        if (Auth.CurrentUser != null)
        {
            User = Auth.CurrentUser;
            Debug.Log("Reusing Firebase user: " + User.UserId);
            return;
        }

        try
        {
            var result = await Auth.SignInAnonymouslyAsync();
            User = result.User;

            Debug.Log("Signed in as: " + User.UserId);

            PlayerPrefs.SetString("firebase_userid", User.UserId);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Anonymous sign-in failed: " + e);
        }
    }

    public void ResetFirebaseDataAndQuit()
    {
        Debug.Log("Clearing Firebase PlayerPrefs and quitting...");

        // Remove only the Firebase User ID
        PlayerPrefs.DeleteKey("firebase_userid");
        PlayerPrefs.Save();

        // Optionally sign out from Firebase
        if (Auth != null)
        {
            Auth.SignOut();
        }

        // Quit the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // stop play mode in editor
#else
    Application.Quit();
#endif
    }
}
