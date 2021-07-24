using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVWrite : MonoBehaviour{
	public string filename = "";
	public string day;
	public GenerationManager generationManager;
	public ClockScript clock;
	float[] wheelSizes;
	float[] cargoCaps;
	float[] fuelCaps;
	
	void Start(){
		generationManager = GetComponent<GenerationManager>();
		day = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
		filename = Application.dataPath + "/fitnessData_" + day + ".csv";
    }
	
	public void WriteCSV() {
		if(generationManager.populationFitness.Length > 0) {
			TextWriter tw = new StreamWriter(filename, true);
			tw.WriteLine("Generasi " + clock.generationCount);
			tw.WriteLine("Fitness;Ukuran ban;Kapasitas kargo;Kapasitas bensin;Probabilitas;Probabilitas Kumulatif");
			for (int i = 0; i < generationManager.populationFitness.Length; i++) {
				tw.WriteLine(generationManager.populationFitness[i].ToString("0.000") + ";" + generationManager.survivorGenes[i,0].ToString("0.000") + ";" + generationManager.survivorGenes[i, 1].ToString("0.000") + ";" + generationManager.survivorGenes[i, 2].ToString("0.000") + ";" + generationManager.populationProbability[i].ToString("0.000") + ";" + generationManager.cumulativeChance[i].ToString("0.000"));
			}
			tw.WriteLine("Nilai fitness terbaik = " + generationManager.bestFitness);
			tw.WriteLine("Total fitness =  " + generationManager.fitnessSum);
			tw.WriteLine("");
			tw.Close();
		}
	}
}
