using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshCool : MonoBehaviour
{
    public TextMeshProUGUI Back;
    public TextMeshProUGUI Front;

    public Color BackColor
    {
        get { return Back.color;}
        set { Back.color = value; }
    }
    public string text
    {
        get
        {
            return Back.text;
        }
        set
        {
            Back.text = value;
            Front.text = value;
        }
    }
}
