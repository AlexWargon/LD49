using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;


[EcsComponent] public class InputC
{
    public float Horizontal;
    public float Vertical;
    public bool PressFire;
    public bool PressJump;
}

[EcsComponent]
public class Player
{
    
}
[EcsComponent] public struct MoveSpeed
{
    public float Value;
}
[EcsComponent] public class RigidBody
{
    public Rigidbody Value;
}
[EcsComponent] public class CharacterControllerRef
{
    public CharacterController Value;
}
[EcsComponent] public class TransformRef
{
    public Transform Value;
}
[EcsComponent] public class ActorRef
{
    public Actor Value;
}

[EcsComponent] public class WeaponHandRef
{
    public MonoEntity Value;
}
[EcsComponent] public class WeaponSway
{
    public float amount;
    public float maxAmount;
    public float smoothAmount;
    public Vector3 initialPosition;
}

[EcsComponent]
public class PlayerTag
{
    
}
[EcsComponent] public struct Direction
{
    public Vector3 Value;
}

[EcsComponent]
public struct Projectile
{
    
}
[EcsComponent] public class CastWeapon
{
    public bool Loaded;
    public MonoEntity Projectile;
    public Entity CurrentProjectile;
    public float ShootDelay;
    public float PinchTime;
    public Transform SphereStartPos;
    public Transform SphereEndPos;
    public Vector3 CurrentSpherePosition;
}

[EcsComponent]
public struct RayCastRef
{
    public Ray Ray;
    public RaycastHit Hit;
    public LayerMask Mask;
}

[EcsComponent]
public class EnergyBall
{
    public float Size;
    public float Power;
}

[EcsComponent]
public class SphereCastRef
{
    public float Radius;
    public Ray Ray;
    public RaycastHit Hit;
    public LayerMask Mask;
}
[EcsComponent]
public class SphereInWeapon{}
