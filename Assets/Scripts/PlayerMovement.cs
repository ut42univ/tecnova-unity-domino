using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float sprintSpeed = 9f;
    public float maxVelocityChange = 10f;
    [Space]
    public float jumpHeight = 3f;
    public LayerMask groundMask = 1;
    public float groundCheckDistance = 0.1f;

    private Vector2 input;
    private Rigidbody rb;

    private bool isSprinting;
    private bool isJumping;

    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Normalize();

        isSprinting = Input.GetButton("Sprint");
        isJumping = Input.GetButton("Jump");

        CheckGrounded();
    }

    void FixedUpdate()
    {
        rb.AddForce(CalcMovement(isSprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);

        // ジャンプ処理
        if (isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down,
            GetComponent<Collider>().bounds.extents.y + groundCheckDistance, groundMask);
    }

    Vector3 CalcMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);

        targetVelocity *= _speed;

        Vector3 velocity = rb.velocity;

        if (input.magnitude > 0.1f)
        {
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

            velocityChange.y = 0;

            return velocityChange;
        }
        else
        {
            Vector3 stopForce = new Vector3(-velocity.x, 0, -velocity.z);
            return stopForce;
        }
    }
}
