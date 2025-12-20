using UnityEngine;

/// <summary>
/// Handles initialization of enemy components.
/// Separates component setup logic from main controller.
/// </summary>
public static class EnemyComponentInitializer
{
    public struct InitializedComponents
    {
        public Rigidbody2D Rigidbody;
        public SpriteRenderer SpriteRenderer;
        public Animator Animator;
        public EnemyHealth EnemyHealth;
        public EnemyHealthBar HealthBar;
        public SimplePathfinding2D Pathfinding;
    }

    public static InitializedComponents InitializeComponents(GameObject enemyObject, bool enableDebugLogs)
    {
        var components = new InitializedComponents();

        // Initialize Rigidbody2D
        components.Rigidbody = GetOrAddComponent<Rigidbody2D>(enemyObject);
        if (components.Rigidbody != null)
        {
            components.Rigidbody.gravityScale = 0;
            components.Rigidbody.freezeRotation = true;
            components.Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // Use Kinematic to prevent enemy from pushing player
            // Enemy can still move and detect collisions, but won't push other rigidbodies
            components.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Enemy {enemyObject.name}] Rigidbody2D setup: gravityScale={components.Rigidbody.gravityScale}, bodyType={components.Rigidbody.bodyType}, freezeRotation={components.Rigidbody.freezeRotation}");
            }
        }

        // Initialize SpriteRenderer
        components.SpriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        if (components.SpriteRenderer == null)
        {
            Debug.LogWarning($"[Enemy {enemyObject.name}] SpriteRenderer not found. Sprite flipping will not work.");
        }

        // Initialize Animator
        components.Animator = enemyObject.GetComponent<Animator>();
        if (components.Animator == null)
        {
            Debug.LogWarning($"[Enemy {enemyObject.name}] Animator not found. Animation will not work.");
        }

        // Initialize EnemyHealth
        components.EnemyHealth = GetOrAddComponent<EnemyHealth>(enemyObject);

        // Initialize EnemyHealthBar
        components.HealthBar = GetOrAddComponent<EnemyHealthBar>(enemyObject);

        // Initialize Pathfinding
        components.Pathfinding = GetOrAddComponent<SimplePathfinding2D>(enemyObject);

        return components;
    }

    private static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        return component;
    }
}

