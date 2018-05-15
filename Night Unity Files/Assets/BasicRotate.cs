using UnityEngine;

public class BasicRotate : MonoBehaviour
{
	public float RotateSpeed = 5f;
	
	public void Update () 
	{
		transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);	
	}
}
