using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextColor : MonoBehaviour
{
    public TextMesh text;

    public void Start()
    {
        ChangeTextColor();
    }

    public void ChangeTextColor()
    {
        text.color = Color.red;
    }
}
