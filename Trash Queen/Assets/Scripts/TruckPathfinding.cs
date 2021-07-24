using UnityEngine;

[RequireComponent(typeof(TruckMotor))]
public class TruckPathfinding : MonoBehaviour{
	public float wheelSize = 10f;
	public float cargoCap = 100f;
	public float fuelCap = 100f;

	public float[] truckGene = new float[3];

	public float cargoSize = 0f;
	public float fuelConsumption = 0.02f;
	public float fuelNow;

	public float dumpedTrash = 0f;

	public LayerMask selectMask;
	public GameObject TargetsGroup;
	public MeshRenderer truckBody;
	public MeshRenderer trashRenderer;
	public Material wallMat;
	public Transform closestTrashCan;
	public bool hasSelected = false;
	public bool isCargoFull = false;
	public bool isEnRoute = false;
	public bool isGoingToRefuel = false;
	public bool atQueen = false;
	public bool isRefueling = false;
	public bool isMoving = false;

	GameObject[] TrashCans;
	GameObject[] GasStations;
	GameObject TrashQueen;
	TruckMotor motor;
	Rigidbody rb;
	Collider truckCollider;
	Camera cam;
	Vector3 selectedTarget;
	TargetControl trashCan;
	TargetControl oldTrashCan;
	float distanceToTrashCan;
    void Start(){
		cam = Camera.main;
		trashRenderer.enabled = false;
		rb = GetComponent<Rigidbody>();
		truckCollider = GetComponent<SphereCollider>();
		TargetsGroup = GameObject.Find("TargetsGroup");
		motor = GetComponent<TruckMotor>();
		wheelSize = truckGene[0];
		cargoCap = truckGene[1];
		fuelCap = truckGene[2];
		fuelNow = fuelCap;
		GasStations = GameObject.FindGameObjectsWithTag("GasStation");
		fuelConsumption = 0.2f;
	}

	private void Awake() {
		wheelSize = truckGene[0];
		cargoCap = truckGene[1];
		fuelCap = truckGene[2];
		fuelNow = fuelCap;
		GasStations = GameObject.FindGameObjectsWithTag("GasStation");
		fuelConsumption = 0.2f;
	}

	void FixedUpdate() {
		if (cargoSize > 0f) {
			fuelConsumption = cargoSize * 0.2f;
			fuelConsumption = Mathf.Clamp(fuelConsumption, 0f, fuelCap);
		}
		if (motor.agent.velocity != Vector3.zero) {
			fuelNow -= fuelConsumption * Time.fixedDeltaTime * Time.timeScale;
			fuelNow = Mathf.Clamp(fuelNow, 0f, fuelCap);
		}
		if (fuelNow <= 0f) {
			if(dumpedTrash <= 0f) {
				motor.agent.enabled = false;
				Destroy(gameObject);
			}
			else if (dumpedTrash > 0f) {
				if (motor.agent.enabled) {
					motor.agent.enabled = false;
					truckBody.material = wallMat;
				}
				if (truckCollider.enabled) {
					truckCollider.enabled = false;
					rb.detectCollisions = false;
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
					rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
					rb.Sleep();
				}
			}
		}
		if ((fuelNow / fuelCap) <= 0.20f && fuelNow > 0f) {
			//go to gas station
			hasSelected = false;
			Refuel();
		}
		else if (cargoSize >= cargoCap || isCargoFull) {
			hasSelected = false;
			isCargoFull = true;
			trashRenderer.enabled = true;
			DumpCargo();
		}
		if ((cargoSize < cargoCap) && !isEnRoute && !isGoingToRefuel) {
			if (!hasSelected) {
				SelectTrashCan();
			}
			if (selectedTarget != null) {
				distanceToTrashCan = Vector3.Distance(transform.position, selectedTarget);
				if (distanceToTrashCan <= 5f) {
					PickUpTrash();
				}
			}
			if(closestTrashCan == null) {
				hasSelected = false;
			}
		}
	}

