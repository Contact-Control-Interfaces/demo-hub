using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabNormal : MonoBehaviour
{
/*
    private Dictionary<GameObject, Texture2D> textureDictionary = new Dictionary<GameObject, Texture2D>();
    Renderer renderer;
    Material material;
    Texture2D tex;
    Vector2 pixelUV;
    Color color;
    int textureId = Shader.PropertyToID("_BumpMap");
    public Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        renderer = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        tex = renderer.material.mainTexture as Texture2D;
        pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;

        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
        tex.Apply();

        if (textureDictionary.TryGetValue(results[i].collider.gameObject, out texture) == false)
{
    bool textureFound = false;
    renderer = results[i].collider.gameObject.GetComponent<Renderer>();
    if (base.GetComponent<Renderer>() != null) // Terrain has no renderer, so need to check
    {
                material = base.GetComponent<Renderer>().sharedMaterial;
        texture = (Texture2D)material.GetTexture(textureId);
        if (texture != null) // We're looking for the bump map texture here, which could be null
        {
                    textureDictionary.Add(results[i].collider.gameObject, texture);
            textureFound = true;
        }
    }
    if(textureFound == false) // If the texture doesn't exist that's fine, but put a null entry in the dictionary so you don't continue to look for it every iteration
    {
        texture = null;
        textureDictionary.Add(results[i].collider.gameObject, texture);
    }
}
if(texture!=null)
{ 
    textureCoord = results[i].textureCoord;
    color = texture.GetPixelBilinear(textureCoord.x, textureCoord.y);
    // Do your stuff with the bump map color now
}

    }*/
}
