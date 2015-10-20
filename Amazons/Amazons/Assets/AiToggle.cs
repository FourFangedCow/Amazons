using UnityEngine;
using System.Collections;

public class AiToggle : MonoBehaviour {
    public int ID = 0;
    GameManager es;
	// Use this for initialization
	void Start () {
	    es = GameObject.Find("EventSystem").GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Toggle(bool b) {
        if (b)
            es.AddAI(ID);
        else
            es.RemoveAI(ID);
    }
}
