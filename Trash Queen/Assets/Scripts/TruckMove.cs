using UnityEngine;

public class TruckMove : MonoBehaviour{
	private Rigidbody rb;
	public float speed;
	public float turnSpeed;
	public float gravityMultiplier;
	public float weight;
    void Start(){
		rb = GetComponent<Rigidbody>();
    }
	
    void FixedUpdate(){
		if (Input.GetKey(KeyCode.W)) {
			rb.AddRelativeForce(Vector3.forward * speed * Time.deltaTime * 100);
		}
		if (Input.GetKey(KeyCode.S)) {
			rb.AddRelativeForce(-Vector3.forward * speed * Time.deltaTime * 100);
		}
		Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
		localVelocity.x = 0;
		rb.velocity = transform.TransformDirection(localVelocity);

		if (Input.GetKey(KeyCode.D)) {
			rb.AddTorque(Vector3.up * turnSpeed * Time.deltaTime * 100);
		}
		else if (Input.GetKey(KeyCode.A)) {
			rb.AddTorque(-Vector3.up * turnSpeed * Time.deltaTime * 100);
		}

		rb.AddForce(Vector3.down * (gravityMultiplier + weight) * Time.deltaTime * 100);
	}
}
