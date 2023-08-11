using System.Collections.Generic;
using UnityEngine;

public class CenterPanelManager : MonoBehaviour
{
    public static CenterPanelManager Instance { get; private set; }

    public GameObject HandPanelPrefab;
    public float DistanceFromFace = 0.5f;
    public int SmoothingHistory = 25; // how many FixedUpdate frames to use for smoothing

    private GameObject panelInstance;
    private PanelText panelText;
    private List<Vector3> positionHistory;

    private bool IgnoreFurtherChanges = false;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;

            panelInstance = Instantiate(HandPanelPrefab);
            panelInstance.SetActive(false);

            panelText = panelInstance.GetComponentInChildren<PanelText>();
            positionHistory = new List<Vector3>(SmoothingHistory);

            panelInstance.SetActive(false);

        } else {
            // there already is a manager
            this.enabled = false;
            Destroy(this);
        }
    }

    private void SetText(string text, bool ignoreFurther = false)
    {
        if (IgnoreFurtherChanges) return;

        if (text == null || text == string.Empty) {
            ClearPanel();
        } else {
            if (!panelInstance.activeInHierarchy) {
                panelInstance.SetActive(true);
            }

            panelText.SetText(text);
        }

        IgnoreFurtherChanges |= ignoreFurther;
    }

    private void ClearPanel()
    {
        if (IgnoreFurtherChanges) return;

        panelText.SetText(string.Empty);
        panelInstance.SetActive(false);
    }

    private void RecordPosition(Vector3 position)
    {
        positionHistory.Add(position);
        while (positionHistory.Count > SmoothingHistory)
            positionHistory.RemoveAt(0);
    }

    private Vector3 getAverage(List<Vector3> hist)
    {
        if (hist.Count == 0)
            return Vector3.zero;

        Vector3 result = Vector3.zero;

        foreach (Vector3 v in hist)
            result += v;

        result /= hist.Count;

        return result;
    }

    public static void SetPanelText(string text, bool ignoreFurther = false)
    {
        Instance.SetText(text, ignoreFurther);
    }

    public static void ClearPanelText()
    {
        Instance.ClearPanel();
    }

    private void FixedUpdate()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 panelPosition = cameraTransform.position + cameraTransform.forward * DistanceFromFace;
        RecordPosition(panelPosition);
        Quaternion panelRotation = Quaternion.LookRotation(-cameraTransform.forward, Vector3.up);
        panelInstance.transform.SetPositionAndRotation(getAverage(positionHistory), panelRotation);
    }
}
