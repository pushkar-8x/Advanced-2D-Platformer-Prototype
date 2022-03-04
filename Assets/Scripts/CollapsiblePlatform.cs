using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsiblePlatform : MonoBehaviour
{
    public float fallSpeed = 10f;
    public float delayTime = 0.5f;

    public Vector3 difference;

    private bool _platformCollapsing;

    private Rigidbody2D _rigidbody;
    private Vector3 _lastPosition;


    private void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _lastPosition = transform.position;

        if(_platformCollapsing)
        {
            _rigidbody.AddForce(Vector2.down * fallSpeed);
            if(_rigidbody.velocity.y==0)
            {
                _platformCollapsing = true;
                _rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }
    }


    private void LateUpdate()
    {
        difference = transform.position - _lastPosition;
    }
    public void CollapsePlatform()
    {
        StartCoroutine("CollapsingPlatform");
    }

    private IEnumerator CollapsingPlatform()
    {
        yield return new WaitForSeconds(delayTime);
        _platformCollapsing = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody.freezeRotation = true;
        _rigidbody.gravityScale = 1f;
        _rigidbody.mass = 1000f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;

    }
}
