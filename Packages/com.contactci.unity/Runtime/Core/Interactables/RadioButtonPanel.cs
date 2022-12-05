using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Maestro
{
    [System.Serializable]
    public class RadioButtonEvent : UnityEvent<int>
    {
    }

    /// <summary>
    /// Describes a panel of "radio" style buttons.
    /// Buttons are latched in the down position when active, and pop back up when reset.
    /// Only one button can be active at once. When a button is pressed, all other buttons reset.
    /// Some buttons may not latch, but will still reset other latched buttons.
    /// </summary>
    public class RadioButtonPanel : MonoBehaviour
    {
        [Tooltip("Buttons which belong to this panel.")]
        public List<RadioButtonBehavior> gangedButtons;

        public RadioButtonEvent onStateChanged;

        [Tooltip("Currently latched button. -1 indicates no latched button.")]
        public int latchedIndex = -1;


        private void Start()
        {
            foreach (var rad in gangedButtons)
            {
                rad.OnLatch += ButtonLatched;
                //if there is a default state, set it up here
                if (latchedIndex > -1 && rad.index == latchedIndex)
                    rad.Latch();
            }
        }
/*        void Awake()
        {
            foreach (var rad in gangedButtons)
            {
                rad.OnLatch += ButtonLatched;
                //if there is a default state, set it up here
                if (latchedIndex > -1 && rad.index == latchedIndex)
                    rad.Latch();
            }
        }*/

        private void ButtonLatched(int index)
        {
            onStateChanged?.Invoke(index);

            foreach (var rad in gangedButtons)
            {
                if(rad.index != index)
                    rad.ResetLatch();
            }
        }
    }
}