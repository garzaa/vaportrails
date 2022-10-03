using UnityEngine;

[CreateAssetMenu(fileName = "Transition", menuName = "Data/Runtime/Transition")]
public class Transition : ScriptableObject {
    public bool subway;
    public Vector2 position;

    public void Clear() {
        subway = false;
        position = Vector2.zero;
    }

    public bool IsEmpty() {
        return (subway == false) && (position.Equals(Vector2.zero));
    }
}
