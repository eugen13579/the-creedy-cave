using UnityEngine;

/// <summary>
/// Handles debug visualization for enemy AI.
/// Separates debug code from game logic.
/// </summary>
public class EnemyDebugDisplay : MonoBehaviour
{
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showOnScreenDebug = true;
    [SerializeField] private bool showDetailedGizmos = true;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
    [SerializeField] private Vector2 debugTextOffset = new Vector2(0, 50);

    private bool debugDisplayEnabled = true;
    private EnemyContext context;
    private Transform enemyTransform;
    private Rigidbody2D rb;
    private float detectionRange;
    private float attackRange;
    private float chaseSpeed;
    private bool usePathfinding;
    private SimplePathfinding2D pathfinding;

    public void Initialize(EnemyContext context, Transform enemyTransform, Rigidbody2D rb, 
        float detectionRange, float attackRange, float chaseSpeed, bool usePathfinding, 
        SimplePathfinding2D pathfinding)
    {
        this.context = context;
        this.enemyTransform = enemyTransform;
        this.rb = rb;
        this.detectionRange = detectionRange;
        this.attackRange = attackRange;
        this.chaseSpeed = chaseSpeed;
        this.usePathfinding = usePathfinding;
        this.pathfinding = pathfinding;
    }

    void Update()
    {
        // Toggle debug display with key
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugDisplayEnabled = !debugDisplayEnabled;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || enemyTransform == null) return;

        // Draw detection range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
        Gizmos.DrawWireSphere(enemyTransform.position, detectionRange);

        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
        Gizmos.DrawWireSphere(enemyTransform.position, attackRange);

        // Draw direction to player if found
        if (context?.PlayerTransform != null)
        {
            Vector3 direction = (context.PlayerTransform.position - enemyTransform.position);
            float distance = direction.magnitude;
            direction.Normalize();

            // Draw line to player
            Gizmos.color = distance <= detectionRange ? Color.green : Color.gray;
            Gizmos.DrawRay(enemyTransform.position, direction * Mathf.Min(distance, detectionRange));

            // Draw current velocity
            if (rb != null && rb.linearVelocity.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(enemyTransform.position, rb.linearVelocity * 0.5f);
            }

            // Draw movement vector
            if (context?.Movement.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(enemyTransform.position, context.Movement * 2f);
            }

            // Detailed gizmos
            if (showDetailedGizmos)
            {
                // Draw player position marker
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(context.PlayerTransform.position, 0.3f);

                // Draw pathfinding direction if using pathfinding
                if (usePathfinding && pathfinding != null && distance <= detectionRange)
                {
                    Vector2 pathDirection = pathfinding.GetDirectionToTarget(
                        enemyTransform.position,
                        context.PlayerTransform.position
                    );
                    if (pathDirection.magnitude > EnemyConstants.MOVEMENT_THRESHOLD)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawRay(enemyTransform.position, pathDirection * 3f);
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (!showOnScreenDebug || !debugDisplayEnabled || context == null) return;
        if (context.PlayerTransform == null) return;

        // Get screen position of enemy
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemyTransform.position + (Vector3)debugTextOffset);

        // Only show if enemy is on screen
        if (screenPos.z < 0) return;

        // Convert to GUI coordinates (Y is flipped)
        float guiY = Screen.height - screenPos.y;

        // Create debug text
        string debugText = $"Enemy: {gameObject.name}\n";
        debugText += $"Distance: {context.DistanceToPlayer:F2}m\n";
        debugText += $"Detection Range: {detectionRange}m\n";
        debugText += $"Attack Range: {attackRange}m\n";
        debugText += $"Is Chasing: {context.IsChasing}\n";
        debugText += $"Is Attacking: {context.IsAttacking}\n";
        debugText += $"Is Hurt: {context.IsHurt}\n";
        debugText += $"Is Dead: {context.IsDead}\n";

        if (rb != null)
        {
            debugText += $"Velocity: {rb.linearVelocity.magnitude:F2} m/s\n";
            debugText += $"Velocity: ({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2})\n";
        }

        debugText += $"Movement: ({context.Movement.x:F2}, {context.Movement.y:F2})\n";
        debugText += $"Chase Speed: {chaseSpeed}\n";
        debugText += $"Use Pathfinding: {usePathfinding}\n";

        if (context.EnemyHealth != null)
        {
            debugText += $"Health: {context.EnemyHealth.CurrentHealth:F0}/{context.EnemyHealth.MaxHealth:F0}\n";
        }

        // Calculate text size
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f)); // Semi-transparent black background

        Vector2 textSize = style.CalcSize(new GUIContent(debugText));

        // Draw background box
        GUI.Box(new Rect(screenPos.x, guiY, textSize.x + 10, textSize.y + 10), "", style);

        // Draw text
        GUI.Label(new Rect(screenPos.x + 5, guiY + 5, textSize.x, textSize.y), debugText, style);
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}



