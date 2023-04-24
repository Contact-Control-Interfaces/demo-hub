using Maestro;
using Maestro.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static GloveSelect;

namespace Maestro
{
    public static class ConfigSettings
    {
        public static bool treeActive;
        public static bool configCompleted = false;
        public static DemoMode demoMode;

        public static void AppleDemoStart(GameObject tree, GameObject configPanel)
        {
            treeActive = false;

            if (configCompleted)
            {
                configPanel.SetActive(false);
                ActivateTree(tree);
            }
            else
            {
                configPanel.SetActive(true);
            }
        }

        public static void CloseConfig(GameObject configPanel)
        {
            configPanel.SetActive(false);
            configCompleted = true;
        }

        public static void ActivateTree(GameObject tree)
        {
            if (treeActive)
            {
                return;
            }
            else
            {
                tree.SetActive(true);
                tree.GetComponent<TreeGrow>().GrowTree();
                treeActive = true;
            }
        }

        public static void DemoCheck(GameObject tree)
        {
            switch (demoMode)
            {
                case DemoMode.OneGlove:
                    if (MaestroGloveConnector.Instance.isLeftConnected() || MaestroGloveConnector.Instance.isRightConnected())
                    {
                        ActivateTree(tree);
                    }
                    break;

                case DemoMode.TwoGlove:
                    if (MaestroGloveConnector.Instance.isLeftConnected() && MaestroGloveConnector.Instance.isRightConnected())
                    {
                        ActivateTree(tree);
                    }
                    break;
            }
        }
    }
}
