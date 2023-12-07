using System;
using UnityEngine;
/// <summary>
/// The indicator beneath player's transform, indicating forward direction
/// </summary>
public class StatusIndicator : MonoBehaviour
{
    [SerializeField] private float translationalSpeed = 15;
    [SerializeField] private float rotationalSpeed = 10.5f;
    [SerializeField] private float positionalOffset = 2;
    [SerializeField] private float positionalOffsetRunning = 1;

    private void Update()
    {
        var t = Player.Instance.transform;
        
        transform.LookAt(CameraManager.Instance.screenspaceLookAt);

        transform.position = Vector3.Lerp(
            transform.position, 
            t.position, 
            translationalSpeed * Time.deltaTime);
    }
}