using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Wargon.ezs;
using Wargon.ezs.Unity;
using Random = UnityEngine.Random;

public class GameEcs : MonoBehaviour
{
    public EnemySpawner EnemySpawner;
    public static World World;
    public UIController UiController;
    public GameService GameService;
    private Systems updateSystems;
    public static bool GameReady;
    
    private void Awake()
    {

        World = new World();
        MonoConverter.Init(World);

        Servise<EnemySpawner>.Set(EnemySpawner);
        Servise<GameService>.Set(GameService);
        StartCoroutine(SpawnerDelay());
        
        updateSystems = new Systems(World)
                .Add(new OnPlayerSpawnSystem())
                .Add(new PlayerInpuntSystem())
                .Add(new PlayerAttackSystem())
                .Add(new WeaponSwaySystem())
                .Add(new ProjectileMoveSystem())
                .Add(new EnemyBulletsCollision())
                .Add(new EnergyBallCollisionSystem())
                
                .Add(new OnEnemySpawnSystem())
                //.Add(new ExplosionCollisionSystem())
                .Add(new ComboSystem())
                .Add(new EnemyAISystem())

                
                .Add(new EnemySpriteAnimationSystem())
                .Add(new MeleeAttackEnemySystem())
                .Add(new RangeAttackEnemySystem())
                .Add(new DamagePlayerByEnemyExplosionSystem())
                //.Add(new EnemyAttakStateSystem())
                
                
                .Add(new EnemyMoveSystem())
                .Add(new BurstEnemySpriteRotationSystem())
                //.Add(new BurstEnemyMoveSystem())
                .Add(new BurstCheckGroundSystem())
                .Add(new DeadEnemyLayDownCountDonwSystem())
                
                .Add(new GameOverSystem())
                
                
                .Add(new PostExplosionCollisionEnemySystem())
                .Add(new PostExplosionCollisionRocksSystem())
                
                .Add(new BurstFlyDeadEnemySystem())
                
                .Add(new LifeTimeSystem())
                .Add(new PlayParticleOnSpawnSystem())
                
                .Add(new OnBulletBackToPoolExplosionSystem())
                .Add(new ClearEventsSystem())

                .Add(new SyncTransformSystem())

            ;
#if UNITY_EDITOR
        new DebugInfo(World);
#endif
        updateSystems.Init();
        StartCoroutine(Delay(1f, () =>
        {
            GameReady = true;
        }));
    }

    private IEnumerator Delay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
    IEnumerator SpawnerDelay()
    {
        yield return new WaitForSeconds(0.1f);
        EnemySpawner.Init();
    }
    void Update()
    {
        if (GameReady)
        {
            updateSystems.OnUpdate();
        }
    }

    private void OnDestroy()
    {
        GameReady = false;
        if (updateSystems != null)
        {
            updateSystems = null;
            World.Destroy();

        }
    }
}
[EcsComponent] public struct SpawnedEvent{}

public class ComboSystem : UpdateSystem
{
    private float comboDelay;
    public override void Update()
    {
        // comboDelay += Time.deltaTime;
        // if (comboDelay > 1f)
        // {
        //     comboDelay = 0;
        //     Servise<GameService>.Get().Combo = 0;
        // }
    }
}
public class OnPlayerSpawnSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity Entity, Player player, SpawnedEvent spawned, TransformRef transformRef) =>
        {
            Servise<GameService>.Get().PlayerEntity = Entity;
            Servise<GameService>.Get().PlayerTrasform = transformRef.Value;
        });
    }
}
public class OnBulletBackToPoolExplosionSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((EnergyBall ball, TransformRef tr, Impact impact, ref BackToPoolEvent evnt) =>
        {
            var explosion = Pools.ReuseEntity(impact.Value, tr.Value.position, Quaternion.identity);
            var multiply = ball.Power * 10f;
            explosion.Get<ExplosionPower>().Value = multiply;
            explosion.Get<SphereCastRef>().Radius = multiply;
            var explosionTransform = explosion.Get<TransformRef>();
            explosionTransform.Value.localScale = new Vector3(multiply, multiply, multiply);
        });
    }
}
[EcsComponent] public class EnemySpawnEvent{}
public class OnEnemySpawnSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Without<Dead>().Each((EnemySpawnEvent evnt, TransformRef transform, SpriteAnim anim, EnemyRef enemyRef, ref TransformComponent transformComponent) =>
        {
            transformComponent.position = transform.Value.position;
            transformComponent.scale = transform.Value.localScale;
            transformComponent.rotation = transform.Value.rotation;
            anim.Value.Run.CurrentAnimation = Random.Range(0, anim.Value.Run.Frames.Length - 1);
            enemyRef.TargetEntity = Servise<GameService>.Get().PlayerEntity;
            enemyRef.MoveToTargetValue = Servise<GameService>.Get().PlayerTrasform;
            //enemyRef.NavMeshAgentVelue.enabled = true;
        });
    }
}
public class ClearEventsSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity e, PooledEvent evnt)=> e.Remove<PooledEvent>());
        entities.Each((Entity e, Collided evnt)=> e.Remove<Collided>());
        entities.Each((Entity e, DamagedByExplosion evnt)=> e.Remove<DamagedByExplosion>());
        entities.Each((Entity e, EnemySpawnEvent evnt)=> e.Remove<EnemySpawnEvent>());
        entities.Each((Entity e, BackToPoolEvent evnt)=> e.Remove<BackToPoolEvent>());
        entities.Each((Entity e, SpawnedEvent evnt)=> e.Remove<SpawnedEvent>());
        entities.Each((Entity e, AttackEvent evnt)=> e.Remove<AttackEvent>());
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

