using UnityEngine;

public class ColliderPortal : MonoBehaviour
{
    [SerializeField] private GameScene destinationScene;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Loader.Load(destinationScene);
        }
    }
}