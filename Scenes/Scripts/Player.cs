using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed;
    private Vector2 dir;
    private void Start() 
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        Vector2 velocity = new Vector2(dir.x * speed * Time.deltaTime, dir.y * speed * Time.deltaTime);
        rb.velocity = velocity;  
    }
    public void Walk(InputAction.CallbackContext value)
    {
        dir = value.ReadValue<Vector2>();
    }
}
