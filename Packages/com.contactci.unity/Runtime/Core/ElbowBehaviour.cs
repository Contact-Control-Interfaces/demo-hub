using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Maestro
{
    public class ElbowBehaviour : MonoBehaviour
    {
        public GameObject Shoulder;

        public void Update()
        {
            if (Shoulder)
            {
                gameObject.transform.LookAt(Shoulder.transform.position, transform.parent.up);
            }
        }
    }
}
