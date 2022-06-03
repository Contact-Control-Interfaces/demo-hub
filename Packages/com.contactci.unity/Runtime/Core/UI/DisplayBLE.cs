using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Maestro.UI
{
    public class DisplayBLE : MonoBehaviour
    {
        private static DisplayBLE instance;

        private float opacity = 0.5f;

        private float elapsed;
        public float tick = 0.25f;

        public int history = 5;
        private List<Vector3> rightHistory, leftHistory;

        private IMaestroHand rightHand, leftHand;
        private MaestroGloveBehaviour rightGlove, leftGlove;
        private Camera mainCamera;

        private GameObject leftPanel, rightPanel;
        private TextMesh leftText, rightText;

        private bool overrideRightText = false, overrideLeftText = false;
        public Vector3 panelOffset = new Vector3(0, 0.2f, 0);
        public GameObject panelPrefab;

        public bool showLeftPanel = true, showRightPanel = true;

        private string inputLeftPanel = null;
        private string inputRightPanel = null;

        private int ellipsisTicks = 2, currentTicks = 0, currentEllipsis = 3;

        public Image leftDot, rightDot;
        private bool blinkState;

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
        private IMaestroHand GetHand(WhichHand handedness)
        {
            return get(handedness, leftHand, rightHand);
        }

        private MaestroGloveBehaviour GetGlove(WhichHand handedness)
        {
            return get(handedness, leftGlove, rightGlove);
        }

        private GameObject GetPanel(WhichHand handedness)
        {
            return get(handedness, leftPanel, rightPanel);
        }

        private bool GetOverride(WhichHand handedness)
        {
            return get(handedness, overrideLeftText, overrideRightText);
        }

        private List<Vector3> GetHistory(WhichHand handedness)
        {
            return get(handedness, leftHistory, rightHistory);
        }

        private string GetName(WhichHand handedness)
        {
            return get(handedness, "Left", "Right");
        }

        private string GetDisplayText(WhichHand handedness)
        {
            return string.Format("Please wait{0}\r\n{1} Connecting", getEllipsis(currentEllipsis), GetName(handedness));
        }

        private TextMesh GetTextMesh(WhichHand handedness)
        {
            return get(handedness, leftText, rightText);
        }

        private Image getDot(WhichHand handedness)
        {
            return get(handedness, leftDot, rightDot);
        }

        private bool getBlinkState(WhichHand handedness)
        {
            return get(handedness, blinkState, !blinkState);
        }

        private T get<T>(WhichHand handedness, T leftOption, T rightOption) 
        {
            return handedness == WhichHand.LeftHand ? leftOption : rightOption;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
                instance = this;

            leftDot.color = Color.clear;
            rightDot.color = Color.clear;

            leftHistory = new List<Vector3>();
            rightHistory = new List<Vector3>();

            // Check if inputs exist
            CheckInput("ToggleLeftPanel", out inputLeftPanel);
            CheckInput("ToggleRightPanel", out inputRightPanel);
            showLeftPanel = true;
            showRightPanel = true;

            // Find active left/right hands
            IMaestroHand[] hands = GameObject.FindObjectsOfType<IMaestroHand>();
            if (hands.Length > 0) {
                if (leftHand == null)
                    leftHand = hands.FirstOrDefault(x => x.whichHand == WhichHand.LeftHand);
                if (rightHand == null)
                    rightHand = hands.FirstOrDefault(x => x.whichHand == WhichHand.RightHand);
            }

            // Get gloves
            if (rightHand != null) {
                rightGlove = rightHand.gameObject.GetComponentInParent<MaestroGloveBehaviour>();
            }

            if (leftHand != null) {
                leftGlove = leftHand.gameObject.GetComponentInParent<MaestroGloveBehaviour>();
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

        private void CheckInput(string input, out string key)
        {
            if (DoesInputExist(input))
                key = input;
            else {
                Debug.LogWarning(string.Format("Input [{0}] not bound!", input));
                key = null;
            }
        }

        private bool DoesInputExist(string input)
        {
            try {
                Input.GetButton(input);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private void RecordHistory(WhichHand handedness)
        {
            List<Vector3> list = GetHistory(handedness);
            IMaestroHand hand = GetHand(handedness);
            if (list != null && hand != null) {
                list.Add(hand.Palm.position);
                while (list.Count > history)
                    list.RemoveAt(0);
            }
        }

        private void UpdateUI()
        {
            blinkState = !blinkState;

            UpdateUI(WhichHand.LeftHand);
            UpdateUI(WhichHand.RightHand);
        }

        private Color fromColor(Color c)
        {
            return new Color(c.r, c.g, c.b, opacity);
        }

        private bool isBleProcessing()
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0 || UNITY_WINRT || UNITY_WINRT_10_0)
            return MaestroNativeWrapper.is_ble_processing();
#elif UNITY_ANDROID
            return MaestroAndroidWrapper.IsBleProcessing();
#else
            return false;
#endif
        }

        private void UpdateUI(WhichHand handedness)
        {
            Image dot = getDot(handedness);
            MaestroGloveBehaviour glove = GetGlove(handedness);

            if (glove.Connected) {
                dot.color = fromColor(Color.green);
            } else if (isBleProcessing()) {
                dot.color = fromColor(Color.magenta);
            } else {
                if (getBlinkState(handedness))
                    dot.color = fromColor(Color.blue);
                else
                    dot.color = fromColor(Color.red);
            }   
        }

        // Update is called once per frame
        void Update()
        {
            // Check input
            if (inputLeftPanel != null && Input.GetButtonDown(inputLeftPanel)) {
                showLeftPanel = !showLeftPanel;
            }

            if(inputRightPanel != null && Input.GetButtonDown(inputRightPanel)) {
                showRightPanel = !showRightPanel;
            }

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

            RecordHistory(WhichHand.RightHand);
            RecordHistory(WhichHand.LeftHand);

            OrientPanel(WhichHand.RightHand);
            OrientPanel(WhichHand.LeftHand);

            string ellipsis = getEllipsis(currentEllipsis);

            UpdateForHandedness(WhichHand.RightHand);
            UpdateForHandedness(WhichHand.LeftHand);
        }

        private void UpdateForHandedness(WhichHand handedness)
        {
            MaestroGloveBehaviour glove = GetGlove(handedness);
            if (glove != null) {
                bool connected = glove.Connected;
                bool overrideText = GetOverride(handedness);

                GetPanel(handedness).SetActive(showPanel(handedness) && (!connected || overrideText));
                if (!connected && !overrideText)
                    GetTextMesh(handedness).text = GetDisplayText(handedness);
            }
        }

        private bool showPanel(WhichHand handedness)
        {
            return handedness == WhichHand.RightHand ? showRightPanel : showLeftPanel;
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
}
