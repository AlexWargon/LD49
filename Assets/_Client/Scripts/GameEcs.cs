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
                
                
                .Add(new LifeTimeSystem())
                .Add(new PlayParticleOnSpawnSystem())
                .Add(new ClearEventsSystem())
                //.Add(new SyncTransformSystem())
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

public class ClearEventsSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity Entity, PooledEvent pooledEvent)=> Entity.Remove<PooledEvent>());
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

[EcsComponent]
public class Particle
{
    public ParticleSystem Value;
}
public class PlayParticleOnSpawnSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((PooledEvent pooled, Particle particle) =>
        {
            particle.Value.Play();
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
        var camera = Camera.main;
        entities.Each((CastWeapon weapon, PlayerTag p, InputC input, TransformRef wpnTransform) =>
        {
            entities.Each((Player playerTag, ref RayCastRef playerCast) =>
            {

                playerCast.Ray = camera.ScreenPointToRay(Input.mousePosition);
                if (input.PressFire && weapon.Loaded)
                {
                    weapon.PinchTime += dt;
                    var sphereTransform = weapon.CurrentProjectile.Get<TransformRef>();
                    var energyBall = weapon.CurrentProjectile.Get<EnergyBall>();
                    var moveDir = (weapon.SphereEndPos.localPosition - weapon.SphereStartPos.localPosition).normalized;
                    var curPos = weapon.CurrentSpherePosition;
                    
                    curPos += moveDir * 1f * dt;
                    weapon.CurrentSpherePosition = curPos;
                    sphereTransform.Value.localPosition = curPos;
                    var scale = sphereTransform.Value.localScale;
                    scale.x += dt * 2f;
                    scale.y += dt * 2f;
                    scale.z += dt * 2f;
                    sphereTransform.Value.localScale = scale;
                    energyBall.Power += dt * 2f;
                    energyBall.Size += dt * 2f;
                    return;
                }
                //SHOOT
                if (!input.PressFire && weapon.PinchTime > 0f)
                {
                    
                    weapon.PinchTime = 0f;

                    var sphere = weapon.CurrentProjectile;
                    var sphereTransform = sphere.Get<TransformRef>();
                    sphereTransform.Value.SetParent(Pools.GetContainer(sphere.Get<Pooled>().containerIndex));
                    ref var sphereDir = ref sphere.GetRef<Direction>();
                    sphereDir.Value = playerCast.Ray.direction;
                    sphere.Remove<UnActive>();
                    sphere.Remove<SphereInWeapon>();
                    
                    
                    
                    var newShere = Pools.ReuseEntity(weapon.Projectile, weapon.SphereStartPos.position, Quaternion.identity);
                    weapon.CurrentProjectile = newShere;
                    sphereTransform = newShere.Get<TransformRef>();
                    sphereTransform.Value.SetParent(wpnTransform.Value);
                    sphereTransform.Value.localScale = Vector3.one;
                    weapon.CurrentSpherePosition = weapon.SphereStartPos.localPosition;
                    weapon.CurrentProjectile.Add(new SphereInWeapon());
                    weapon.CurrentProjectile.Set<UnActive>();
                    newShere.Get<EnergyBall>().Power = 1;
                    newShere.Get<EnergyBall>().Size = 1;
                    weapon.Loaded = true;
                }

                if (!weapon.Loaded && weapon.PinchTime < 0.01f)
                {
                    var newShere = Pools.ReuseEntity(weapon.Projectile, weapon.SphereStartPos.position, Quaternion.identity);
                    weapon.CurrentProjectile = newShere;
                    var sphereTransform = newShere.Get<TransformRef>();
                    sphereTransform.Value.SetParent(wpnTransform.Value);
                    sphereTransform.Value.localScale = Vector3.one;
                    weapon.CurrentSpherePosition = weapon.SphereStartPos.localPosition;
                    weapon.CurrentProjectile.Add(new SphereInWeapon());
                    weapon.CurrentProjectile.Set<UnActive>();
                    newShere.Get<EnergyBall>().Power = 1;
                    newShere.Get<EnergyBall>().Size = 1;
                    weapon.Loaded = true;
                }

            });

        });
    }
}

public class InWeaponSpherePosSetSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((CastWeapon weapon) =>
        {
            entities.Each((SphereInWeapon inWeapon, ref TransformComponent transform) =>
            {
                
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
        
        entities.Without<UnActive>().Each((ref Direction direction, ref MoveSpeed speed, TransformRef transform) =>
        {
            transform.Value.position += direction.Value * speed.Value * dt;
            Vector3 pos = transform.Value.position;
            pos.y += -0.5f * dt;
            transform.Value.position = pos;
        });
    }
}

public class EnergyBallCollisionSystem : UpdateSystem
{
    private RaycastHit[] hits = new RaycastHit[128];
    public override void Update()
    {
        entities.Without<UnActive>().Each((EnergyBall EnergyBall, SphereCastRef sphereCast, TransformRef transform, Impact impact) =>
        {
            sphereCast.Ray.origin = transform.Value.position;
            //var hitsCount = Physics.SphereCastNonAlloc(sphereCastRef.Ray, sphereCastRef.Radius, hits, 50f);
            if (Physics.SphereCast(sphereCast.Ray.origin, sphereCast.Radius, transform.Value.forward,
                out sphereCast.Hit))
            {
                var explosion = Pools.ReuseEntity(impact.Value, transform.Value.position, Quaternion.identity);
                var multiply = EnergyBall.Power * 10f;
                explosion.Get<TransformRef>().Value.localScale = new Vector3(multiply, multiply, multiply);
            }
        });
    }
}

public class LifeTimeSystem : UpdateSystem
{
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<UnActive>().Each((Pooled pool) =>
        {
            pool.CurrentLifeTime -= dt;
            if(pool.CurrentLifeTime <= 0)
                pool.SetActive(false);
        });
    }
}
public class ExplosionCollisionSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Without<UnActive>().Each((TransformRef transform, SphereCastRef sphereCast, Particle particle, PooledEvent pooledEvent) =>
        {
            if (Physics.SphereCast(sphereCast.Ray.origin, sphereCast.Radius, transform.Value.forward, out sphereCast.Hit))
            {
                
            }
        });
    }
}