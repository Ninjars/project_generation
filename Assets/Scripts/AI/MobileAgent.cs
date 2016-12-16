using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MobileAgent : BaseAgent {

    protected int speed;
    private float initialSpeed;
    private float acceleration;
    private float terminalSpeed;
    private Vector3 currentVelocity = Vector3.zero;
    private float decelerationBuffer = 1f;

    public void setSpeed(float val) {
        terminalSpeed = val / 10f;
        initialSpeed = (val / 10f) / 2f;
        acceleration = (val / 10f) / 2f;
        Debug.Log("set speed " + val + " " + terminalSpeed + " " + acceleration);
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
        transform.position += currentVelocity * Time.fixedDeltaTime;

        if (dist <= nextAction.getMaxTriggerRange()) {
            nextAction.setInRange(true);
            currentVelocity = Vector3.zero;
            return true;
        } else {
            return false;
        }
    }
}
