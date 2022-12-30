using UnityEngine;

namespace Maestro.Core.Utilities
{
    public class ReturnChildrenToSpawn : MonoBehaviour
    {
        public void PoofAll()
        {
            foreach(var c in GetComponentsInChildren<ReturnToSpawn>())
                c.Poof();
        }
    }
}