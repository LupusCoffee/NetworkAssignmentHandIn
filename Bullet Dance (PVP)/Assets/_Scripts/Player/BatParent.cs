using UnityEngine;

public class BatParent : MonoBehaviour
{
    public Vector2 MousePosition {  get; set; }

    private void Update()
    {
        transform.up = (MousePosition - (Vector2)transform.position).normalized;
    }
}