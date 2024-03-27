using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VFXData : MonoBehaviour
{
    public enum VFXType
    {
        Heart,
        BigImpactSmoke,
        BigSmoke,
        DashSmoke1,
        DashSmoke2,
        ExpandingCircle256,
        ExpandingCircle128,
        ExpandingCircle64,
        Explosion,
        Fire1,
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

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 localPosition)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = localPosition;
        obj.AddComponent<VFXHandler>().Initialize(sheet);
        return obj;
    }

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 localPosition, Transform parent)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = localPosition;
        obj.transform.parent = parent;
        obj.AddComponent<VFXHandler>().Initialize(sheet);
        return obj;
    }

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 localPosition, Vector3 scale)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = scale;
        obj.AddComponent<VFXHandler>().Initialize(sheet);
        return obj;
    }

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 localPosition, Vector3 scale, Transform parent)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = scale;
        obj.transform.parent = parent;
        obj.AddComponent<VFXHandler>().Initialize(sheet);
        return obj;
    }

    public static GameObject SpawnVFX(Sprite[] sheet, Vector2 localPosition, Vector3 scale, Transform parent, float VFXduration)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = scale;
        obj.transform.parent = parent;
        obj.AddComponent<VFXHandler>().Initialize(sheet, true, VFXduration);
        return obj;
    }
}
