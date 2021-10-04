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
        Servise<GameService>.Set(new GameService());
        Servise<EnemySpawner>.Set(EnemySpawner);
        Servise<GameService>.Set(GameService);
        StartCoroutine(SpawnerDelay());
        
        updateSystems = new Systems(World)

                .Add(new PlayerInpuntSystem())
                .Add(new PlayerAttackSystem())
                .Add(new WeaponSwaySystem())
                .Add(new ProjectileMoveSystem())
                .Add(new EnergyBallCollisionSystem())
                .Add(new OnPlayerSpawnSystem())
                .Add(new OnEnemySpawnSystem())
                //.Add(new ExplosionCollisionSystem())
                .Add(new ComboSystem())
                .Add(new EnemyAISystem())
                .Add(new EnemySpriteAnimationSystem())
                .Add(new EnemyAttakStateSystem())
                .Add(new EnemyMoveSystem())
                .Add(new DeadEnemyLayDownCountDonwSystem())
                
                .Add(new GameOverSystem())
                
                
                .Add(new PostExplosionCollisionEnemySystem())
                .Add(new PostExplosionCollisionRocksSystem())
                
                .Add(new BurstFlyEnemySystem())
                
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
        GameReady = true;
    }

    IEnumerator SpawnerDelay()
    {
        yield return new WaitForSeconds(0.1f);
        EnemySpawner.Init();
    }
    void Update()
    {
        if (World != null)
        {
            updateSystems.OnUpdate();
        }
    }
    
}
[EcsComponent] public struct SpawnedEvent{}

public class ComboSystem : UpdateSystem
{
    private float comboDelay;
    public override void Update()
    {
        comboDelay += Time.deltaTime;
        if (comboDelay > 1f)
        {
            comboDelay = 0;
            Servise<GameService>.Get().Combo = 0;
        }
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
    public float SpeedRotation;
    public float MaxY;
}
[EcsComponent] public struct ColliderRef
{
    public Collider Value;
}
[EcsComponent] public class Dead{}
public class BurstFlyEnemySystem : UpdateSystem
{
    private FlyEnemyJob j0b;
    private const float DEAD_Y_POS = 0.4f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Quaternion DeadRotation()
    {
        return Quaternion.Euler(90,Random.Range(0,360),90);
    }
    public override void Update()
    {
        j0b.deadpos = DEAD_Y_POS;
        j0b.dt = Time.deltaTime;
        entities.EachWithJob<FlyEnemyJob, FlyWithBurst, TransformComponent>(ref j0b);
        entities.Each((Entity entity, TransformRef transform, ColliderRef Collider, ref FlyWithBurst fly) =>
        {
            if (fly.Grounded)
            {
                entity.Set<Dead>();
                entity.Set<NoBurst>();
                entity.Remove<FlyWithBurst>();
                transform.Value.rotation = DeadRotation();
                Collider.Value.enabled = false;
            }
        });
    }
    struct FlyEnemyJob : IJobExecute<FlyWithBurst, TransformComponent>
    {
        public float deadpos;
        public float dt;
        public void ForEach(ref FlyWithBurst fly, ref TransformComponent transform)
        {
            if (!fly.Grounded)
            {
                transform.position += fly.Direction * fly.Force * dt;
                fly.Delay += dt;
                fly.Direction.y -= dt * 0.5f;
                var rot = transform.rotation.eulerAngles;
                rot.z += fly.SpeedRotation * dt;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, rot.z), 0.1f);
            }

            if (fly.Delay > 1)
            {
                if (transform.position.y < deadpos)
                    fly.Grounded = true;
            }
        }
    }
}
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

            ui.AddKills();
            enemy.NavMeshAgentVelue.enabled = false;
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
                MaxY =  Random.Range(5,15),
                SpeedRotation =  rSpeed
            };
            ref var transform = ref entity.GetRef<TransformComponent>();
            transform.position = transformRef.Value.position;
            transform.rotation = transformRef.Value.rotation;
            entity.Remove<NoBurst>();
            entity.Add(flyForce);
        });
        if (gameService.Combo > MINIMUM_COMBO_SIZE && comboShowDelay > COMBO_SHOW_DELAY)
        {
            Debug.Log($"xxx COMBO xxx {gameService.Combo} xxx COMBO xxx");
            ui.ShowCombo(gameService.Combo);
            comboShowDelay = 0;
        }
    }
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

public class EnemyMoveSystem : UpdateSystem
{
    private float delay;
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<Dead>().Each((EnemyRef enemyRef, RunState runState, ref NoBurst tag) =>
        {
            delay += dt;
            if (delay > 1f)
            {
                enemyRef.NavMeshAgentVelue.destination = enemyRef.MoveToTargetValue.position;
                delay = 0f;
            }

        });
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
[EcsComponent] public class RunState{}
public class EnemyAISystem : UpdateSystem
{
    private const float LOW_AGENT_DISTANCE = 50f;
    public override void Update()
    {
        entities.Without<Dead>().Each((Entity entity, EnemyRef enemy, TransformRef TransformRef) =>
        {
            var transform = TransformRef.Value;
            var distance = Vector3.Distance(transform.position, enemy.MoveToTargetValue.position);
            
            if (distance > LOW_AGENT_DISTANCE)
                enemy.NavMeshAgentVelue.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            else
                enemy.NavMeshAgentVelue.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            
            if (distance <= enemy.AttackRange)
            {
                enemy.State = EnemyState.Attack;
                entity.Set<AttackState>();
                entity.Remove<RunState>();
            }
            else
            {
                enemy.State = EnemyState.Run;
                entity.Set<RunState>();
                entity.Remove<AttackState>();
            }
        });
        
    }
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
            Time.timeScale = 0;
            entity.Destroy();
        });
    }
}
public class EnemySpriteAnimationSystem : UpdateSystem
{
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<Dead>().Each((EnemyRef EnemyRef, SpriteAnim animation, SpriteRender render) =>
        {
            var spriteRenderer = render.Value;
            var spriteAnimation = animation.Value;
            switch (EnemyRef.State)
            {
                case EnemyState.Run:
                    PlayAnimation(ref spriteAnimation.Run, animation.Value, spriteRenderer,dt);
                    break;
                // case EnemyState.Attack:
                //     PlayAnimation(ref spriteAnimation.Attack, animation.Value, spriteRenderer,dt);
                //     break;
                // case EnemyState.Death:
                //     PlayAnimation(ref spriteAnimation.Death, animation.Value, spriteRenderer,dt);
                //     break;
                // case EnemyState.Dead:
                //     break;

            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PlayAnimation(ref Animation animation, SpriteAnimation animator, SpriteRenderer render, float dt)
    {
        animator.CurruntFrameTime += dt;
        if (animator.CurruntFrameTime >= animator.FrameTime)
        {
            animation.CurrentAnimation++;
            if (animation.CurrentAnimation == animation.Frames.Length)
                animation.CurrentAnimation = 0;
            render.sprite = animation.Frames[animation.CurrentAnimation];
            animator.CurruntFrameTime = 0f;
        }
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