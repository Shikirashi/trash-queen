using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetControl : MonoBehaviour{
	public float TrashAmount;
	public bool isOccupied;
	public Transform occupier;

    void Start(){
		TrashAmount = 500;
		occupier = null;
    }

    void FixedUpdate(){
        if(TrashAmount <= 0) {
			Destroy(gameObject);
		}
    }

	private void OnTriggerEnter(Collider collision) {
		if (!isOccupied) {
			if (collision.tag == "Truck") {
				isOccupied = true;
				occupier = collision.transform;
			}
		}
	}

	private void OnTriggerExit(Collider collision) {
		if (isOccupied) {
			if(collision.transform == occupier) {
				isOccupied = false;
				occupier = null;
			}
		}
	}
}
