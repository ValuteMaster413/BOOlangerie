using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "";
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnChangeSceneButtonClicked()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] Target scene name is empty! Please set it in the Inspector when the scene is ready.");
        }
    }
}
