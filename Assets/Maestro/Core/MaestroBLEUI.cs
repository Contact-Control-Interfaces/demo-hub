using Leap.Unity;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MaestroBLEUI : MonoBehaviour
{
    private static MaestroBLEUI instance;

    public Transform watcher, left, right, process, leftConnected, rightConnected;

    private float elapsed;
    public float tick = 0.1f;

    [DllImport("MaestroAPI")]
    public static extern bool is_ble_processing();

    [DllImport("MaestroAPI")]
    public static extern bool is_ble_watcher_running();

    [DllImport("MaestroAPI")]
    public static extern bool is_ble_left_connecting();

    [DllImport("MaestroAPI")]
    public static extern bool is_ble_right_connecting();

    public int history = 5;
    private List<Vector3> rightHistory, leftHistory;

    private MaestroHand rightHand, leftHand;
    private MaestroGloveBehaviour rightGlove, leftGlove;
    private Camera mainCamera;

    private GameObject leftPanel, rightPanel;
    private TextMesh leftText, rightText;

    public bool showPanels = true;
    private bool overrideRightText = false, overrideLeftText = false;
    public Vector3 panelOffset = new Vector3(0, 0.2f, 0);
    public GameObject panelPrefab;

    private int ellipsisTicks = 2, currentTicks = 0, currentEllipsis = 3;

    public static void SetLeftText(string text)
    {
        instance.overrideLeftText = true;
        SetText(text, instance.leftText);
    }

    public static void SetRightText(string text)
    {
        instance.overrideRightText = true;
        SetText(text, instance.rightText);
    }

    private static void SetText(string text, TextMesh panel)
    {
        if (panel != null)
            panel.text = text;
    }

    #region Getters/setters based on handedness

    private MaestroHand GetHand(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return rightHand;
            case WhichHand.LeftHand:
                return leftHand;
        }
    }

    private MaestroGloveBehaviour GetGlove(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return rightGlove;
            case WhichHand.LeftHand:
                return leftGlove;
        }
    }

    private GameObject GetPanel(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return rightPanel;
            case WhichHand.LeftHand:
                return leftPanel;
        }
    }

    private bool GetOverride(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return overrideRightText;
            case WhichHand.LeftHand:
                return overrideLeftText;
        }
    }

    private List<Vector3> GetHistory(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return rightHistory;
            case WhichHand.LeftHand:
                return leftHistory;
        }
    }

    private string GetName(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return "Right";
            case WhichHand.LeftHand:
                return "Left";
        }
    }

    private string GetDisplayText(WhichHand handedness)
    {
        return string.Format("Please wait{0}\r\n{1} Connecting", getEllipsis(currentEllipsis), GetName(handedness));
    }

    private TextMesh GetTextMesh(WhichHand handedness)
    {
        switch (handedness) {
            default:
            case WhichHand.RightHand:
                return rightText;
            case WhichHand.LeftHand:
                return leftText;
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;

        leftHistory = new List<Vector3>();
        rightHistory = new List<Vector3>();

        watcher.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        process.gameObject.SetActive(false);
        leftConnected.gameObject.SetActive(false);
        rightConnected.gameObject.SetActive(false);

        // Find active left/right hands
        MaestroHand[] hands = GameObject.FindObjectsOfType<MaestroHand>();
        foreach (MaestroHand hand in hands) {
            if (hand.whichHand == WhichHand.LeftHand) {
                if (leftHand == null)
                    leftHand = hand;
                else
                    continue;
            } else {
                if (rightHand == null)
                    rightHand = hand;
                else
                    continue;
            }
        }

        // Get gloves
        if (rightHand != null) {
            rightGlove = rightHand.gameObject.GetComponentInParent<MaestroGloveBehaviour>();
            TryAddEnableDisable(rightGlove);
        }

        if (leftHand != null) {
            leftGlove = leftHand.gameObject.GetComponentInParent<MaestroGloveBehaviour>();
            TryAddEnableDisable(leftGlove);
        }

        // Find main camera
        mainCamera = GameObject.FindObjectOfType<Camera>();
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");

        // Spawn panels
        leftPanel = Instantiate(panelPrefab);
        rightPanel = Instantiate(panelPrefab);

        // Get Text meshes
        leftText = leftPanel.GetComponentInChildren<TextMesh>();
        leftText.text = GetDisplayText(WhichHand.LeftHand);
        rightText = rightPanel.GetComponentInChildren<TextMesh>();
        rightText.text = GetDisplayText(WhichHand.RightHand);

        // Init history
        RecordHistory(WhichHand.RightHand);
        RecordHistory(WhichHand.LeftHand);

        // Init panel positions
        OrientPanel(WhichHand.RightHand);
        OrientPanel(WhichHand.LeftHand);
    }

    private void TryAddEnableDisable(MaestroGloveBehaviour toAddTo)
    {
        if (toAddTo != null && toAddTo.addHandEnableDisable) {
            toAddTo.gameObject.AddComponent<HandEnableDisable>();
        }
    }

    private void RecordHistory(WhichHand handedness)
    {
        List<Vector3> list = GetHistory(handedness);
        MaestroHand hand = GetHand(handedness);
        if (list != null && hand != null) {
            list.Add(hand.Palm.transform.position);
            while (list.Count > history)
                list.RemoveAt(0);
        }
    }

    private void UpdateUI()
    {
        watcher.gameObject.SetActive(is_ble_watcher_running());
        left.gameObject.SetActive(is_ble_left_connecting());
        right.gameObject.SetActive(is_ble_right_connecting());
        process.gameObject.SetActive(is_ble_processing());

        leftConnected.gameObject.SetActive(leftGlove != null ? leftGlove.Connected : false);
        rightConnected.gameObject.SetActive(rightGlove != null ? rightGlove.Connected : false);
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= tick) {
            elapsed = 0f;
            currentTicks++;
            if (currentTicks > ellipsisTicks) {
                currentEllipsis++;
                if (currentEllipsis > 3)
                    currentEllipsis = 0;
                currentTicks = 0;
            }

            UpdateUI();
        }

        // Always update panels
        if (showPanels || overrideRightText || overrideLeftText) {

            RecordHistory(WhichHand.RightHand);
            RecordHistory(WhichHand.LeftHand);

            OrientPanel(WhichHand.RightHand);
            OrientPanel(WhichHand.LeftHand);

            string ellipsis = getEllipsis(currentEllipsis);

            UpdateForHandedness(WhichHand.RightHand);
            UpdateForHandedness(WhichHand.LeftHand);
        }
    }

    private void UpdateForHandedness(WhichHand handedness)
    {
        MaestroGloveBehaviour glove = GetGlove(handedness);
        if (glove != null) {
            bool connected = glove.Connected;
            bool overrideText = GetOverride(handedness);

            GetPanel(handedness).SetActive((showPanels && !connected) || overrideText);
            if (!connected && !overrideText)
                GetTextMesh(handedness).text = GetDisplayText(handedness);
        }
    }

    private string getEllipsis(int num)
    {
        return "".PadRight(num, '.');
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

    //private void OrientPanel(GameObject panel, Vector3 position)
    private void OrientPanel(WhichHand handedness)
    {
        GameObject panel = GetPanel(handedness);
        if (panel != null && mainCamera != null) {
            Vector3 position = getAverage(GetHistory(handedness));
            panel.transform.position = position + 0.2f * mainCamera.transform.up;
            panel.transform.rotation = Quaternion.LookRotation(mainCamera.transform.position - panel.transform.position);
        }
    }
}
