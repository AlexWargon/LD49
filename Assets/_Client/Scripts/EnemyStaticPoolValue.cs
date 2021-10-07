using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyStaticPoolValue
{
    public static int EnemyPoolValue
    {
        get => PlayerPrefs.GetInt("EnemyPoolValue");
        set => PlayerPrefs.SetInt("EnemyPoolValue", value);
    }
}