	void SelectTrashCan() {
		if(TargetsGroup.transform.childCount > 0) {
			closestTrashCan = GetClosestTrashCan();
			if(closestTrashCan != null) {
				selectedTarget = closestTrashCan.position;
				if (trashCan != null) {
					oldTrashCan = trashCan;
				}
				trashCan = closestTrashCan.GetComponent<TargetControl>();
				hasSelected = true;
				motor.UpdateStatus();
				motor.MoveToPoint(selectedTarget);
			}
		}
		else if(TargetsGroup.transform.childCount <= 0) {
			closestTrashCan = null;
			hasSelected = false;
		}
	}

	void PickUpTrash() {
		if(trashCan.TrashAmount > 0) {
			trashCan.TrashAmount -= 10 * Time.fixedDeltaTime * Time.timeScale;
			cargoSize += 10 * Time.fixedDeltaTime * Time.timeScale;
			cargoSize = Mathf.Clamp(cargoSize, 0f, cargoCap);
		}
		if(trashCan.TrashAmount <= 0) {
			hasSelected = false;
		}
	}

	public Transform GetClosestTrashCan() {
		TrashCans = GameObject.FindGameObjectsWithTag("Trashcan");
		float closestDistance = Mathf.Infinity;
		Transform targetTransform = null;

		foreach(GameObject TargetTrashcan in TrashCans) {
			float currentDistance;
			currentDistance = Vector3.Distance(transform.position, TargetTrashcan.transform.position);
			if (currentDistance < closestDistance) {
				if (!hasSelected) {
					closestDistance = currentDistance;
					targetTransform = TargetTrashcan.transform;
				}
			}
		}
		return targetTransform;
	}

	void DumpCargo() {
		if (!isEnRoute) {
			isEnRoute = true;
			TrashQueen = GameObject.FindGameObjectWithTag("TrashQueen");
			motor.agent.SetDestination(TrashQueen.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)));
			//Debug.Log("Now moving to trash queen");
		}
		if (atQueen) {
			if (cargoSize > 0) {
				cargoSize -= 10 * Time.fixedDeltaTime * Time.timeScale;
				dumpedTrash += 10 * Time.fixedDeltaTime * Time.timeScale;
				cargoSize = Mathf.Clamp(cargoSize, 0f, cargoCap);
			}
			if (cargoSize <= 0) {
				isEnRoute = false;
				isGoingToRefuel = false;
				isCargoFull = false;
				atQueen = false;
				trashRenderer.enabled = false;
				//Debug.Log("Truck has dumped " + dumpedTrash + " kg of trash");
			}
		}
	}

	void Refuel() {
		if (!isGoingToRefuel) {
			isGoingToRefuel = true;
			float closestDistance = Mathf.Infinity;
			Transform targetGasStation = null;

			foreach (GameObject station in GasStations) {
				float currentDistance;
				currentDistance = Vector3.Distance(transform.position, station.transform.position);
				if (currentDistance < closestDistance) {
					closestDistance = currentDistance;
					targetGasStation = station.transform;
				}
			}
			motor.agent.SetDestination(targetGasStation.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)));
			//Debug.Log("Now moving to " + motor.agent.destination);
		}
		if (isRefueling) {
			fuelNow = fuelCap;
			if(fuelNow >= fuelCap) {
				isRefueling = false;
				isGoingToRefuel = false;
				isEnRoute = false;
				Mathf.Clamp(fuelNow, 0f, fuelCap);
				//Debug.Log("Truck " + transform.name + " has finished refueling");
			}
		}
	}

	private void OnTriggerEnter(Collider other) {
		if(other.tag == "TrashQueen") {
			atQueen = true;
		}
		else if (other.tag == "GasStation") {
			isRefueling = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.tag == "TrashQueen") {
			atQueen = false;
		}
		else if (other.tag == "GasStation") {
			isRefueling = false;
		}
	}
}
