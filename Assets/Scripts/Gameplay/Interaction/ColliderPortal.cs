using UnityEngine;

public class ColliderPortal : MonoBehaviour
{
    [SerializeField] private GameScene destinationScene;
    [SerializeField] private Vector3 teleportPosition;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DataPersistenceManager.Instance.ModifyPosition(teleportPosition);
            Loader.Load(destinationScene);
        }
    }
}