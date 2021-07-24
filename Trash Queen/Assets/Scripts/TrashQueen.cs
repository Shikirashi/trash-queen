using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashQueen : MonoBehaviour{
	TruckPathfinding truckSimp;
	bool isDumping;

    void Start(){
        
    }

    void Update(){
        
    }

	private void OnTriggerEnter(Collider other) {
		if(other.tag == "Truck") {
			truckSimp = other.GetComponent<TruckPathfinding>();
		}
	}
}
