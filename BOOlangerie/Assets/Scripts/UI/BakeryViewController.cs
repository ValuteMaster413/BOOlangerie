using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BakeryViewController : MonoBehaviour
{
    [Header("Stations UI")]
    [SerializeField] private GameObject counterStationUI;
    [SerializeField] private GameObject prepStationUI;

    [Header("Navigation Buttons")]
    [SerializeField] private Button goToPrepButton;
    [SerializeField] private Button goToCounterButton;

    private bool isSwitching = false;

    private void Awake()
    {
        if (goToPrepButton != null)
            goToPrepButton.onClick.AddListener(ShowPrepStation);

        if (goToCounterButton != null)
            goToCounterButton.onClick.AddListener(ShowCounterStation);
    }

    private void Start()
    {
        if (counterStationUI != null) counterStationUI.SetActive(true);
        if (prepStationUI != null) prepStationUI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (goToPrepButton != null)
            goToPrepButton.onClick.RemoveListener(ShowPrepStation);

        if (goToCounterButton != null)
            goToCounterButton.onClick.RemoveListener(ShowCounterStation);
    }

    public void ShowPrepStation()
    {
        if (isSwitching) return;
        StartCoroutine(SwitchStationRoutine(counterStationUI, prepStationUI));
    }

    public void ShowCounterStation()
    {
        if (isSwitching) return;
        StartCoroutine(SwitchStationRoutine(prepStationUI, counterStationUI));
    }

    private IEnumerator SwitchStationRoutine(GameObject currentStation, GameObject targetStation)
    {
        isSwitching = true;
        
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.StartCoroutine("FadeOut");
            
            if (currentStation != null) currentStation.SetActive(false);
            if (targetStation != null) targetStation.SetActive(true);
            
            yield return SceneTransitionManager.Instance.StartCoroutine("FadeIn");
        }
        else
        {
            if (currentStation != null) currentStation.SetActive(false);
            if (targetStation != null) targetStation.SetActive(true);
        }

        isSwitching = false;
    }
}