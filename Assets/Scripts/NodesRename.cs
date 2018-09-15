using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodesRename : MonoBehaviour {

	private GameObject[] lNodes;

	// Renombra los nodos del escenario para individualizarlos.
	void Start () {
	
		lNodes = GameObject.FindGameObjectsWithTag ("Node");

		for (int i = 0; i < lNodes.Length; i++) {
			lNodes[i].name = i.ToString();
		}
	}
	
}
