using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class GenerationManager : MonoBehaviour{
	public int population = 0;
	public float fitnessSum = 0f;
	public float bestFitness = 0f;
	public float currentFitness = 0f;
	public float overallBestFitness = 0f;
	public float[] fitnessSumTracker = new float[100];
	public float[] populationFitness;
	public float[] populationProbability;
	public float[] cumulativeChance;
	public float[,] survivorGenes;
	public float mutationRate = 0.6f;
	public int populationFitnessCounter = 0;
	public GameObject truckPrefab;
	public GameObject resultsGroup;
	public GameObject lastResultsGroup;
	public TextMeshProUGUI wheelSizeText;
	public TextMeshProUGUI cargoCapText;
	public TextMeshProUGUI fuelCapText;
	public TextMeshProUGUI fitnessText;
	public TextMeshProUGUI lastWheelSizeText;
	public TextMeshProUGUI lastCargoCapText;
	public TextMeshProUGUI lastFuelCapText;
	public TextMeshProUGUI lastFitnessText;
	public CSVWrite csvWriter;
	public ClockScript clock;
	public TargetInstantiate target;

	TruckPathfinding pathfinder;
	TruckPathfinding dumped;
	GameObject[] trucks;
	GameObject[] trucksToDestroy;
	int populationIndex = 0;
	int generationIndex = 0;
	int[] activeTrucks;
	int[] chosenParents;
	int bestFitnessIndex;
	float mutationNum;

	public class OverallFitness {
		public float overallBestFitness;
		public float bestWheelSize;
		public float bestCargoCap;
		public float bestFuelCap;
	}

	public OverallFitness veryBest =  new OverallFitness();

	void Start(){
		ParseFile();
		csvWriter = GetComponent<CSVWrite>();
		resultsGroup.SetActive(false);
		lastResultsGroup.SetActive(false);
		overallBestFitness = 0f;
		generationIndex = 0;
        for(int i = 0; i < population; i++) {
			Vector3 spawnPoint = new Vector3(Random.Range(-450f, 450f), -2.6f, Random.Range(-450f, 450f));
			Collider[] hits = Physics.OverlapSphere(spawnPoint, 3f, LayerMask.GetMask("wallMask"));
			if (hits.Length == 0) {
				GameObject g = Instantiate(truckPrefab, spawnPoint, Quaternion.identity);
				pathfinder = g.GetComponent<TruckPathfinding>();
				pathfinder.truckGene[0] = Random.Range(1f, 100f); //wheel size
				pathfinder.truckGene[1] = Random.Range(1f, 100f); //cargo capacity
				pathfinder.truckGene[2] = Random.Range(1f, 100f); //fuel capacity
			}
			else {
				i--;
			}
			
		}
	}

	void ParseFile() {
		string text = File.ReadAllText(Application.dataPath + "/StreamingAssets/config.txt");

		char[] separators = { ',', ';', '|' };
		string[] strValues = text.Split(separators);

		List<int> intValues = new List<int>();
		foreach (string str in strValues) {
			int val = 0;
			if (int.TryParse(str, out val))
				intValues.Add(val);
		}
		population = intValues[0];
		mutationRate = (float)intValues[1] / 100f;
		target.targetAmount = intValues[2];
		clock.minuteLimit = intValues[3];
	}

	public void CalculateFitness() {
		trucks = null;
		populationProbability = null;
		populationFitness = null;
		cumulativeChance = null;
		survivorGenes = null;
		populationFitnessCounter = 0;
		populationIndex = 0;
		bestFitness = 0f;
		fitnessSum = 0f;
		Debug.Log("Calculating fitness...");
		trucks = GameObject.FindGameObjectsWithTag("Truck");
		activeTrucks = new int[trucks.Length];
		for (int i = 0; i < trucks.Length; i++) {
			if (trucks[i].GetComponent<TruckPathfinding>().dumpedTrash > 0) {
				activeTrucks[i] = 1;
				populationIndex++;
			}
			else {
				activeTrucks[i] = 0;
			}
		}
		populationFitness = new float[populationIndex];
		populationProbability = new float[populationIndex];
		survivorGenes = new float[populationIndex, 3];
		cumulativeChance = new float[populationIndex];
		for (int i = 0; i < trucks.Length; i++) {
			if(activeTrucks[i] == 1) {
				populationFitness[populationFitnessCounter] = trucks[i].GetComponent<TruckPathfinding>().dumpedTrash;
				currentFitness = populationFitness[populationFitnessCounter];
				if(currentFitness > bestFitness) {
					bestFitness = currentFitness;
					bestFitnessIndex = populationFitnessCounter;
				}
				survivorGenes[populationFitnessCounter, 0] = trucks[i].GetComponent<TruckPathfinding>().truckGene[0];
				survivorGenes[populationFitnessCounter, 1] = trucks[i].GetComponent<TruckPathfinding>().truckGene[1];
				survivorGenes[populationFitnessCounter, 2] = trucks[i].GetComponent<TruckPathfinding>().truckGene[2];
				if (bestFitness > overallBestFitness) {
					overallBestFitness = bestFitness;
					veryBest.overallBestFitness = populationFitness[populationFitnessCounter];
					veryBest.bestWheelSize = survivorGenes[populationFitnessCounter, 0];
					veryBest.bestCargoCap = survivorGenes[populationFitnessCounter, 1];
					veryBest.bestFuelCap = survivorGenes[populationFitnessCounter, 2];
				}
				populationFitnessCounter++;
			}
		}
		for (int i = 0; i < populationFitness.Length; i++) {
			fitnessSum += populationFitness[i];
		}
		fitnessSumTracker[generationIndex] = fitnessSum;
		generationIndex++;
		Debug.Log("Fitness sum is " + fitnessSum);
		Debug.Log("Best fitness of last generation is " + bestFitness/fitnessSum);
		for (int i = 0; i < populationFitness.Length; i++) {
			populationProbability[i] = (populationFitness[i] / fitnessSum);
		}
		for (int i = 0; i < populationFitness.Length; i++) {
			if(populationProbability[i] == 0) {
				cumulativeChance[i] = 0;
			}
			else if(i > 0) {
				cumulativeChance[i] = (cumulativeChance[i - 1] + populationProbability[i]);
			}
			else if (i == 0) {
				cumulativeChance[i] = populationProbability[i];
			}
		}
		if(survivorGenes.Length <= 0) {
			Debug.LogWarning("All trucks have failed their purpose");
			//Time.timeScale = 0f;
			GenerateResult();
			return;
		}
		lastResultsGroup.SetActive(true);
		float tempHolder = survivorGenes[bestFitnessIndex, 0];
		lastWheelSizeText.text = "Ukuran roda: " + tempHolder;
		tempHolder = survivorGenes[bestFitnessIndex, 1];
		lastCargoCapText.text = "Kapasitas kargo: " + tempHolder;
		tempHolder = survivorGenes[bestFitnessIndex, 2];
		lastFuelCapText.text = "Kapasitas bensin: " + tempHolder;
		lastFitnessText.text = "Fitness: " + populationFitness[bestFitnessIndex];
		csvWriter.WriteCSV();
		GenerateNextPopulation();
	}

	public void GenerateNextPopulation() {
		//choose parents and generate the next population
		chosenParents = new int[(int)population];
		for (int i = 0; i < population; i++) {
			float randomNum = Random.Range(0f, 1f);
			for (int j = 0; j < populationProbability.Length; j++) {
				if(j > 0) {
					if (cumulativeChance[j - 1] < randomNum && randomNum <= cumulativeChance[j]) {
						chosenParents[i] = j;
					}
				}
				else if (j == 0) {
					if (randomNum <= cumulativeChance[0]) {
						chosenParents[i] = j;
					}
				}
				if(randomNum == 1f) {
					chosenParents[i] = populationProbability.Length;
				}
			}
		}
		//crossover
		for(int k = 0; k < population; k++) {
			int randomGene = Random.Range((int)0, (int)3);
			if (k < population - 1) {
				float holder = survivorGenes[chosenParents[k], randomGene];
				survivorGenes[chosenParents[k], randomGene] = survivorGenes[chosenParents[k + 1], randomGene];
				survivorGenes[chosenParents[k + 1], randomGene] = holder;
			}
			else {
				float holder = survivorGenes[chosenParents[k], randomGene];
				survivorGenes[chosenParents[k], randomGene] = survivorGenes[chosenParents[0], randomGene];
				survivorGenes[chosenParents[0], randomGene] = holder;
			}
		}
		trucksToDestroy = null;
		trucksToDestroy = trucks;
		foreach(GameObject t in trucksToDestroy) {
			Destroy(t);
		}
		for (int i = 0; i < population; i++) {
			Vector3 spawnPoint = new Vector3(Random.Range(-450f, 450f), -2.73f, Random.Range(-450f, 450f));
			Collider[] hits = Physics.OverlapSphere(spawnPoint, 3f, LayerMask.GetMask("wallMask"));
			if(i > chosenParents.Length) {
				Debug.LogWarning("Index has exceeded chosenParents length");
			}
			if (chosenParents[i] > survivorGenes.Length) {
				Debug.LogWarning("chosenParents[i] has exceeded survivorGenes length");
			}
			if (hits.Length == 0) {
				GameObject g = Instantiate(truckPrefab, spawnPoint, Quaternion.identity);
				pathfinder = g.GetComponent<TruckPathfinding>();
				//Debug.Log("Index is " + i + " and chosenParents is " + chosenParents[i]);
				pathfinder.truckGene[0] = survivorGenes[chosenParents[i], 0];
				pathfinder.truckGene[1] = survivorGenes[chosenParents[i], 1];
				pathfinder.truckGene[2] = survivorGenes[chosenParents[i], 2];
				mutationNum = Random.Range(0f, 1f);
				if (mutationNum <= mutationRate) {
					pathfinder.truckGene[Random.Range(0, 3)] = Random.Range(1f, 100f);
				}
			}
			else {
				i--;
			}
		}
	}

	public void GenerateResult() {
		resultsGroup.SetActive(true);
		lastResultsGroup.SetActive(false);
		wheelSizeText.text = "Ukuran roda: " + veryBest.bestWheelSize;
		cargoCapText.text = "Kapasitas kargo: " + veryBest.bestCargoCap;
		fuelCapText.text = "Kapasitas bensin: " + veryBest.bestFuelCap;
		fitnessText.text = "Fitness: " + veryBest.overallBestFitness;
		Time.timeScale = 0f;
	}
}
