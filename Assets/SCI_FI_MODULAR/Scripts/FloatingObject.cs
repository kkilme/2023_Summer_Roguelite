using UnityEngine;
using System.Collections;

public class FloatingObject : MonoBehaviour {
	private Vector3 mAxis;
	private float mAngle;
	public float mRotationSpeed = 0.1f;
	public float returnSpeed = 1.0f;
	private Vector3 orgPos;
	// Use this for initialization
	void Start () {
		Random.rotationUniform.ToAngleAxis(out mAngle,out mAxis);
		orgPos = transform.position;


	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation *= Quaternion.AngleAxis (mAngle * mRotationSpeed * Time.deltaTime, mAxis);
	}

	void FixedUpdate()
	{
		Rigidbody r = GetComponent<Rigidbody> ();
		r.WakeUp ();
		Vector3 pos = transform.position;
		transform.position = Vector3.Lerp (pos, orgPos, Time.fixedDeltaTime * returnSpeed);
	}
}
