using System.Collections.Generic;

/// <summary>
/// Selector node: Tries children until one succeeds or is running.
/// Returns Success/Running on first child success, Failure if all fail.
/// </summary>
public class BTSelector : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public BTSelector(params BTNode[] nodes)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Execute(context);
            if (status == NodeStatus.Success || status == NodeStatus.Running)
            {
                return status;
            }
        }
        return NodeStatus.Failure;
    }
}

/// <summary>
/// Sequence node: Executes children in order until one fails or is running.
/// Returns Success if all succeed, Failure/Running on first child failure/running.
/// </summary>
public class BTSequence : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public BTSequence(params BTNode[] nodes)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Execute(EnemyContext context)
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Execute(context);
            if (status == NodeStatus.Failure)
            {
                return NodeStatus.Failure;
            }
            if (status == NodeStatus.Running)
            {
                return NodeStatus.Running;
            }
        }
        return NodeStatus.Success;
    }
}



