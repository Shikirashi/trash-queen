using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetInstantiate : MonoBehaviour{
	public int targetAmount;
	public GameObject targetPrefab;
	GameObject[] targets;

	void Start(){
		for (int i = 0; i < targetAmount; i++) {
			Vector3 spawnPoint = new Vector3(Random.Range(-450f, 450f), -2.7f, Random.Range(-450f, 450f));
			Collider[] hits = Physics.OverlapSphere(spawnPoint, 3f, LayerMask.GetMask("wallMask"));
			if(hits.Length == 0) {
				var trashChild = Instantiate(targetPrefab, spawnPoint, Quaternion.identity);
				trashChild.transform.parent = transform;
			}
			else {
				i--;
			}
		}
	}

	public void RefreshTargets() {
		targets = null;
		targets = GameObject.FindGameObjectsWithTag("Trashcan");
		foreach(GameObject can in targets) {
			Destroy(can);
		}


		for (int i = 0; i < targetAmount; i++) {
			Vector3 spawnPoint = new Vector3(Random.Range(-450f, 450f), -2.73f, Random.Range(-450f, 450f));
			Collider[] hits = Physics.OverlapSphere(spawnPoint, 3f, LayerMask.GetMask("wallMask"));
			if (hits.Length == 0) {
				var trashChild = Instantiate(targetPrefab, spawnPoint, Quaternion.identity);
				trashChild.transform.parent = transform;
			}
			else {
				i--;
			}
		}
	}
}