[EcsComponent]
public class AudioRef
{
    public AudioSource Value;
}
public class PlayParticleOnSpawnSystem : UpdateSystem
{
    public override void Update()
    {
        if(!GameEcs.GameReady) return;
        entities.Each((PooledEvent pooled, Particle particle) =>
        {
            particle.Value.Play();
        });
        entities.Each((PooledEvent Pooled, AudioRef audio) =>
        {
            
            audio.Value.Play();
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

            var finalPostion = new Vector3(movementX, movementY, 0);
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
                    energyBall.Power += dt * 1f;
                    energyBall.Size += dt * 1f;
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

public class EnemyBulletsCollision : UpdateSystem
{
    public override void Update()
    {
        entities.Without<UnActive>().Each((Collided collidedEvent, TransformRef transform, Impact impact, Pooled pool) =>
        {
            pool.SetActive(false);
            Pools.ReuseEntity(impact.Value, transform.Value.position, Quaternion.identity);
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
    public float SpeedRotation;
    public float MaxTime;
}
[EcsComponent] public struct ColliderRef
{
    public Collider Value;
}

[EcsComponent]
public struct EnemySprite
{
    public Vector3 Dir;
    public Vector3 Euler;
}

[EcsComponent] public struct CanRun{}
[EcsComponent] public struct CanRotate{}
[EcsComponent] public struct Dead{}

[EcsComponent] public class DeathState{}
public class PostExplosionCollisionEnemySystem : UpdateSystem
{
    private const float ROTATION_SPEED = 5500f;
    private const int MINIMUM_COMBO_SIZE = 50;
    private const float COMBO_SHOW_DELAY = 0.2f;
    private float comboShowDelay = 0;
    public override void Update()
    {
        var dt = Time.deltaTime;
        comboShowDelay += dt;
        var gameService = Servise<GameService>.Get();
        var ui = Servise<UIController>.Get();
        entities.Without<RookRef,Dead>().Each((Entity entity, DamagedByExplosion damaged, TransformRef transformRef, CanTakeDamageByExplosion tag, EnemyRef enemy, NoBurst noBurst) =>
        {
            
            gameService.Combo++;
            comboShowDelay = 0f;
            enemy.State = EnemyState.Death;
            ui.AddKills();
            //enemy.NavMeshAgentVelue.enabled = false;
            var dir = (transformRef.Value.position - damaged.From).normalized;
            if (dir.y < 0.1f)
                dir.y = Random.Range(0.4f, 1f);
            float rSpeed;
            if (dir.x > 0)
                rSpeed = ROTATION_SPEED;
            else
                rSpeed = -ROTATION_SPEED;
            var flyForce = new FlyWithBurst
            {
                Direction = dir,
                Force = damaged.Power,
                MaxTime =  10f,
                SpeedRotation =  rSpeed
            };
            
            ref var transform = ref entity.GetRef<TransformComponent>();
            transform.position = transformRef.Value.position;
            transform.rotation = transformRef.Value.rotation;
            entity.Remove<NoBurst>();
            entity.Remove<RunState>();
            entity.Remove<AttackState>();
            entity.Remove<CanRotate>();
            entity.Remove<CanRun>();
            entity.Set<DeathState>();

            entity.Add(flyForce);
        });
        TryShowCombo(gameService, ui);
    }

    private void TryShowCombo(GameService gameService, UIController ui)
    {
        if (gameService.Combo > MINIMUM_COMBO_SIZE && comboShowDelay > COMBO_SHOW_DELAY)
        {
            Debug.Log($"xxx COMBO xxx {gameService.Combo} xxx COMBO xxx");
            ui.ShowCombo(gameService.Combo);
            gameService.Combo = 0;
        }
    }

    private bool startComboDelay;

}

public class PostExplosionCollisionRocksSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity entity, DamagedByExplosion damaged, RigidBody rigidBody, RookRef rookRef, CanTakeDamageByExplosion tag) =>
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
            DOVirtual.DelayedCall(5f , () => { rigidBody.Value.isKinematic = true; });
            
            //entity.Remove<CanTakeDamageByExplosion>();
        });
    }
}

[EcsComponent]
public struct AiPath
{
    public Vector3 Target;
    public float MoveSpeed;
    public Vector3 Dir;
    public float Offset;
}



public class EnemyMoveSystem : UpdateSystem
{
    private float delay;
    private float constDelayt = 0.5f;
    public override void Update()
    {

        delay += Time.deltaTime;

        if (delay > constDelayt)
        {
            
            //if(enemyRef.NavMeshAgentVelue.isOnNavMesh)
                    
            //if(enemyRef.NavMeshAgentVelue.isOnNavMesh)
            // enemyRef.NavMeshAgentVelue.destination = enemyRef.MoveToTargetValue.position;
            // if (enemyRef.NavMeshAgentVelue.isOnNavMesh)
            // {
            //     enemyRef.NavMeshAgentVelue.CalculatePath(enemyRef.MoveToTargetValue.position,
            //         enemyRef.NavMeshAgentVelue.path);
            //     //enemyRef.NavMeshAgentVelue.destination = enemyRef.MoveToTargetValue.position;
            //     if(enemyRef.NavMeshAgentVelue.path.corners.Length > 0)
            //         path.Target = enemyRef.NavMeshAgentVelue.path.corners[0];
            // }
            entities.Without<Dead>().Each((EnemyRef enemyRef, RunState runState, ref NoBurst tag) =>
            {
                enemyRef.NavMeshAgentVelue.SetDestination(enemyRef.MoveToTargetValue.position);
                
            });
            delay = 0f;
            //path.Target = enemyRef.NavMeshAgentVelue.path.corners[0];
        }
    }
}

[EcsComponent]
public class SpriteAnim
{
    public SpriteAnimation Value;
}

[EcsComponent]
public class SpriteRender
{
    public SpriteRenderer Value;
}
[EcsComponent] public class AttackState{}
[EcsComponent] public struct RunState{}
public class EnemyAISystem : UpdateSystem
{
    private const float LOW_AGENT_DISTANCE = 50f;
    public override void Update()
    {
        entities.Without<Dead,DeathState>().Each((Entity entity, EnemyRef enemy, TransformRef TransformRef) =>
        {
            var transform = TransformRef.Value;
            enemy.DistanceToTarget = Vector3.Distance(transform.position, enemy.MoveToTargetValue.position);
            
            // if (distance > LOW_AGENT_DISTANCE)
            //     enemy.NavMeshAgentVelue.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            // else
            //     enemy.NavMeshAgentVelue.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            
            if (enemy.DistanceToTarget < enemy.AttackRange)
            {
                if (enemy.State != EnemyState.Attack)
                {
                    entity.Set<AttackState>();
                    entity.Remove<RunState>();
                    enemy.State = EnemyState.Attack;
                    //enemy.NavMeshAgentVelue.SetDestination(transform.position);

                }
            }
            else
            {
                if (enemy.State != EnemyState.Run)
                {
                    entity.Set<RunState>();
                    entity.Remove<AttackState>();
                    enemy.State = EnemyState.Run;

                }
            }
        });
        
    }
}

[EcsComponent]
public class SpriteEntity
{
    public MonoEntity Value;
}
[EcsComponent]
public class Damaged
{
    public int Damage;
}

[EcsComponent]
public class Health
{
    public int Value;
}

[EcsComponent]
public class Damage
{
    public int Value;
}
public class EnemyAttakStateSystem : UpdateSystem
{
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<Dead>().Each((Entity entity, EnemyRef enemy, Damage damage, AttackState state) =>
        {
            if (enemy.CurrentAttackDelay <= 0)
            {
                if (enemy.TargetEntity.IsDead())
                {
                    Debug.Log("PLayer Dead");
                    return;
                }
                
                var playerHp = enemy.TargetEntity.Get<Health>();
                playerHp.Value -= damage.Value;
                if (playerHp.Value <= 0)
                {
                    world.CreateEntity().Set<GameOver>();
                }
                
                enemy.CurrentAttackDelay = enemy.AttackDelay;
            }
            enemy.CurrentAttackDelay -= dt;
        });
    }
}

[EcsComponent]
public class GameOver
{
    
}
public class GameOverSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((Entity entity, GameOver gameover) =>
        {
            Debug.Log("GAMEOVER");
            //Time.timeScale = 0;
            entity.Destroy();
            GameEcs.GameReady = false;
        });
    }
}

