using System;
using UnityEngine;
/// <summary>
/// The indicator beneath player's transform, indicating forward direction
/// </summary>
public class ForwardIndicator : MonoBehaviour
{
    [SerializeField] private float translationalSpeed = 15;
    [SerializeField] private float rotationalSpeed = 10.5f;
    [SerializeField] private float positionalOffset = 2;
    [SerializeField] private float positionalOffsetRunning = 1;
    
    private void Update()
    {
        var t = Player.Instance.transform;
        
        transform.rotation = 
            Quaternion.Slerp(transform.rotation, t.rotation, rotationalSpeed * Time.deltaTime);

        var runOffset = Player.Instance.IsRunning() ? positionalOffsetRunning : 0;
        transform.position = Vector3.Lerp(
            transform.position, 
            t.position + t.forward * (positionalOffset + runOffset), 
            translationalSpeed * Time.deltaTime);
    }
}