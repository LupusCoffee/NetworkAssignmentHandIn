using UnityEngine;

public class StartArea : MonoBehaviour
{
    public delegate void StartAreaOccupied();
    public static event StartAreaOccupied onStartAreaOccupied;
    public delegate void StartAreaNotOccupied();
    public static event StartAreaNotOccupied onStartAreaNotOccupied;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
            onStartAreaOccupied?.Invoke();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
            onStartAreaNotOccupied?.Invoke();
    }
}
