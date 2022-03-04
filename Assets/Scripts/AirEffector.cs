using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class AirEffector : MonoBehaviour
{

    public AirEffectorType airEffectorType;
    public float speed;
    public Vector2 direction;
    private BoxCollider2D _collider;

    private void Start()
    {
        direction = transform.up;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void DeactivateEffector()
    {
        StartCoroutine("DeactivateEffectorCoroutine");
    }

    private IEnumerator DeactivateEffectorCoroutine()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        _collider.enabled = true;
    }



}
