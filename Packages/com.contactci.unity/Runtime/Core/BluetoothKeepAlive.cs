using System.Runtime.InteropServices;
using UnityEngine;

namespace Maestro
{
    public class BluetoothKeepAlive : MonoBehaviour
    {
#if UNITY_EDITOR_WIN
        [DllImport("WinRTDLL")]
        public static extern bool stop_watcher();

        [DllImport("WinRTDLL")]
        public static extern bool is_left_connected();

        [DllImport("WinRTDLL")]
        public static extern bool is_right_connected();

        [DllImport("WinRTDLL")]
        public static extern void force_disconnect_left();

        [DllImport("WinRTDLL")]
        public static extern void force_disconnect_right();

        [Tooltip("Keep Bluetooth connections and detection while Editor is running. If false, disconnect gloves on scene quit.")]
        public bool shouldKeepBluetoothRunning;

        void OnApplicationQuit()
        {
            if (shouldKeepBluetoothRunning)
                return;

            Debug.Log("Stopping BLE");

            if (!stop_watcher())
                Debug.LogWarning("Failed to stop Bluetooth advertisement watcher.");

            Debug.Log("Disconnecting left glove.");
            force_disconnect_left();

            Debug.Log("Disconnecting right glove.");
            force_disconnect_right();
        }
#endif
    }
}