using Maestro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabMaterials : MonoBehaviour
{
    public GameObject otherCamera;
    public GameObject[] Interactables;
    public Collider[] colliders;
    private Dictionary<GameObject, Material> materialStorage = new Dictionary<GameObject, Material>();

    [SerializeField]
    private Material ghostShader;

    public void Start()
    {
        //To grab the children of a game object you need to grab the child's transform to reference to the actual game object
        Interactables = GetComponentsInChildren<Transform>(true).Select(o => o.gameObject).ToArray();
        colliders = GetComponentsInChildren<Collider>(true);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            ApplyGhostShader();

        if (Input.GetKeyDown(KeyCode.H))
            RemoveGhostShader();
    }

    public void GhostShaderState(GameObject child, Material childMaterial, bool returnToSpawnState, bool colliderState, bool storeMaterials = false)
    {
        if (child.TryGetComponent<ReturnToSpawn>(out ReturnToSpawn returnToSpawn))
            returnToSpawn.enabled = returnToSpawnState;

        if (child.TryGetComponent<Collider>(out Collider c))
            c.enabled = colliderState;

        if (childMaterial != null && child.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            if (storeMaterials)
                materialStorage[mr.gameObject] = mr.material;
            mr.material = childMaterial;
        }
    }

    public void ApplyGhostShader()
    {
        CameraToggle();
        foreach (GameObject child in Interactables) {
            GhostShaderState(child, ghostShader, false, false, true);
        }
    }

    public void RemoveGhostShader()
    {
        CameraToggle();
        foreach (GameObject child in Interactables) {
            bool hasStoredMaterial = materialStorage.TryGetValue(child.gameObject, out Material material);
            GhostShaderState(child, material, true, true); // this function checks for null in case the lookup fails
        }
        materialStorage.Clear();
    }

    public void CameraToggle()
    {
        if (otherCamera != null)
            otherCamera.SetActive(!otherCamera.activeSelf);
    }
}