[EcsComponent] 
public class StayDeadCountDown
{
    public float Default;
    public float Value;
}

[EcsComponent]
public class View
{
    public MonoEntity Value;
}
public class DeadEnemyLayDownCountDonwSystem : UpdateSystem
{
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Each((Dead Dead, StayDeadCountDown deadCountDown, View view) =>
        {
            deadCountDown.Value -= dt;
            if(deadCountDown.Value <= 0)
                Servise<EnemySpawner>.Get().BackToPool(view.Value, deadCountDown);
        });
    }
}
[EcsComponent] public class RangeAttackEnemy
{
    public MonoEntity Bullet;
    public Transform FirePoint;
}
[EcsComponent] public class MeleeAttackEnemy{}
[EcsComponent]public class AttackEvent{}
public class RangeAttackEnemySystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((EnemyRef enemy, RangeAttackEnemy rangeAttack, TransformRef transform , AttackEvent evnt) =>
        {
            var dir = (enemy.MoveToTargetValue.position - transform.Value.position).normalized;
            var bullet = Pools.ReuseEntity(rangeAttack.Bullet, rangeAttack.FirePoint.position, Quaternion.identity);
            bullet.GetRef<Direction>().Value = dir;
        });
    }
}

public class MeleeAttackEnemySystem : UpdateSystem
{
    public override void Update()
    {
        entities.Each((EnemyRef enemy, MeleeAttackEnemy meleeAttack, AttackEvent evnt, Damage damage) =>
        {
            var playerHp = enemy.TargetEntity.Get<Health>();
            playerHp.Value -= damage.Value;
            if (playerHp.Value <= 0)
            {
                world.CreateEntity().Set<GameOver>();
            }
        });
    }
}

