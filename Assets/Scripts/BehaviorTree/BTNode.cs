using UnityEngine;

/// <summary>
/// Status returned by behavior tree nodes.
/// </summary>
public enum NodeStatus
{
    Success,
    Failure,
    Running
}

/// <summary>
/// Base class for all behavior tree nodes.
/// </summary>
public abstract class BTNode
{
    /// <summary>
    /// Executes the node's logic.
    /// </summary>
    /// <param name="context">Enemy context containing state and references</param>
    /// <returns>NodeStatus indicating success, failure, or running</returns>
    public abstract NodeStatus Execute(EnemyContext context);
}


