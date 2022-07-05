using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoofyAhhMovement : MonoBehaviour
{

    public float speed = 5f;
    private float health = 100f;
    public Rigidbody2D rb;

    Vector2 movement;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // inputs
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // movement
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
