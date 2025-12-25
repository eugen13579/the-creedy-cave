using UnityEngine;

/// <summary>
/// Handles all enemy animation logic.
/// Separates animation concerns from main controller.
/// </summary>
public class EnemyAnimationController
{
    private Animator animator;
    private string currentAnimationState;
    private string idleAnimationName;
    private string walkAnimationName;
    private string attack01AnimationName;
    private string attack02AnimationName;
    private string hurtAnimationName;
    private string deathAnimationName;
    private bool enableDebugLogs;
    private bool deathAnimationFinished = false;

    public EnemyAnimationController(
        Animator animator,
        string idleAnimationName,
        string walkAnimationName,
        string attack01AnimationName,
        string attack02AnimationName,
        string hurtAnimationName,
        string deathAnimationName,
        bool enableDebugLogs)
    {
        this.animator = animator;
        this.idleAnimationName = idleAnimationName;
        this.walkAnimationName = walkAnimationName;
        this.attack01AnimationName = attack01AnimationName;
        this.attack02AnimationName = attack02AnimationName;
        this.hurtAnimationName = hurtAnimationName;
        this.deathAnimationName = deathAnimationName;
        this.enableDebugLogs = enableDebugLogs;
        this.currentAnimationState = "";
    }

    public string CurrentAnimationState
    {
        get => currentAnimationState;
        set => currentAnimationState = value;
    }

    /// <summary>
    /// Updates animation based on movement and state.
    /// </summary>
    public void UpdateAnimation(bool isDead, bool isHurt, bool isAttacking, Vector2 movement)
    {
        if (animator == null) return;

        // Don't change animation if dead
        if (isDead) return;

        // Don't change animation if currently hurt
        if (isHurt) return;

        // Handle attack animation completion
        if (isAttacking)
        {
            HandleAttackAnimation();
            return;
        }

        // Determine animation based on movement
        bool isMoving = movement.magnitude > EnemyConstants.MOVEMENT_THRESHOLD;
        string targetAnimation = isMoving ? walkAnimationName : idleAnimationName;

        // Switch animation if needed
        if (currentAnimationState != targetAnimation)
        {
            PlayAnimation(targetAnimation);
        }
        // Restart animation if it finished (for looping)
        else if (currentAnimationState != "" && IsAnimationFinished(currentAnimationState))
        {
            PlayAnimation(currentAnimationState);
        }
    }

    /// <summary>
    /// Handles hurt animation state and completion.
    /// </summary>
    public bool HandleHurtAnimation(bool isHurt, float hurtAnimationStartTime)
    {
        if (!isHurt || animator == null) return false;

        float timeSinceHurtStart = Time.time - hurtAnimationStartTime;

        // Wait for animation to start
        if (timeSinceHurtStart < EnemyConstants.HURT_ANIMATION_START_DELAY)
        {
            return true; // Still waiting
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool hurtAnimExists = !string.IsNullOrEmpty(this.hurtAnimationName);
        bool hurtAnimPlaying = hurtAnimExists && stateInfo.IsName(this.hurtAnimationName);
        bool hurtAnimFinished = hurtAnimPlaying && stateInfo.normalizedTime >= 1.0f;

        if (hurtAnimFinished)
        {
            // Animation finished
            currentAnimationState = "";
            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyAnimationController] Hurt animation finished");
            }
            return false; // No longer hurt
        }
        else if (hurtAnimPlaying)
        {
            return true; // Still playing
        }
        else if (hurtAnimExists && timeSinceHurtStart > EnemyConstants.HURT_ANIMATION_TIMEOUT)
        {
            // Animation doesn't exist in animator
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[EnemyAnimationController] Hurt animation '{this.hurtAnimationName}' not found in Animator Controller");
            }
            currentAnimationState = "";
            return false;
        }
        else if (!hurtAnimExists)
        {
            // Animation doesn't exist, clear state
            return false;
        }

        return true; // Still waiting
    }

    /// <summary>
    /// Plays hurt animation.
    /// </summary>
    public void PlayHurtAnimation()
    {
        if (animator == null) return;

        PlayAnimation(hurtAnimationName);
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyAnimationController] Playing hurt animation: {hurtAnimationName}");
        }
    }

    /// <summary>
    /// Plays death animation.
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator == null) return;
        deathAnimationFinished = false;
        PlayAnimation(deathAnimationName);
        // Ensure animation doesn't loop
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyAnimationController] Playing death animation: {deathAnimationName}");
        }
    }

    /// <summary>
    /// Handles death animation to ensure it plays once and stops at the final frame.
    /// </summary>
    public void HandleDeathAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(deathAnimationName)) return;

        // If death animation has finished, continuously keep it at the final frame
        if (deathAnimationFinished)
        {
            // Continuously set to final frame to prevent any transitions
            animator.Play(deathAnimationName, 0, 1.0f);
            return;
        }

        // If animator is disabled, mark as finished and re-enable to set final frame
        if (!animator.enabled)
        {
            animator.enabled = true;
            animator.Play(deathAnimationName, 0, 1.0f);
            deathAnimationFinished = true;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isDeathAnim = stateInfo.IsName(deathAnimationName);

        if (isDeathAnim)
        {
            // If animation has reached the end, mark as finished and set to final frame
            if (stateInfo.normalizedTime >= 1.0f)
            {
                deathAnimationFinished = true;
                // Set to final frame
                animator.Play(deathAnimationName, 0, 1.0f);
                if (enableDebugLogs)
                {
                    Debug.Log($"[EnemyAnimationController] Death animation finished, locking at final frame");
                }
            }
        }
        else
        {
            // If not in death animation state, check if we should force it
            // Only force if we haven't finished yet (to avoid resetting to first frame)
            if (!deathAnimationFinished)
            {
                animator.Play(deathAnimationName, 0, 0f);
            }
            else
            {
                // Already finished, keep at final frame
                animator.Play(deathAnimationName, 0, 1.0f);
            }
        }
    }

    /// <summary>
    /// Plays attack animation (randomly chooses between attack01 and attack02).
    /// </summary>
    public string PlayAttackAnimation()
    {
        if (animator == null) return "";

        string attackAnimation = Random.Range(0, 2) == 0 ? attack01AnimationName : attack02AnimationName;
        PlayAnimation(attackAnimation);
        return attackAnimation;
    }

    /// <summary>
    /// Checks if attack animation is still playing.
    /// </summary>
    public bool IsAttackAnimationPlaying()
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttackAnim = stateInfo.IsName(attack01AnimationName) || stateInfo.IsName(attack02AnimationName);
        return isAttackAnim && stateInfo.normalizedTime < 1.0f;
    }

    private void HandleAttackAnimation()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttackAnim = stateInfo.IsName(attack01AnimationName) || stateInfo.IsName(attack02AnimationName);

        if (isAttackAnim && stateInfo.normalizedTime >= 1.0f)
        {
            // Attack animation finished
            currentAnimationState = "";
        }
        else if (!isAttackAnim)
        {
            // Attack animation not playing, reset state
            currentAnimationState = "";
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName)) return;
        animator.Play(animationName, 0, 0f);
        currentAnimationState = animationName;
    }

    private bool IsAnimationFinished(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName)) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f;
    }
}


