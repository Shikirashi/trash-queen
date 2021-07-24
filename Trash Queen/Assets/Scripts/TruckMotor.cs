using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TruckMotor : MonoBehaviour{

	public NavMeshAgent agent;
	public TruckPathfinding pathfinder;

    void Start(){
		agent = GetComponent<NavMeshAgent>();
		pathfinder = GetComponent<TruckPathfinding>();
		agent.speed = (pathfinder.wheelSize * 20f) / (pathfinder.cargoSize / 30f + 1) - (pathfinder.fuelNow / 30f + 1);
		agent.angularSpeed = (3000f / pathfinder.wheelSize) / (pathfinder.cargoSize / 30f + 1);
		agent.acceleration = (250f / pathfinder.wheelSize) / (pathfinder.cargoSize / 30f + 1);
	}

    public void MoveToPoint(Vector3 point){
		if (agent.enabled) {
			agent.SetDestination(point);
		}
    }

	public void UpdateStatus() {
		agent.speed = (pathfinder.wheelSize * 20f) / (pathfinder.cargoSize / 30f + 1) - (pathfinder.fuelNow/30f + 1);
		agent.angularSpeed = (3000f / pathfinder.wheelSize) / (pathfinder.cargoSize / 30f + 1);
		agent.acceleration = (250f / pathfinder.wheelSize) / (pathfinder.cargoSize / 30f + 1);
	}
}
