using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;

    private Rigidbody rb;
    void Start()
    {
        Camera.main.enabled = false;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        float h_Move = Input.GetAxis("Horizontal");
        float v_Move = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h_Move, 0, v_Move);
        direction.Normalize();

        rb.velocity = transform.TransformDirection(direction) * speed * Time.deltaTime;
    }
}
