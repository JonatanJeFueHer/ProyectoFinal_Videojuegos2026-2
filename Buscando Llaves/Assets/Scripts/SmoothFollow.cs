using UnityEngine;
public class SmoothFollow : MonoBehaviour {
public Transform target;
public float distance = 5f;
public float height = 2f;
public float smoothSpeed = 10f;
void LateUpdate(){
Vector3 desiredPos = target.position - target.forward * distance
+ Vector3.up * height;
transform.position = Vector3.Lerp(transform.position,
desiredPos, smoothSpeed * Time.deltaTime);
transform.LookAt(target);
}
}