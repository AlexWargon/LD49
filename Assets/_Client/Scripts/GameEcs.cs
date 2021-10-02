using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

public class GameEcs : MonoBehaviour
{
    public static World World;
    private Systems updateSystems;
    private void Awake()
    {
        World = new World();
        MonoConverter.Init(World);
        updateSystems = new Systems(World)

                .Add(new PlayerInpuntSystem())
                .Add(new PlayerAttackSystem())
                .Add(new WeaponSwaySystem())
                .Add(new ProjectileMoveSystem())
                .Add(new EnergyBallCollisionSystem())
                
            
            
            
                .Add(new SyncTransformSystem())
            ;
#if UNITY_EDITOR
        new DebugInfo(World);
#endif
        updateSystems.Init();
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

public class PlayerInpuntSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((InputC input, PlayerTag p) =>
        {
            input.Horizontal = Input.GetAxis("Horizontal");
            input.Vertical = Input.GetAxis("Vertical");
            input.PressFire = Input.GetButton("Fire1");
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

public class WeaponSwaySystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((PlayerTag tag, WeaponSway sway, TransformRef transformRef) =>
        {
            var transform = transformRef.Value;
            float movementX = -Input.GetAxis("Mouse X") * sway.amount;
            float movementY = -Input.GetAxis("Mouse Y") * sway.amount;

            movementX = Mathf.Clamp(movementX, -sway.maxAmount, sway.maxAmount);
            movementY = Mathf.Clamp(movementY, -sway.maxAmount, sway.maxAmount);

            Vector3 finalPostion = new Vector3(movementX, movementY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, finalPostion + sway.initialPosition, Time.deltaTime * sway.smoothAmount);
        });
    }
}

public class PlayerAttackSystem : UpdateSystem
{
    public override void Update()
    {
        var dt = Time.deltaTime;

        entities.Each((CastWeapon weapon, PlayerTag p, InputC input) =>
        {
            entities.Each((Player playerTag, ref RayCastRef playerCast) =>
            {
                Debug.Log("SSS");
                if (input.PressFire && weapon.Loaded)
                {
                    weapon.PinchTime += dt;
                    ref var sphereTransform = ref weapon.CurrentProjectile.GetRef<TransformComponent>();
                    var energyBall = weapon.CurrentProjectile.Get<EnergyBall>();
                    var moveDir = (weapon.SphereEndPos.position - weapon.SphereStartPos.position).normalized;
    
                    sphereTransform.position += moveDir * 2f * dt;
                    sphereTransform.scale.x += dt;
                    sphereTransform.scale.y += dt;
                    sphereTransform.scale.z += dt;
                    energyBall.Power += dt;
                    energyBall.Size += dt;
                    return;
                }

                if (!input.PressFire && weapon.PinchTime > 0f)
                {
                    weapon.PinchTime = 0f;
                    var dir = (playerCast.Hit.point - weapon.CurrentProjectile.GetRef<TransformComponent>().position).normalized;
                    weapon.CurrentProjectile.GetRef<Direction>().Value = dir;
                    weapon.CurrentProjectile.Remove<SphereInWeapon>();
                    weapon.CurrentProjectile = Pools.ReuseEntity(weapon.Projectile, weapon.SphereStartPos.position, Quaternion.identity);
                    weapon.Loaded = true;
                }

                if (!weapon.Loaded && weapon.PinchTime < 0.01f)
                {
                    
                    weapon.CurrentProjectile = Pools.ReuseEntity(weapon.Projectile, weapon.SphereStartPos.position, Quaternion.identity);
                    weapon.Loaded = true;
                }
            });

        });
    }
}

public class ProjectileMoveSystem : UpdateSystem
{
    private const float RAY_DISTANCE = 0.5f;

    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<UnActive>().Each((ref MoveSpeed speed, ref TransformComponent transform, ref Direction direction) =>
        {
            transform.position += direction.Value * speed.Value * dt;
        });
        
    }
}

public class EnergyBallCollisionSystem : UpdateSystem
{
    public override void Update()
    {
        // entities.Without<UnActive>().Each((EnergyBall EnergyBall, SphereCastRef sphereCastRef, ref TransformComponent transform) =>
        // {
        //     sphereCastRef.Ray.origin = transform.position;
        //     if (Physics.SphereCast(sphereCastRef.Ray, sphereCastRef.Radius, out sphereCastRef.Hit))
        //     {
        //         if(sphereCastRef.Hit.collider != null)
        //             Debug.Log($"{sphereCastRef.Hit.collider.name} Damaged");
        //     }
        // });
    }
}