using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MobileAgent : BaseAgent {

    protected int speed;
    private float acceleration;
    private float terminalSpeed;
    private Vector3 currentVelocity = Vector3.zero;
    private float decelerationBuffer = 1f;
    private BoxCollider midCollider;
    private BoxCollider farCollider;

    protected void start(float speed) {
        setSpeed(speed);
        GlobalRegister.addMobileAgent(this);
        midCollider = gameObject.AddComponent<BoxCollider>();
        farCollider = gameObject.AddComponent<BoxCollider>();
    }

    public void setSpeed(float val) {
        terminalSpeed = val / 10f;
        acceleration = (val / 10f) / 2f;
    }

    public override bool moveAgent(GOAPAction nextAction) {
        Vector3 targetPosition = nextAction.target.transform.position;
        float dist = Vector3.Distance(transform.position, targetPosition);
        Vector3 moveDirection = Vector3.Normalize(targetPosition - transform.position);
        float currentSpeed = currentVelocity.magnitude;
        float stoppingDistance = currentSpeed * currentSpeed / (2 * acceleration) * Time.fixedDeltaTime;

        if (stoppingDistance > dist + decelerationBuffer) {
            //Debug.Log("decelerating");
            currentVelocity -= moveDirection * acceleration;
        } else if (stoppingDistance < dist - decelerationBuffer) {
            //Debug.Log("accelerating");
            currentVelocity += moveDirection * acceleration;
        }

        // todo: more intelligent agent movement that compensates for its own velocity and so can avoid getting stuck in orbits
        // todo: implement actual path finding algo!
        
        Vector3.ClampMagnitude(currentVelocity, terminalSpeed);

        updateColliders(currentVelocity);

        transform.position += currentVelocity * Time.fixedDeltaTime;

        if (dist <= nextAction.getMaxTriggerRange()) {
            nextAction.setInRange(true);
            currentVelocity = Vector3.zero;
            return true;
        } else {
            return false;
        }
    }

    private void updateColliders(Vector3 velocity) {
        float magnitude = velocity.magnitude;
        midCollider.size.Set(1, 1, magnitude / 2);
        midCollider.center.Set(0.5f, 0.5f, magnitude / 4f);
        farCollider.size.Set(1, 1, magnitude / 2);
        farCollider.center.Set(0.5f, 0.5f, magnitude * 3 / 4f);
    }

    private List<MobileAgent> getPotentialCollidingAgents() {
        Vector3 scanOffset = gameObject.transform.position + currentVelocity;
        Collider[] foundObjects = Physics.OverlapSphere(scanOffset, scanOffset.magnitude);
        List<MobileAgent> collidingAgents = new List<MobileAgent>();
        foreach (Collider hit in foundObjects) {
            Transform parent = hit.transform.parent;
            if (parent == null) {
                continue;
            }
            MobileAgent agent = parent.gameObject.GetComponent<MobileAgent>();
            if (agent != null && onCollisionCourse(agent)) {
                collidingAgents.Add(agent);
            }
        }
        return collidingAgents;
    }

    private bool onCollisionCourse(MobileAgent other) {

        return false;
    }

    void OnCollisionStay(Collision collisionInfo) {
        foreach (ContactPoint contact in collisionInfo.contacts) {
            Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
        }
    }
}