public class DamagePlayerByEnemyExplosionSystem : UpdateSystem
{
    public override void Update()
    {
        entities.Without<Dead>().Each((Entity entity, Health Health, Damaged damaged) =>
        {
            Debug.Log(damaged.Damage);
            Health.Value -= damaged.Damage;
            if (Health.Value <= 0)
            {
                world.CreateEntity().Set<GameOver>();
                entity.Set<Dead>();
            }
            entity.Remove<Damaged>();
        });
        
    }
}

public class BurstCheckGroundSystem : UpdateSystem, IJobSystemTag
{
    private JobCast job;
    private LayerMask mask = LayerMask.GetMask("Ground");
    private Vector3 offset = new Vector3(0, -2.3f, 0);
    public override void Update()
    {
        var dir = Vector3.down;
        job.offset = offset;
        job.dt = Time.deltaTime;
        job.mass = 24f;
        job.y = 2.5f;
        entities.Without<Dead,DeathState>().EachWithJobRaycast(ref job, in dir, in offset, mask, 0.3f);
    }
    struct JobCast : IJobExecute<TransformComponent, RaycastHit>
    {
        public float dt;
        public float mass;
        public Vector3 offset;
        public float y;
        public void ForEach(ref TransformComponent transform, ref RaycastHit raycast)
        {
            //Debug.DrawLine(transform.position, raycast.point, Color.green);
            if (raycast.distance < 0.25f)
            {
                transform.position.y = raycast.point.y + y;
            }
            else
            {
                transform.position += Vector3.down * mass * dt;
            }
        }
    }
}
public class BurstEnemyMoveSystem : UpdateSystem, IJobSystemTag
{
    private MoveJob job;
    public override void Update()
    {
        job.target = Servise<GameService>.Get().PlayerCaneraTransform.position;

        job.dt = Time.deltaTime;
        job.offest= 2.5f;
        entities.Without<Dead,DeathState>().EachWithJob<MoveJob, AiPath, TransformComponent, RunState, Dead, DeathState>(ref job);
    }

