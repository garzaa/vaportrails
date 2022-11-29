using UnityEngine;
using XNode;

[CreateAssetMenu]
public class AttackGraph : NodeGraph {
    public string stateMachineName = "GroundAttacks";

    public CombatNode GetRootNode() {
        foreach (Node i in nodes) {
            if (i is InitialBranchNode) return i as CombatNode;
        }
        return null;
    }
}
