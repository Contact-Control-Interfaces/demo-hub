using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FrustumDeformer : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Mesh deformingMesh;
    public Dictionary<int, Transform> MappedIndices = new Dictionary<int, Transform>();

    public Transform[] BottomCandidateTransforms;
    public Transform[] TopMappedTransforms;
    public int[] BottomMappedIndices;
    public int[] TopMappedIndices;

    private Vector3[] originalVertices;

    private void Start()
    {
        if (meshFilter == null)
            meshFilter = this.GetComponentInChildren<MeshFilter>();

        deformingMesh = meshFilter.mesh;
        originalVertices = deformingMesh.vertices;
    }

    private GameObject GenerateDebugVisual(Vector3 pos, int index)
    {
        GameObject result = new GameObject(index.ToString());
        result.transform.position = this.transform.TransformPoint(pos);
        return result;
    }

    /// <summary>
    /// Of the provided candidates, selects the outermost transforms when sorted left-to-right from the user's perspective.
    /// </summary>
    /// <returns>The leftmost and rightmost transforms from the user's perspective</returns>
    private Transform[] SelectOutermostCandidates(Transform[] candidates)
    {
        var clone = (Transform[]) candidates.Clone();
        var ordered = clone.OrderBy(x => Camera.main.WorldToScreenPoint(x.position).x).ToArray();
        return new Transform[] { ordered[0], ordered[ordered.Length - 1] };
    }

    private void Update()
    {
        Vector3[] copy = new Vector3[originalVertices.Length];
        originalVertices.CopyTo(copy, 0);
        UpdateFaceVertices(ref copy, SelectOutermostCandidates(BottomCandidateTransforms), BottomMappedIndices);
        UpdateFaceVertices(ref copy, TopMappedTransforms, TopMappedIndices);
        deformingMesh.vertices = copy;
    }

    private void UpdateFaceVertices(ref Vector3[] vertices, Transform[] mapped, int[] indicesToModfiy)
    {
        for (int i = 0; i < indicesToModfiy.Length; i++) {
            vertices[indicesToModfiy[i]] = this.transform.InverseTransformPoint(mapped[i].position);
            deformingMesh.RecalculateNormals();
            deformingMesh.RecalculateBounds();
        }
    }
}
