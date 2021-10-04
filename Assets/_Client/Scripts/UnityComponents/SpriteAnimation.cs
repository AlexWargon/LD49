using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    public bool AttackFrameEnd;
    public float FrameTime;
    public float CurruntFrameTime;
    public int AttackFrame = 9;
    public Animation Run;
    public Animation Attack;
    public Animation Death;

}

[Serializable]
public struct Animation
{
    public int CurrentAnimation;
    public Sprite[] Frames;
}
