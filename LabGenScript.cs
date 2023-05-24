using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Flags]
public enum Celda
{
	NADA = 0, LEFT = 1, TOP = 2, RIGHT = 4, BOTTOM = 8, VISITED = 16
}

public enum GameState
{
	PS_RESET, PS_GENERAR, PS_RESOLVER
}

public struct CELDPOS
{
	public int x;		public int z;		public Celda Pared;
	public int gCost, hCost;
}


public class LabGenScript : MonoBehaviour
{
    List<GameObject> m_Walls 	= new List<GameObject>();
    List<GameObject> m_Ruta 	= new List<GameObject>();
    List<CELDPOS> m_Tortuga 	= new List<CELDPOS>();
    List<CELDPOS> m_OpenSet		= new List<CELDPOS>();
    List<CELDPOS> m_ClosedSet 	= new List<CELDPOS>();

    public GameObject WallFab;
	GameObject TugaObj = null;
    public float Lado = 6.0f;
    public static Celda[,] Maze;
    public int m_x = 4;
    public int m_z = 4;

	GameState State;

	CELDPOS Tortuga;
	CELDPOS m_Start, m_End;

    // Start is called before the first frame update
    void Start()
    {
    	Maze = new Celda[12, 12];
 //   	Maze = Grilla.Create(12, 12);
    	    
		Tortuga = new CELDPOS();

    	var rnd = new System.Random();
    	Tortuga.x = rnd.Next(0, 11);		Tortuga.z = rnd.Next(0, 11);		



		m_Start = new CELDPOS();
    	var st = new System.Random();
    	m_Start.x = st.Next(0, 11);		m_Start.z = st.Next(0, 11);		

		m_End = new CELDPOS();
    	var nd = new System.Random();
    	m_End.x = st.Next(0, 11);			m_End.z = st.Next(0, 11);		



		Debug.Log("StartX = " + m_Start.x + " 	StartZ = " + m_Start.z + " EndX = " + m_End.x + "		EndZ = " + m_End.z);

//    	Tortuga.x = 0;		Tortuga.z = 0;		

        for (int x = 0; x < 12; x++)
            for (int z = 0; z < 12; z++)
                Maze[x, z] = Celda.LEFT | Celda.TOP | Celda.RIGHT | Celda.BOTTOM;


		State = 0;



		Vector3 Pos = new Vector3((m_Start.x - m_x / 2) * Lado, 1.5f, (m_Start.z - m_z / 2) * Lado);
        GameObject m_StartObj = Instantiate(WallFab, Pos, Quaternion.identity);
		m_StartObj.transform.localScale = new Vector3(1.0f, 8.0f, 1.0f);

        MeshRenderer Mesh = m_StartObj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.blue;



		Vector3 EndPos = new Vector3((m_End.x - m_x / 2) * Lado, 1.5f, (m_End.z - m_z / 2) * Lado);
        GameObject m_EndObj = Instantiate(WallFab, EndPos, Quaternion.identity);
		m_EndObj.transform.localScale = new Vector3(1.0f, 8.0f, 1.0f);

        MeshRenderer EndMesh = m_EndObj.GetComponent<MeshRenderer>();
        EndMesh.material.color = Color.red;

//    	DrawMaze(Maze);
    }

    // Update is called once per frame
    void Update()
    {
		switch (State)
		{
			case GameState.PS_RESET:
			break;
			case GameState.PS_GENERAR:
				Generate();
			break;
			case GameState.PS_RESOLVER:
				ResolverMaze();
			break;
		}
/*
		if (GenCompleto())
		{
			m_Tortuga.Clear();
			Debug.Log("Gen Completo....Tortugas = " + m_Tortuga.Count);
			return;
		}
*/

//		Generate();
		DrawMaze(Maze);
		DrawTortuga();


/*	       
		Vector3 Pos = new Vector3((m_Start.x - m_x / 2) * Lado, 1.5f, (m_Start.z - m_z / 2) * Lado);
        TugaObj = Instantiate(WallFab, Pos, Quaternion.identity);
		TugaObj.transform.localScale = new Vector3(1.0f, 8.0f, 1.0f);

        MeshRenderer Mesh = TugaObj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.green;
*/

		if (Input.GetKeyDown(KeyCode.R))
		{	
			ClearMaze();
			ClearRuta();
			ResetMaze();
			State = GameState.PS_RESET;
		}

		if (Input.GetKeyDown(KeyCode.G))
		{	
			State = GameState.PS_GENERAR;
		}

		if (Input.GetKeyDown(KeyCode.V))
		{	
			Tortuga = m_Start;

        	for (int x = 0; x < 12; x++)
            	for (int z = 0; z < 12; z++)
                	Maze[x, z] &= ~Celda.VISITED;



                	
//			Maze[Tortuga.x, Tortuga.z] |= Celda.VISITED;

			State = GameState.PS_RESOLVER;
		}
    }


	Celda Cambio(Celda cl)
    {
    	switch (cl)
    	{
    		case Celda.LEFT:
    		return Celda.RIGHT;
    		case Celda.TOP:
    		return Celda.BOTTOM;
    		case Celda.RIGHT:
    		return Celda.LEFT;
    		case Celda.BOTTOM:

    		return Celda.TOP;
    	}
    	
    	return 0;
    }

