using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
                
                //.Add(new ExplosionCollisionSystem())
                .Add(new PostExplosionCollisionEnemySystem())
                .Add(new PostExplosionCollisionRocksSystem())
                .Add(new LifeTimeSystem())
                .Add(new PlayParticleOnSpawnSystem())
                .Add(new EnemyMoveSystem())
                .Add(new ClearEventsSystem())

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

public class ClearEventsSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity e, PooledEvent evnt)=> e.Remove<PooledEvent>());
        entities.Each((Entity e, Collided evnt)=> e.Remove<Collided>());
        entities.Each((Entity e, DamagedByExplosion evnt)=> e.Remove<DamagedByExplosion>());
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
        entities.Each((PooledEvent pooled, ExplosionTriggerRef particle) =>
        {
            particle.Value.triggered = false;
            particle.Value.delayStarted = false;
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
[EcsComponent] public class Collided{}
public class EnergyBallCollisionSystem : UpdateSystem
{
    public override void Update()
    {
        // entities.Without<UnActive>().Each((EnergyBall EnergyBall, SphereCastRef sphereCast, TransformRef transform, Impact impact, Pooled pool) =>
        // {
        //     sphereCast.Ray.origin = transform.Value.position;
        //
        //     if (Physics.SphereCast(sphereCast.Ray.origin, sphereCast.Radius, Vector3.zero, out sphereCast.Hit))
        //     {
        //         var explosion = Pools.ReuseEntity(impact.Value, transform.Value.position, Quaternion.identity);
        //         var multiply = EnergyBall.Power * 10f;
        //         var explosionTransform = explosion.Get<TransformRef>();
        //         explosionTransform.Value.localScale = new Vector3(multiply, multiply, multiply);
        //         pool.SetActive(false);
        //     }
        // });
        entities.Without<UnActive>().Each((EnergyBall EnergyBall, Collided Collided, TransformRef transform, Impact impact, Pooled pool) =>
        {
            var explosion = Pools.ReuseEntity(impact.Value, transform.Value.position, Quaternion.identity);
            var multiply = EnergyBall.Power * 10f;
            explosion.Get<ExplosionPower>().Value = multiply;
            explosion.Get<SphereCastRef>().Radius = multiply;
            var explosionTransform = explosion.Get<TransformRef>();
            explosionTransform.Value.localScale = new Vector3(multiply, multiply, multiply);
            pool.SetActive(false);
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

[EcsComponent]
public class DamagedByExplosion
{
    public float Power;
    public Vector3 From;
}

[EcsComponent]
public class CanTakeDamageByExplosion
{
}
[EcsComponent]
public class ExplosionPower
{
    public float Value;
}
public class ExplosionCollisionSystem : UpdateSystem
{
    private Collider[] colliders = new Collider[64];
    public override void Update()
    {
        entities.Without<UnActive>().Each((TransformRef transform, SphereCastRef sphereCast, PooledEvent pooledEvent, ExplosionPower power) =>
        {
            var collidersCount = Physics.OverlapSphereNonAlloc(transform.Value.position, sphereCast.Radius, colliders);

            for (var i = 0; i < collidersCount; i++)
            {
                Debug.Log(colliders[i].name);
                var mono = colliders[i].GetComponent<MonoEntity>();
                if (!mono) return;
            
                if (mono.Entity.Has<CanTakeDamageByExplosion>())
                {
                    mono.Entity.Add(new DamagedByExplosion
                    {
                        Power = power.Value,
                        From = transform.Value.position
                    });
                }
            }
            // if (!Physics.SphereCast(sphereCast.Ray.origin, sphereCast.Radius, transform.Value.forward, out sphereCast.Hit)) return;
            // Debug.Log("123123");
            // var mono = sphereCast.Hit.collider.GetComponent<MonoEntity>();
            // if (!mono) return;
            //
            // if (mono.Entity.Has<CanTakeDamageByExplosion>())
            // {
            //     Debug.Log("SSS");
            //     mono.Entity.Add(new DamagedByExplosion
            //     {
            //         Power = power.Value,
            //         From = transform.Value.position
            //     });
            // }
        });
    }
}

[EcsComponent]
public struct FlyWithBurst
{
    public Vector3 Direction;
    public float Delay;
    public float Force;
    public bool Grounded;
}

public class BurstFlyEnemySystem : UpdateSystem
{
    private FlyEnemyJob j0b;
    public override void Update()
    {
        j0b.dt = Time.deltaTime;
        entities.EachWithJob<FlyEnemyJob, FlyWithBurst, TransformComponent>(ref j0b);
    }
    public struct FlyEnemyJob : IJobExecute<FlyWithBurst, TransformComponent>
    {
        public float dt;
        public void ForEach(ref FlyWithBurst fly, ref TransformComponent transform)
        {
            if (!fly.Grounded)
            {
                transform.position += fly.Direction * fly.Force * dt;
                fly.Delay += dt;
            }

            if (fly.Delay > 1)
            {
                if (transform.position.y < 0.1f)
                    fly.Grounded = true;
            }
            
            
        }
    }
}
[EcsComponent]
public class CurveLine
{
    public AnimationCurve Value;
}

public class PostExplosionCollisionEnemySystem : UpdateSystem
{
    public override void Update()
    {
        entities.Without<RookRef>().Each((Entity entity, DamagedByExplosion damaged, RigidBody rb, CanTakeDamageByExplosion tag, EnemyRef enemy) =>
        {
            var force = (rb.Value.position - damaged.From).normalized * damaged.Power; 
            rb.Value.AddForce(force, ForceMode.Impulse);
        });
    }
}

public class PostExplosionCollisionRocksSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity entity,DamagedByExplosion damaged, RigidBody rigidBody, RookRef rookRef, CanTakeDamageByExplosion tag) =>
        {
            rigidBody.Value.isKinematic = false;
            var force1 = (rigidBody.Value.position - damaged.From).normalized * damaged.Power * 5; 
            entity.Get<RigidBody>().Value.AddForce(force1, ForceMode.Impulse);
            
            
            for (var i = rookRef.Other.Value.Count - 1; i >= 0; i--)
            {
                var rock = rookRef.Other.Value[i].Entity;
                if(rock.id == entity.id) continue;
                if (rock.Has<CanTakeDamageByExplosion>())
                {
                    var rb = rock.Get<RigidBody>().Value;
                    rb.isKinematic = false;
                    var force = (rb.position - damaged.From).normalized * damaged.Power * 5; 
                    rock.Get<RigidBody>().Value.AddForce(force, ForceMode.Impulse);
                    rock.Remove<CanTakeDamageByExplosion>();
                }
            }
            entity.Remove<CanTakeDamageByExplosion>();
        });
    }
}

public class EnemyMoveSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Without<UnActive>().Each((EnemyRef enemyRef) =>
        {
            enemyRef.NavMeshAgentVelue.destination = enemyRef.MoveToTargetValue.position;
        });
    }
}