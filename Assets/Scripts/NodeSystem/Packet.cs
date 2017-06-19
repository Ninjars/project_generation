using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public class Packet : MonoBehaviour {

        public int speed;
        private float acceleration;
        private float terminalSpeed;
        private Vector3 currentVelocity = Vector3.zero;
        private float decelerationBuffer = 1f;
        private float completionDistance = 0.5f;
        public GameNode target;
        private Player owner;

    	// Use this for initialization
    	void Start () {
            terminalSpeed = speed / 10f;
            acceleration = (speed / 10f) / 2f;
    	}
    	
    	// Update is called once per frame
    	void FixedUpdate () {
            move();
    	}

        private void move() {
            if (target == null) {
                Debug.Log("Packet: no target; destroying");
                GameObject.Destroy(gameObject);
                return;
            }
            Vector3 targetPosition = target.getPosition();
            float dist = Vector3.Distance(transform.position, targetPosition);
            Vector3 moveDirection = Vector3.Normalize(targetPosition - transform.position);
            float currentSpeed = currentVelocity.magnitude;
            float stoppingDistance = currentSpeed * currentSpeed / (2 * acceleration) * Time.fixedDeltaTime;

            if (stoppingDistance > dist + decelerationBuffer) {
                currentVelocity -= moveDirection * acceleration;
            } else if (stoppingDistance < dist - decelerationBuffer) {
                currentVelocity += moveDirection * acceleration;
            }

            Vector3.ClampMagnitude(currentVelocity, terminalSpeed);

            transform.position += currentVelocity * Time.fixedDeltaTime;

            if (dist <= completionDistance) {
                currentVelocity = Vector3.zero;
                target.onPacket(this);
                GameObject.Destroy(gameObject);
            }
        }

        public void setOwner(Player player) {
            owner = player;
        }

        public Player getOwner() {
            return owner;
        }
    }
}
