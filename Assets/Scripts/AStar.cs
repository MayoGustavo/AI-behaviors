using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour {


	/* Declaracion de variables. ------------------------------------------------------------ */
	/* -------------------------------------------------------------------------------------- */
	public GameObject Crumb;
	public float fRadiusNeighborDetect;
	public float fRadiusNodeDetect;

	public float fArrivalDistance;
	public float fSpeed;
	public float fRotationSpeed;

	public LayerMask RayMask;

	private GameObject Nodo1;
	private GameObject Nodo2;
	private List<GameObject> P = new List<GameObject>();
	private GameObject[] lCrumbs;

	private bool isRunning;
	private int currentWaypoint;
	private Vector3 vDirection;

	private const int MOUSE_LEFT_BUTTON = 1;
	private const int LAYER_NODE = 8;
	private const string CRUMB = "Crumb";

	//------------------------------------------------------------------


	// Use this for initialization
	void Start () {
		isRunning = false;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetMouseButtonDown(MOUSE_LEFT_BUTTON)) {

			Nodo1 = GetStartNode(transform);
			Nodo2 = GetFinalNode();

			if (Nodo1 != null && Nodo2 != null) {
				A(Nodo1, Nodo2);
				isRunning = true;
				currentWaypoint = 0;
			}

			lCrumbs = GameObject.FindGameObjectsWithTag (CRUMB);
			for (int i = 0; i < lCrumbs.Length; i++) {
				GameObject.Destroy (lCrumbs[i].gameObject);
			}

			foreach (var item in P) {
				MonoBehaviour.Instantiate (Crumb, item.transform.position, item.transform.rotation);
			}

		}

		// Persigue waypoints hasta llegar al ultimo.
		if (isRunning) {

			vDirection = P[currentWaypoint].transform.position - transform.position;
			vDirection.Normalize();
			transform.forward = Vector3.Slerp(transform.forward, vDirection, fRotationSpeed * Time.deltaTime);
			transform.position += transform.forward * fSpeed * Time.deltaTime;

			if(Vector3.Distance(transform.position, P[currentWaypoint].transform.position) < fArrivalDistance){
				currentWaypoint++;
				
				if(currentWaypoint >= P.Count){
					isRunning = false;
				}
			}
		}
	}



	/* Busca nodo mas cercano a personaje. -------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	GameObject GetStartNode (Transform pTransform)
	{
		List<GameObject> Nodes = new List<GameObject> ();
		Collider[] hitColliders = Physics.OverlapSphere(pTransform.position, fRadiusNodeDetect);
		foreach (var item in hitColliders) {
			
			if (item.gameObject.layer == LAYER_NODE) {
				
				Nodes.Add(item.gameObject);
			}
		}

		float fMinimalDistance;
		GameObject Node = null;

		fMinimalDistance = Mathf.Infinity;
		for (int i = 0; i < Nodes.Count; i++) {
			float fDistance = Vector3.Distance(Nodes[i].transform.position , pTransform.position);
			if (fDistance < fMinimalDistance)	{
				fMinimalDistance = fDistance;
				Node = Nodes[i];
			}
		}

		return Node;
	}


	/* Busca nodo mas cercano a clic mouse. ------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	GameObject GetFinalNode ()
	{

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, RayMask)){

			List<GameObject> Nodes = new List<GameObject> ();
			Collider[] hitColliders = Physics.OverlapSphere(hit.point, fRadiusNodeDetect);
			foreach (var item in hitColliders) {
				
				if (item.gameObject.layer == 8) {
					
					Nodes.Add(item.gameObject);
				}
			}
			
			float fMinimalDistance;
			GameObject Node = null;
			
			fMinimalDistance = Mathf.Infinity;
			for (int i = 0; i < Nodes.Count; i++) {
				float fDistance = Vector3.Distance(Nodes[i].transform.position , hit.point);
				if (fDistance < fMinimalDistance)	{
					fMinimalDistance = fDistance;
					Node = Nodes[i];
				}
			}
			
			return Node;
		}

		return null;
	}

	/* Calculo camino con A*. --------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private bool A (GameObject NodoInicial, GameObject NodoFinal){

		List<GameObject> ClosedSet = new List<GameObject>(); // Nodos ya evaluados.
		List<GameObject> OpenSet = new List<GameObject>(); 	 // Nodos a ser evaluados para posible camino.
		Dictionary<string, GameObject> CameFrom = new Dictionary<string, GameObject>();	// Nodos padres que forman el camino.

		Dictionary<string, int> G_Score = new Dictionary<string, int>();
		Dictionary<string, int> H_Score = new Dictionary<string, int>();
		Dictionary<string, int> F_Score = new Dictionary<string, int>();

		GameObject X;
		List<GameObject> NeighborNodesX = new List<GameObject>();
		int TentativeGScore;
		bool TentativeIsBetter;

		P.Clear ();

		OpenSet.Add (NodoInicial);

		G_Score.Add (NodoInicial.name, 0);
		H_Score.Add (NodoInicial.name, HeuristicCostEstimate(NodoInicial, NodoFinal));
		F_Score.Add (NodoInicial.name, G_Score [NodoInicial.name] + H_Score [NodoInicial.name]);


		while (OpenSet.Count >= 0) {
		
			X = NodoMenorValorF(OpenSet, F_Score);

			if (X == NodoFinal) {
				ReconstructPath (CameFrom, CameFrom[NodoFinal.name], NodoInicial);
				P.Add (CameFrom[NodoFinal.name]);
				P.Add (NodoFinal);
				return true;
			}

			OpenSet.Remove(X);
			ClosedSet.Add(X);

			NeighborNodesX =  BuscarVecinosX(X);

			foreach (var Y in NeighborNodesX) {

				if (BuscarClosedSet(Y, ClosedSet))	continue;

				TentativeGScore = G_Score[X.name] + DistanciaXY(X, Y);

				if (!BuscarOpenSet(Y, OpenSet)) {
					OpenSet.Add(Y);
					TentativeIsBetter = true;
				}
				else if (TentativeGScore < G_Score[Y.name]) {
					TentativeIsBetter = true;			}
				else { 
					TentativeIsBetter = false;			}


				if (TentativeIsBetter) {
					if (!BuscarIndiceCameFrom (CameFrom, Y.name)) {
						CameFrom.Add(Y.name,X);
						G_Score.Add(Y.name, TentativeGScore);
						H_Score.Add(Y.name, HeuristicCostEstimate(Y, NodoFinal));
						F_Score.Add(Y.name, G_Score[Y.name] + H_Score[Y.name]);
					}
					else {
						CameFrom[Y.name] = X;
						G_Score[Y.name] = TentativeGScore;
						H_Score[Y.name] = HeuristicCostEstimate(Y, NodoFinal);
						F_Score[Y.name] = G_Score[Y.name] + H_Score[Y.name];
					}
				}
			}
		}


		return false;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private GameObject ReconstructPath (Dictionary<string,GameObject> CameFrom, GameObject CurrentNode, GameObject StartNode) {

		if (BuscarCameFrom(CameFrom, CurrentNode)) {

			if (CurrentNode.name != StartNode.name) 
				P.Add (ReconstructPath (CameFrom, CameFrom[CurrentNode.name], StartNode));
			return CurrentNode;
		} else {
			return CurrentNode;
		}
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private bool BuscarCameFrom (Dictionary<string,GameObject> CameFrom, GameObject CurrentNode) {

		foreach (KeyValuePair<string, GameObject> Node in CameFrom) {

			if (Node.Value.name == CurrentNode.name)
				return true;
		}
		return false;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private bool BuscarIndiceCameFrom (Dictionary<string,GameObject> CameFrom, string Indice) {
		
		foreach (KeyValuePair<string, GameObject> Node in CameFrom) {
			
			if (Node.Key == Indice)
				return true;
		}
		return false;
	}

	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private GameObject NodoMenorValorF (List<GameObject> OpenSet, Dictionary<string,int> FScore) {

		GameObject NodoMenor;
		string IndiceMenor;
		int FMenor;

		NodoMenor = OpenSet [0];
		IndiceMenor = NodoMenor.name;
		FMenor = FScore [IndiceMenor];

		foreach (var Nodo in OpenSet) {

			if (FScore [Nodo.name] < FMenor) {

				NodoMenor = Nodo;
				IndiceMenor = Nodo.name;
				FMenor = FScore [Nodo.name];
			}
		}

		return NodoMenor;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private int HeuristicCostEstimate (GameObject NodoA, GameObject NodoB) {

		return Mathf.FloorToInt (Vector3.Distance (NodoA.transform.position, NodoB.transform.position));
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private List<GameObject> BuscarVecinosX (GameObject X) {

		List<GameObject> Neighbors = new List<GameObject> ();

		Collider[] hitColliders = Physics.OverlapSphere(X.transform.position, fRadiusNeighborDetect);
		foreach (var item in hitColliders) {

			if (item.gameObject.layer == LAYER_NODE) {

				Neighbors.Add(item.gameObject);
			}
		}
		return Neighbors;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private bool BuscarOpenSet (GameObject Y, List<GameObject> lOpenSet) {

		foreach (var Nodo in lOpenSet) {

			if (Y.name == Nodo.name)
				return true;
		}

		return false;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private bool BuscarClosedSet (GameObject Y, List<GameObject> lClosedSet) {

		foreach (var Nodo in lClosedSet) {

			if (Y.name == Nodo.name)
				return true;
		}

		return false;
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private int DistanciaXY (GameObject X, GameObject Y) {

		return Mathf.FloorToInt (Vector3.Distance (X.transform.position, Y.transform.position));
	}



	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		//Gizmos.DrawSphere(transform.position, fRadiusNeighborDetect);
	}
}
