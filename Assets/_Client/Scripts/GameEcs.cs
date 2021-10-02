using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs;

public class GameEcs : MonoBehaviour
{
    public static World World;
    private Systems updateSystems;
    private void Awake()
    {
        World = new World();
        updateSystems = new Systems(World)



            ;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (World != null)
        {
            updateSystems.OnUpdate();
        }
    }
}

[EcsComponent]
public class InputC
{
    public float Horizontal;
    public float Vertical;
    public bool PressFire;
    public bool PressJump;
}
[EcsComponent] public class Player{}
[EcsComponent]
public struct MoveSpeed
{
    public float Value;
}

[EcsComponent]
public class RigidBody
{
    public Rigidbody Value;
}

[EcsComponent]
public class CharacterControllerRef
{
    public CharacterController Value;
}

[EcsComponent]
public class TransformRef
{
    public Transform Value;
}
public class PlayerInpuntSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((InputC input, Player p) =>
        {
            input.Horizontal = Input.GetAxis("Horizontal");
            input.Vertical = Input.GetAxis("Vertical");
            input.PressFire = Input.GetMouseButtonDown(0);
            input.PressJump = Input.GetKey(KeyCode.Space);
        });
    }
}

public class PlayerMoveSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((InputC input, RigidBody rigidBody, ActorRef actor, TransformRef transformRef, CharacterControllerRef ch) =>
        {

        });
    }
}
[EcsComponent]
public class ActorRef
{
    public Actor Value;
}