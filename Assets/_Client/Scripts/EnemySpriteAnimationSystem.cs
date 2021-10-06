using System.Runtime.CompilerServices;
using UnityEngine;
using Wargon.ezs;

public class EnemySpriteAnimationSystem : UpdateSystem
{

    private const float MAX_ANIMATION_DISTANCE = 80;
    public override void Update()
    {
        var dt = Time.deltaTime;
        entities.Without<Dead>().Each((Entity entity, EnemyRef enemy, SpriteAnim animation, SpriteRender render, Damage damage) =>
        {
            if(enemy.DistanceToTarget > MAX_ANIMATION_DISTANCE) return;
            
            var spriteRenderer = render.Value;
            var spriteAnimation = animation.Value;
            switch (enemy.State)
            {
                case EnemyState.Run:
                    PlayAnimation(ref spriteAnimation.Run, animation.Value, spriteRenderer,dt);
                    break;
                case EnemyState.Attack:
                    PlayAnimation(ref spriteAnimation.Attack, animation.Value, spriteRenderer,dt);
                    if (spriteAnimation.Attack.CurrentAnimation == spriteAnimation.AttackFrame && spriteAnimation.AttackFrameEnd)
                    {
                        spriteAnimation.AttackFrameEnd = false;
                        entity.Set<AttackEvent>();
                    }
                    break;
                case EnemyState.Death:
                    SetDeadSprite(ref spriteAnimation.Death, spriteRenderer);
                    break;
                case EnemyState.Dead:
                    
                    break;

            }
        });
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetDeadSprite(ref Animation animation, SpriteRenderer render)
    {
        render.sprite = animation.Frames[0];
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
            animator.AttackFrameEnd = true;
        }
    }
}