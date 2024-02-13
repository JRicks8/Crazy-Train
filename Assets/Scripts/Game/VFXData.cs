using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXData : MonoBehaviour
{
    public enum VFXType
    {
        BigImpactSmoke,
        BigSmoke,
        DashSmoke1,
        DashSmoke2,
        ExpandingCircle256,
        ExpandingCircle128,
        ExpandingCircle64,
        Explosion,
        Impact1,
        ImpactDustKick,
        MediumSmoke,
        SideImpact,
        SidePuff,
        SideSmoke1,
        SideSmoke2,
        SmallImpact1,
        SmallImpact2,
        SmallSidePuff1,
        SmallSidePuff2,
        SmallSmoke1,
        SmallSmoke2,
    }

    public List<Texture2D> VFXTextures = new List<Texture2D>();

    public static List<Sprite[]> staticVFXSprites = new List<Sprite[]>();

    private void Awake()
    {
        for (int i = 0; i < VFXTextures.Count; i++)
        {
            Texture2D tex = VFXTextures[i];

            staticVFXSprites.Add(Resources.LoadAll<Sprite>(tex.name));
        }
    }

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 position, Vector3 scale = default)
    {
        GameObject obj = new GameObject();
        obj.transform.position = position;
        if (scale == Vector3.zero)
            obj.transform.localScale = Vector3.one;
        else
            obj.transform.localScale = scale;
        obj.AddComponent<VFXHandler>().Initialize(sheet);
        return obj;
    }
}
