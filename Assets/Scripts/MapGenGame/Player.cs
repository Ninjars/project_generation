using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenGame {
    public class Player : MonoBehaviour {

        Rigidbody body;
        Vector3 velocity;

    	// Use this for initialization
    	void Start () {
            body = GetComponent<Rigidbody>();
    	}
    	
    	// Update is called once per frame
    	void Update () {
            velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * 10;
    	}

        void FixedUpdate() {
            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        }
    }
}