    public bool GenCompleto()
    {
        bool bVisited = true; 		int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (bVisited)
                    bVisited = Maze[x, z].HasFlag(Celda.VISITED);

        return bVisited;
    }


	public void Generate()
	{
		if (GenCompleto())
		{
			m_Tortuga.Clear();
			Debug.Log("Gen Completo....Tortugas = " + m_Tortuga.Count);
			return;
		}

		Maze[Tortuga.x, Tortuga.z] |= Celda.VISITED;
   		m_Tortuga.Add(Tortuga);
 
		CELDPOS Vec = new CELDPOS();		Vec = GetVecinos(Tortuga);


		if (!Maze[Vec.x, Vec.z].HasFlag(Celda.VISITED))
		{
			Maze[Vec.x, Vec.z] &= ~Vec.Pared;
			Maze[Tortuga.x, Tortuga.z] &= ~Cambio(Vec.Pared);			//Celda.RIGHT;

			Tortuga = Vec;		//GetVecinos(Tortuga);
//   			m_Tortuga.Add(Tortuga);
		}
		else if (m_Tortuga.Count > 0)
		{
   	     	Tortuga = m_Tortuga[0];
   	     	m_Tortuga.RemoveAt(0);
		}

		int jots = Mathf.Abs(-67);

	//	Maze[Tortuga.x, Tortuga.z] &= ~Celda.RIGHT;
	}

