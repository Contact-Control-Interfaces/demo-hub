using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public interface IGrabContext
    {
        /**
         * Stores current values from the MaestroInteractable
         */
        void Populate(MaestroInteractable interactable);

        /**
         * Applies these stored values to the MaestroInteractable
         */
        void Apply(MaestroInteractable interactable);
    }
}
