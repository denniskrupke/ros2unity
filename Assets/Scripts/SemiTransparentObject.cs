using UnityEngine;
using System.Collections;

public class SemiTransparentObject : MonoBehaviour {

    private Renderer[] mRenderers;
    public float alpha = .1f;
        
    Color oldColor;

    void Start()
    {                
        mRenderers = GetComponentsInChildren<Renderer>();
        SetRendererAlphas();
    }

    public void SetRendererAlphas()
    {
        for (int i = 0; i < mRenderers.Length; i++)
        {
            for (int j = 0; j < mRenderers[i].materials.Length; j++)
            {
                oldColor = mRenderers[i].materials[j].color;
                mRenderers[i].materials[j].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                oldColor.a = alpha;
                //oldColor.r = alpha;
                //oldColor.g = alpha;
                
                mRenderers[i].materials[j].color = oldColor;

                //Color matColor = mRenderers[i].materials[j].color;
                //matColor.a = alpha;
                //mRenderers[i].materials[j].color = matColor;
            }
        }
    }
}
