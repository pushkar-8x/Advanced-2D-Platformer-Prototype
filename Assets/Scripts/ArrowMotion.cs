using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ArrowMotion : MonoBehaviour
{
    public float arrowSpeed = 5f;
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 direction = Camera.main.WorldToScreenPoint(Mouse.current.position.ReadValue()) - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle > 180)
            {
                angle = angle%180;
            }
            
                transform.rotation = Quaternion.AngleAxis(angle * arrowSpeed, Vector3.forward);

        }
    }
}