    public CELDPOS GetVecinos(CELDPOS Tort)
    {
    	CELDPOS Vec = new CELDPOS();
    	List<CELDPOS> m_Vecinos = new List<CELDPOS>();
    	
    	if (Tort.x - 1 >= 0)
    	{	
//			if (!Maze[Tort.x - 1, Tort.z].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x - 1;		Vec.z = Tort.z;				Vec.Pared = Celda.RIGHT;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.z - 1 >= 0)
    	{	
//			if (!Maze[Tort.x, Tort.z - 1].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x;			Vec.z = Tort.z - 1;			Vec.Pared = Celda.BOTTOM;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.x + 1 < 12)
    	{	
//			if (!Maze[Tort.x + 1, Tort.z].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x + 1;		Vec.z = Tort.z;				Vec.Pared = Celda.LEFT;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.z + 1 < 12)
    	{	
// 			if (!Maze[Tort.x, Tort.z + 1].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x;			Vec.z = Tort.z + 1;			Vec.Pared = Celda.TOP;
    	        m_Vecinos.Add(Vec);
//			}
    	}

		
    	var rnd = new System.Random();
    	int nIndex = rnd.Next(0, m_Vecinos.Count);		
	

/*
		int b = m_Vecinos.Count;

		for (int i = 0; i < b; i++)
		{
			CELDPOS c = m_Vecinos[i];

        Vector3 Pos = new Vector3(c.x * Lado, 1.5f, c.z * Lado);
        TugaObj = Instantiate(WallFab, Pos, Quaternion.identity);

		}
*/





		Vec = m_Vecinos[nIndex];

//		Debug.Log("Vecinos = " + m_Vecinos.Count + " 	TortugaX " + Tort.x + " TotugaZ " + Tort.z + "		Tortugas " + m_Tortuga.Count + " Direction " + Vec.Pared);
//		Debug.Log(" Index " + nIndex + "  Vecinos " + m_Vecinos.Count);
  
    	return Vec;		// m_Vecinos[nIndex];
    }


    public CELDPOS SetVecinos(CELDPOS Tort)
    {
    	CELDPOS Vec = new CELDPOS();			CELDPOS Via = new CELDPOS();

		int gCost, hCost;			int Cost = 1000;

    	List<CELDPOS> m_Vecinos = new List<CELDPOS>();
    	
    	if (Tort.x - 1 >= 0)
    	{	
			if (!Maze[Tort.x - 1, Tort.z].HasFlag(Celda.VISITED) && !Maze[Tort.x, Tort.z].HasFlag(Celda.LEFT))
			{
    			Vec.x = Tort.x - 1;		Vec.z = Tort.z;				
    	        m_Vecinos.Add(Vec);
			}
    	}
    	
    	if (Tort.z - 1 >= 0)
    	{	
			if (!Maze[Tort.x, Tort.z - 1].HasFlag(Celda.VISITED) && !Maze[Tort.x, Tort.z].HasFlag(Celda.TOP))
			{
    			Vec.x = Tort.x;			Vec.z = Tort.z - 1;			
    	        m_Vecinos.Add(Vec);
			}
    	}
    	
    	if (Tort.x + 1 < 12)
    	{	
			if (!Maze[Tort.x + 1, Tort.z].HasFlag(Celda.VISITED) && !Maze[Tort.x, Tort.z].HasFlag(Celda.RIGHT))
			{
    			Vec.x = Tort.x + 1;		Vec.z = Tort.z;				
    	        m_Vecinos.Add(Vec);
			}
    	}
    	
    	if (Tort.z + 1 < 12)
    	{	
 			if (!Maze[Tort.x, Tort.z + 1].HasFlag(Celda.VISITED) && !Maze[Tort.x, Tort.z].HasFlag(Celda.BOTTOM))
			{
    			Vec.x = Tort.x;			Vec.z = Tort.z + 1;			
    	        m_Vecinos.Add(Vec);
			}
    	}

		
    	var rnd = new System.Random();
    	int nIndex = rnd.Next(0, m_Vecinos.Count);		
	
		int b = m_Vecinos.Count;

		for (int i = 0; i < b; i++)
		{
			CELDPOS c = m_Vecinos[i];

			gCost = c.x - m_Start.x + c.z - m_Start.z;
			hCost = c.x - m_End.x + c.z - m_End.z;

			if (gCost + hCost < Cost)
			{
				Via = c;
				Cost = gCost + hCost;
			}
		}



		Debug.Log("Vecinos " + m_Vecinos.Count + " Viax " + Via.x + " Viaz " + Via.z);



    	return Via;		// m_Vecinos[nIndex];
    }

    public void ResetMaze()
    {
	    for (int x = 0; x < 12; x++)
            for (int z = 0; z < 12; z++)
                Maze[x, z] = Celda.LEFT | Celda.TOP | Celda.RIGHT | Celda.BOTTOM;


    	var rnd = new System.Random();
    	Tortuga.x = rnd.Next(0, 11);		Tortuga.z = rnd.Next(0, 11);		

    	var st = new System.Random();
    	m_Start.x = st.Next(0, 11);			m_Start.z = st.Next(0, 11);		

    	var nd = new System.Random();
    	m_End.x = st.Next(0, 11);			m_End.z = st.Next(0, 11);		
	}

    public void ClearMaze()
    {
		int nCant = m_Walls.Count;

        for (int z = 0; z < nCant; z++)
        {
       		GameObject go = m_Walls[z];
       		Destroy(go);
        }

		m_Walls.Clear();
	}

    public void ClearRuta()
    {
		int nCant = m_Ruta.Count;

        for (int z = 0; z < nCant; z++)
        {
       		GameObject go = m_Ruta[z];
       		Destroy(go);
        }

		m_Ruta.Clear();
	}


    public void DrawMaze(Celda[,] maze)
    {
        int x, z; 	Vector3 Pos;
        float yScala = 6.0f;

//	Vector3 v = Vector3.left;
/*    	x = m_Walls.Count;
            
        for (z = 0; z < x; z++)
        {
       		GameObject go = m_Walls[z];
       		Destroy(go);
        }
*/
		ClearMaze();

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (maze[x, z].HasFlag(Celda.LEFT))
                {
                    Pos = new Vector3((x - m_x / 2 - 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
                    GameObject Left = Instantiate(WallFab, Pos, Quaternion.identity);
                    Left.transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);
					m_Walls.Add(Left);
                }


                if (maze[x, z].HasFlag(Celda.TOP))
                {
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
		            GameObject Top = Instantiate(WallFab, Pos, Quaternion.identity);
		            Top.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);
					m_Walls.Add(Top);
                }

                if (maze[x, z].HasFlag(Celda.RIGHT))
                {
                    Pos = new Vector3((x - m_x / 2 + 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
		            GameObject Right = Instantiate(WallFab, Pos, Quaternion.identity);
		            Right.transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);
 					m_Walls.Add(Right);
               }

                if (maze[x, z].HasFlag(Celda.BOTTOM))
                {
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
		            GameObject Bottom = Instantiate(WallFab, Pos, Quaternion.identity);
		            Bottom.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);
 					m_Walls.Add(Bottom);
               }
            }
    }

    public void DrawTortuga()
	{
		if (TugaObj)
			Destroy(TugaObj);

  		var rnd = new System.Random();
//    	Tortuga.x = rnd.Next(0, 12);		Tortuga.z = rnd.Next(0, 12);

        Vector3 Pos = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        TugaObj = Instantiate(WallFab, Pos, Quaternion.identity);
		TugaObj.transform.localScale = new Vector3(1.0f, 8.0f, 1.0f);

        MeshRenderer Mesh = TugaObj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.black;
	}

	void ResolverMaze()
	{
		Maze[Tortuga.x, Tortuga.z] |= Celda.VISITED;

		if (Tortuga.x == m_End.x && Tortuga.z == m_End.z)
		{
			Debug.Log("FANTASTICO");

			return;
		}


/*
        Vector3 Pos = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        GameObject Tuga = Instantiate(WallFab, Pos, Quaternion.identity);
		Tuga.transform.localScale = new Vector3(1.0f, 8.0f, 1.0f);

        MeshRenderer Mesh = Tuga.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.yellow;
*/


        Vector3 Pos = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        GameObject Tuga = Instantiate(WallFab, Pos, Quaternion.identity);
		m_Ruta.Add(Tuga);

		Tortuga = SetVecinos(Tortuga);

        Vector3 Positio = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        Tuga = Instantiate(WallFab, Positio, Quaternion.identity);
		m_Ruta.Add(Tuga);
	}
}