    struct MoveJob : IJobExecute<AiPath, TransformComponent, RunState>
    {
        public float offest;
        public float dt;
        public Vector3 target;
        private Vector3 result;
        public void ForEach(ref AiPath path, ref TransformComponent transform, ref RunState r)
        {
            result = Vector3.MoveTowards(transform.position, target, dt * path.MoveSpeed);
            transform.position.x = result.x;
            transform.position.z = result.z;
        }
    }
}
public class BurstFlyDeadEnemySystem : UpdateSystem, IJobSystemTag
{
    private readonly static Vector3 Offest = new Vector3(0f, 0.2f, 0f);
    private FlyEnemyJob j0b;
    private const float DEAD_Y_POS = 0.2f;
    private readonly LayerMask mask = LayerMask.GetMask("Ground");
    private readonly Vector3 offset = new Vector3(0, 0, 0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Quaternion DeadRotation()
    {
        return Quaternion.Euler(90,Random.Range(0,360),90);
    }
    public override void Update()
    {
        j0b.deadpos = DEAD_Y_POS;
        var rayCastDiraction = Vector3.down;
        j0b.dt = Time.smoothDeltaTime;
        entities
            .Without<CanRotate,CanRun>()
            .EachWithJobRaycast<FlyEnemyJob, CanRotate,CanRun,FlyWithBurst>(ref j0b, in rayCastDiraction, in offset, mask, 1f);
        
        entities.Each((Entity entity, ColliderRef Collider, DeathState deathState, ref TransformRef transform, ref FlyWithBurst fly) =>
        {
            if (fly.Grounded)
            {
                Debug.Log("SSS");
                entity.Get<EnemyRef>().NavMeshAgentVelue.enabled = false;
                transform.Value.position = entity.Get<TransformComponent>().position;
                transform.Value.rotation = DeadRotation();
                Collider.Value.enabled = false;
                entity.Get<EnemyRef>().State = EnemyState.Dead;
                
                entity.Set<Dead>();
                entity.Set<NoBurst>();
                entity.Remove<FlyWithBurst>();
            }
        });
    }
    struct FlyEnemyJob : IJobExecute<TransformComponent, FlyWithBurst,RaycastHit>
    {
        public float deadpos;
        public float dt;
        private Vector3 rot;
        
        public void ForEach(ref TransformComponent transform, ref FlyWithBurst fly, ref RaycastHit raycast)
        {
            //Debug.DrawLine(transform.position, raycast.point, Color.green);
            if (!fly.Grounded)
            {
                transform.position += fly.Direction * fly.Force * dt;
                fly.Delay += dt;
                fly.Direction.y -= dt * 0.5f;
                rot = transform.rotation.eulerAngles;
                rot.z += fly.SpeedRotation * dt;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, rot.z), 0.1f);
            }
            if (fly.Delay > 1)
            {
                if (raycast.GetColliderID() != 0 && raycast.distance < 0.2f)
                {
                    transform.position = raycast.point;
                    transform.position += BurstFlyDeadEnemySystem.Offest;
                    Debug.Log($"point {raycast.point}");
                    Debug.Log($"transform.position {transform.position}");
                    fly.Grounded = true;
                }
                // if (transform.position.y <= deadpos || fly.MaxTime < 0)
                //     fly.Grounded = true;
            }
        }
    }
}

public class BurstEnemySpriteRotationSystem : UpdateSystem, IJobSystemTag
{
    private Job j0b;
    public override void Update()
    {
        j0b.dt = Time.deltaTime;
        j0b.playerPosition = Servise<GameService>.Get().PlayerCaneraTransform.position;

        entities.Without<Dead,DeathState>().EachWithJob<Job,EnemySprite,TransformComponent,CanRotate,           
            Dead,DeathState>(ref j0b);
    }

    struct Job : IJobExecute<EnemySprite, TransformComponent, CanRotate>
    {
        public float dt;
        public Vector3 playerPosition;
        public void ForEach(ref EnemySprite sprite, ref TransformComponent transformComponent, ref CanRotate r)
        {
            playerPosition.y = transformComponent.position.y;
            sprite.Dir = playerPosition - transformComponent.position;
            transformComponent.rotation = Quaternion.LookRotation(sprite.Dir);
        }
    }
}