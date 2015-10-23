using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {
    public GameObject Target;
    Transform T_Targ;
    Transform T_Self;
	// Use this for initialization
	void Start () {
        Target = GameObject.Find("Main Camera");
        T_Self = GetComponent<Transform>();
        T_Targ = Target.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        print(T_Self.position - new Vector3(T_Targ.position.x, T_Self.position.y, T_Targ.position.z));
        T_Self.rotation = Quaternion.LookRotation(T_Self.position - new Vector3(T_Targ.position.x, T_Self.position.y, T_Targ.position.z));
	}
}
