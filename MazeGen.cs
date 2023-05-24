
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum Celula
{
	LEFT = 1, TOP = 2, RIGHT = 4, BOTTOM = 8, VISITED = 16, NADA = 32
}
[Flags]
public enum MazeState
{
	PS_RESET, PS_GENERAR, PS_RESOLVER
}

public struct CELDAINFO
{
    public List<CELDAINFO> m_Vecinos;		public bool [] bWall;//public CELDAINFO m_Parent; 
	public int x;		public int z;		public Celula Pared;
	public int gCost, hCost;				public Celula Direction;
}


public class MazeGen : MonoBehaviour
{
    List<GameObject> m_Walls 	= new List<GameObject>();
    List<GameObject> m_Ruta 	= new List<GameObject>();
    List<CELDAINFO> m_Tortuga 	= new List<CELDAINFO>();
    List<CELDAINFO> m_OpenSet	= new List<CELDAINFO>();
//    List<CELDPOS> m_ClosedSet 	= new List<CELDPOS>();

    public GameObject WallFab;
	GameObject TugaObj = null;
    public float Lado = 6.0f;
    public static CELDAINFO[,] Maze;
    public int m_x = 8;
    public int m_z = 8;



	
//	int i = 0;				int j = 0;

	MazeState State;

	CELDAINFO Tortuga, Meta, End;
    // Start is called before the first frame update
    void Start()
    {
        State = MazeState.PS_RESET;

        CreateMaze();           ResetMaze();
    }

    // Update is called once per frame
    void Update()
    {
		switch (State)
		{
			case MazeState.PS_RESET:
			break;
			case MazeState.PS_GENERAR:
				Degenerado();
//				GenerateMaze();
//				Tortuga = GenLab(Tortuga);
			break;
			case MazeState.PS_RESOLVER:
//				ResolverMaze();
			break;
		}

		if (Input.GetKeyDown(KeyCode.R))
		{	
			ClearMaze();
			ClearRuta();
			ResetMaze();

			State = MazeState.PS_RESET;
		}

		if (Input.GetKeyDown(KeyCode.G))
		{	
			State = MazeState.PS_GENERAR;
		}

		if (Input.GetKeyDown(KeyCode.V))
		{	
//			Tortuga = Meta;

            ClearMaze();
        	for (int x = 0; x < m_x; x++)
            	for (int z = 0; z < m_z; z++)
                	Maze[x, z].Pared &= ~Celula.VISITED;




			Maze[Meta.x, Meta.z].gCost = 0;
			Maze[Meta.x, Meta.z].hCost = Mathf.Abs(Meta.x - End.x) + Mathf.Abs(Meta.z - End.z);	
			
			Maze[Meta.x, Meta.z].Pared |= Celula.VISITED;
            m_OpenSet.Add(Meta);

//                Meta.gCost = 0;		//Mathf.Abs(Meta.x - x) + Mathf.Abs(Meta.z - z);
//                Meta.hCost = 		Maze[Tortuga.x, Tortuga.z].Pared |= Celula.VISITED;


            Tortuga = SetTortuga();

//            m_OpenSet.Add(Tortuga);
			SetVecindad(Tortuga);
                	
//			Maze[Tortuga.x, Tortuga.z] |= Celda.VISITED;

			State = MazeState.PS_RESOLVER;
		}

        if (Input.GetKeyDown(KeyCode.UpArrow))
		{
//	    	var st = new System.Random();
//            ResolverMaze();					//DrawVecinos(Tortuga);
				

			Degenerado();			DrawMaze();	
//			Tortuga = GenLab(Tortuga);



//			j++;			j = j % m_z;	

//			Tortuga.z = j;		Tortuga.x = i;

//			Debug.Log("I " + i + " J " + j + " Vecinos " + Maze[i, j].m_Vecinos.Count);
		}

        if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			Tortuga.z --;			if (Tortuga.z < 0) Tortuga.z = m_z - 1;		

//			Debug.Log("Vecinos " + Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count);
			SetVecindad(Tortuga);
		}


        if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			Tortuga.x --;			if (Tortuga.x < 0) Tortuga.x = m_x - 1;		

			Debug.Log("Vecinos " + Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count);
			SetVecindad(Tortuga);
		}


        if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			Tortuga.x++;			Tortuga.x = Tortuga.x % m_x;		

			Debug.Log("Vecinos " + Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count);
			SetVecindad(Tortuga);
		}

        DrawMaze();         DrawTortuga();			//DrawVecinos(Tortuga);
    }

    void CreateMaze()
    {
    	Maze    = new CELDAINFO[m_x, m_z];
		Tortuga = new CELDAINFO();
        Meta    = new CELDAINFO();
        End     = new CELDAINFO();
    }

    void ResetMaze()
    {
    	var rnd = new System.Random();
//    	Tortuga.x = rnd.Next(0, m_x);		Tortuga.z = rnd.Next(0, m_z);		

    	Meta.x = rnd.Next(1, m_x - 1);		    Meta.z = rnd.Next(1, m_z - 1);		
		Tortuga = Meta;
    	End.x = rnd.Next(1, m_x - 1);			End.z = rnd.Next(1, m_z - 1);		

		Vector3 Pos;
		
		Pos = new Vector3((Meta.x - m_x / 2) * Lado, 1.5f, (Meta.z - m_z / 2) * Lado);
        GameObject m_Obj = Instantiate(WallFab, Pos, Quaternion.identity);
		m_Obj.transform.localScale = new Vector3(2.0f, 8.0f, 2.0f);

        MeshRenderer Mesh = m_Obj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.red;
        m_Ruta.Add(m_Obj);

		Pos = new Vector3((End.x - m_x / 2) * Lado, 1.5f, (End.z - m_z / 2) * Lado);
        m_Obj = Instantiate(WallFab, Pos, Quaternion.identity);
		m_Obj.transform.localScale = new Vector3(2.0f, 8.0f, 2.0f);

        Mesh = m_Obj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.cyan;
        m_Ruta.Add(m_Obj);


        m_OpenSet.Clear();
 
        for (int x = 0; x < m_x; x++)
            for (int z = 0; z < m_z; z++)
            {
				Maze[x, z] = new CELDAINFO();

				Maze[x, z].m_Vecinos = new List<CELDAINFO>();
//				Maze[x, z].m_Parent = new CELDAINFO();

                Maze[x, z].Pared = Celula.LEFT | Celula.TOP | Celula.RIGHT | Celula.BOTTOM;
                Maze[x, z].Direction = Celula.LEFT | Celula.TOP | Celula.RIGHT | Celula.BOTTOM;
//				Maze[x, z].Direction = 0;		//Celula.NADA;

                Maze[x, z].gCost = 500;
//                Maze[x, z].gCost = Mathf.Abs(Meta.x - x) + Mathf.Abs(Meta.z - z);
                Maze[x, z].hCost = Mathf.Abs(End.x - x) + Mathf.Abs(End.z - z);

				Maze[x, z].m_Vecinos.Clear();			int[,] Vec = new int[4, 2];
				CELDAINFO m_Vec = new CELDAINFO();

				Vec[0, 0] = x - 1;					Vec[0, 1] = z;
				Vec[1, 0] = x;						Vec[1, 1] = z + 1;
				Vec[2, 0] = x + 1;					Vec[2, 1] = z;
				Vec[3, 0] = x;						Vec[3, 1] = z - 1;

				Maze[x, z].bWall = new bool[4];

				for (int i = 0; i < 4; i++)
				{
					Maze[x, z].bWall[i] = true;

					if (Vec[i, 0] < m_x - 1 && Vec[i, 0] >= 1 && Vec[i, 1] < m_z - 1 && Vec[i, 1] >= 1)
					{

						m_Vec.x = Vec[i, 0];				m_Vec.z = Vec[i, 1];

						Maze[x, z].m_Vecinos.Add(m_Vec);
					}
//					else
//						Debug.Log("Cagada " + m_Vec.x + m_Vec.z);
				}
            }



    }

	Celula Cambio(Celula cl)
    {
    	switch (cl)
    	{
    		case Celula.LEFT:
    		return Celula.RIGHT;
    		case Celula.TOP:
    		return Celula.BOTTOM;
    		case Celula.RIGHT:
    		return Celula.LEFT;
    		case Celula.BOTTOM:
    		return Celula.TOP;
    	}
    	
    	return 0;
    }

    public bool GenCompleto()
    {
        bool bVisited = true; 		int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (bVisited)
                    bVisited = Maze[x, z].Pared.HasFlag(Celula.VISITED);

        return bVisited;
    }

	void Degenerado()
	{
		if (GenCompleto())
		{
			m_Tortuga.Clear();
			Debug.Log("Gen Completo....Tortugas = " + m_Tortuga.Count);
			return;
		}

//		if (Tort.x < 0 || Tort.x >= m_x || Tort.z < 0 || Tort.z >= m_z)
//			return Tort;

		Maze[Tortuga.x, Tortuga.z].Pared |= Celula.VISITED;
   		m_Tortuga.Add(Tortuga);

		CELDAINFO Vec = new CELDAINFO();		//Vec = GetVecinos(Tortuga);

    	var rnd = new System.Random();			int nCant = Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count;


    	int nIndex = rnd.Next(0, nCant);						
		Vec = Maze[Tortuga.x, Tortuga.z].m_Vecinos[nIndex];



		if (!Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED) && Maze[Tortuga.x, Tortuga.z].bWall[nIndex])		
		{
			nIndex = (nIndex + nCant) % 4;
			Maze[Tortuga.x, Tortuga.z].bWall[nIndex] = false;				nIndex = (nIndex + 2) % 4;
			Maze[Vec.x, Vec.z].bWall[nIndex] = false;		
			Tortuga = Vec;		
			Debug.Log("Vecinos " + Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count);
			Debug.Log("INDICE " + nIndex);
		}
		else if (m_Tortuga.Count > 0)
		{
   	     	Tortuga = m_Tortuga[0];
   	     	m_Tortuga.RemoveAt(0);
		}
	}

	CELDAINFO GenLab(CELDAINFO Tort)
	{
		if (GenCompleto())
		{
			m_Tortuga.Clear();
			Debug.Log("Gen Completo....Tortugas = " + m_Tortuga.Count);
			return Tort;
		}

		if (Tort.x < 0 || Tort.x >= m_x || Tort.z < 0 || Tort.z >= m_z)
			return Tort;

		Maze[Tort.x, Tort.z].Pared |= Celula.VISITED;
   		m_Tortuga.Add(Tort);

		CELDAINFO Vec = new CELDAINFO();		//Vec = GetVecinos(Tortuga);

    	var rnd = new System.Random();			int nCant = Maze[Tort.x, Tort.z].m_Vecinos.Count;


    	int nIndex = rnd.Next(0, nCant);						Vec = Maze[Tort.x, Tort.z].m_Vecinos[nIndex];

		Debug.Log("INDICE " + nIndex);


		if (!Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED) && Maze[Tort.x, Tort.z].bWall[nIndex])		
		{
			Maze[Tort.x, Tort.z].bWall[nIndex] = false;				nIndex = (nIndex + 2) % 4;		//nCant;
			Maze[Vec.x, Vec.z].bWall[nIndex] = false;		
			Tort = Vec;		
		}
		else if (m_Tortuga.Count > 0)
		{
   	     	Tort = m_Tortuga[0];
   	     	m_Tortuga.RemoveAt(0);
		}

	 	return Vec;
  	}	


	public void GenerateMaze()
	{
		if (GenCompleto())
		{
			m_Tortuga.Clear();
			Debug.Log("Gen Completo....Tortugas = " + m_Tortuga.Count);
			return;
		}

		Maze[Tortuga.x, Tortuga.z].Pared |= Celula.VISITED;
   		m_Tortuga.Add(Tortuga);
 
		CELDAINFO Vec = new CELDAINFO();		Vec = GetVecinos(Tortuga);

//		while (Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED))
//			Vec = GetVecinos(Tortuga);

    	if (!Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED))
	    {
		    Maze[Vec.x, Vec.z].Pared &= ~Vec.Pared;
		    Maze[Tortuga.x, Tortuga.z].Pared &= ~Cambio(Vec.Pared);			//Celda.RIGHT;


			
//			Maze[Vec.x, Vec.z].Direction |= Vec.Pared;
//			Maze[Tortuga.x, Tortuga.z].Direction |= Cambio(Vec.Pared);

			Maze[Vec.x, Vec.z].Direction &= ~Vec.Pared;
			Maze[Tortuga.x, Tortuga.z].Direction &= ~Cambio(Vec.Pared);

		    Tortuga = Vec;		//GetVecinos(Tortuga);
	    }
		else if (m_Tortuga.Count > 0)
		{
   	     	Tortuga = m_Tortuga[0];
   	     	m_Tortuga.RemoveAt(0);
		}
	}

    public CELDAINFO GetVecinos(CELDAINFO Tort)
    {
    	CELDAINFO Vec = new CELDAINFO();                Vec = Tort;
    	List<CELDAINFO> m_Vecinos = new List<CELDAINFO>();
    				Debug.Log("gCost " + Meta.gCost + " hCost " + Maze[Meta.x, Meta.z].hCost);

    	if (Tort.x - 1 >= 0)
    	{	
//			if (!Maze[Tort.x - 1, Tort.z].HasFlag(Celda.VISITED))
//			{				
    			Vec.x = Tort.x - 1;		Vec.z = Tort.z;				Vec.Pared = Celula.RIGHT;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.z - 1 >= 0)
    	{	
//			if (!Maze[Tort.x, Tort.z - 1].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x;			Vec.z = Tort.z - 1;			Vec.Pared = Celula.TOP;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.x + 1 < m_x)
    	{	
//			if (!Maze[Tort.x + 1, Tort.z].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x + 1;		Vec.z = Tort.z;				Vec.Pared = Celula.LEFT;
    	        m_Vecinos.Add(Vec);
//			}
    	}
    	
    	if (Tort.z + 1 < m_z)
    	{	
// 			if (!Maze[Tort.x, Tort.z + 1].HasFlag(Celda.VISITED))
//			{
    			Vec.x = Tort.x;			Vec.z = Tort.z + 1;			Vec.Pared = Celula.BOTTOM;
    	        m_Vecinos.Add(Vec);
//			}
    	}

		
    	var rnd = new System.Random();
    	int nIndex = rnd.Next(0, m_Vecinos.Count);		
	

        if (m_Vecinos.Count > 0)
		    Vec = m_Vecinos[nIndex];
  
    	return Vec;		// m_Vecinos[nIndex];
    }

    public CELDAINFO SetVecinos(CELDAINFO Tort)
    {
//    	CELDAINFO Vec = new CELDAINFO();			
		CELDAINFO Via = new CELDAINFO();

		int gCost, hCost;			int Cost = 1000;

		int nCant = Maze[Tort.x, Tort.z].m_Vecinos.Count;

		for (int i = 0; i < nCant; i++)
		{
			CELDAINFO c = Maze[Tort.x, Tort.z].m_Vecinos[i];

			gCost = Maze[c.x, c.z].gCost;
			hCost = Maze[c.x, c.z].hCost;

			if ((gCost + hCost) < Cost)
			{
				Via = c;
				Cost = gCost + hCost;
			}
		}

		if (!Maze[Via.x, Via.z].Pared.HasFlag(Celula.VISITED))
    		return Via;		// m_Vecinos[nIndex];
		else
			return Tort;
    }


    public void SetVecindad(CELDAINFO Tort)
    {
//        CELDAINFO Vec = new CELDAINFO();                     
		int dist;
		m_OpenSet.Clear();

		int nCant = Maze[Tort.x, Tort.z].m_Vecinos.Count;
		Celula Dir = Maze[Tort.x, Tort.z].Direction;			//tuga |= Cambio(tuga);
		Celula VecDir;			
		


		for (int v = 0; v < nCant; v++)
		{
        	CELDAINFO Vec = new CELDAINFO();                     
			Vec = Maze[Tort.x, Tort.z].m_Vecinos[v];		//Dir = (Celula)Mathf.Pow(2, v);

           	if (m_OpenSet.Contains(Vec))
           	{
                dist = Maze[Tort.x, Tort.z].gCost + 1;

               	if (dist < Maze[Vec.x, Vec.z].gCost)
				{
                   	Maze[Vec.x, Vec.z].gCost = dist;     
					break;
				}        
           	}

			if (!(Vec.x < 0 || Vec.x >= m_x || Vec.z < 0 || Vec.z >= m_z))	
			{
				Dir = Maze[Tort.x, Tort.z].Direction;
				VecDir = Cambio(Dir);
//				if (Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.NADA)
//	           	&& Maze[Tort.x, Tort.z].Pared.HasFlag(Celula.NADA) 
//				&& !Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED)
//				)
				if (Maze[Vec.x, Vec.z].Direction.HasFlag(VecDir)
//	           	&& Maze[Tort.x, Tort.z].Pared.HasFlag(Celula.NADA) 
				&& !Maze[Vec.x, Vec.z].Pared.HasFlag(Celula.VISITED)
				)
           		{
	               	Maze[Vec.x, Vec.z].gCost = Maze[Tort.x, Tort.z].gCost + 1;
	               	Maze[Vec.x, Vec.z].hCost = Mathf.Abs(Vec.x - End.x) + Mathf.Abs(Vec.z - End.z);

//					Debug.Log("Vec Direccion " + Maze[Vec.x, Vec.z].Direction);
	   	        	m_OpenSet.Add(Vec);
 	          	}
			}
			else
				Debug.Log("Vecinos Caca");
		}


//		Debug.Log("Vecinos " + Maze[Tort.x, Tort.z].Pared);

		Debug.Log("Open Set " + m_OpenSet.Count);
//		Debug.Log("Vecinos " + Maze[Tort.x, Tort.z].m_Vecinos.Count);
    }

    CELDAINFO SetTortuga()
    {
	    CELDAINFO Via = new CELDAINFO();     	Via.x = 2;			Via.z = 2;
		CELDAINFO c = new CELDAINFO();

		int Cost = 1000;
    	int nCant = m_OpenSet.Count;

		for (int i = 0; i < nCant; i++)
		{
		    c = m_OpenSet[i];

//			if (!Maze[c.x, c.z].Pared.HasFlag(Celula.VISITED))
//			{
				if ((Maze[c.x, c.z].gCost + Maze[c.x, c.z].hCost) < Cost)
				{
					Cost = Maze[c.x, c.z].gCost + Maze[c.x, c.z].hCost;
					Via = c;

//				Debug.Log("TORTUGA Costo " + (Maze[Via.x, Via.z].gCost + Maze[Via.x, Via.z].hCost));
//				Debug.Log("Costo es COSTOSO " + Cost + " OpenSet " + nCant);
				}
//			}
		}

//    	var rnd = new System.Random();
//    	Via.x = rnd.Next(0, m_x);			Via.z = rnd.Next(0, m_z);		


//		return Maze[Via.x, Via.z];
        return Via;
    }
//		Tortuga = Maze[Tortuga.x, Tortuga.z].m_Vecinos[0];


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



    public void DrawMaze()
    {
        int x, z; 	Vector3 Pos;
        float yScala = 6.0f;

		ClearMaze();

/*
        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (Maze[x, z].Pared.HasFlag(Celula.LEFT))
                {
                    Pos = new Vector3((x - m_x / 2 - 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
                    GameObject Left = Instantiate(WallFab, Pos, Quaternion.identity);
                    Left.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);
					m_Walls.Add(Left);
                }


                if (Maze[x, z].Pared.HasFlag(Celula.TOP))
                {
//                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
		            GameObject Top = Instantiate(WallFab, Pos, Quaternion.identity);
		            Top.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);
					m_Walls.Add(Top);
                }

                if (Maze[x, z].Pared.HasFlag(Celula.RIGHT))
                {
                    Pos = new Vector3((x - m_x / 2 + 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
		            GameObject Right = Instantiate(WallFab, Pos, Quaternion.identity);
		            Right.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);
 					m_Walls.Add(Right);
                }

                if (Maze[x, z].Pared.HasFlag(Celula.BOTTOM))
                {
//                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
		            GameObject Bottom = Instantiate(WallFab, Pos, Quaternion.identity);
		            Bottom.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);
 					m_Walls.Add(Bottom);
                }
            }

*/

 
        for (x = 1; x < m_x - 1; x++)
            for (z = 1; z < m_z - 1; z++)
            {
                if (Maze[x, z].bWall[0])
                {
                    Pos = new Vector3((x - m_x / 2 - 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
                    GameObject Left = Instantiate(WallFab, Pos, Quaternion.identity);
                    Left.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);
					m_Walls.Add(Left);
                }


                if (Maze[x, z].bWall[1])
                {
//                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
		            GameObject Top = Instantiate(WallFab, Pos, Quaternion.identity);
		            Top.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);
					m_Walls.Add(Top);
                }

                if (Maze[x, z].bWall[2])
                {
                    Pos = new Vector3((x - m_x / 2 + 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
		            GameObject Right = Instantiate(WallFab, Pos, Quaternion.identity);
		            Right.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);
 					m_Walls.Add(Right);
                }

                if (Maze[x, z].bWall[3])
                {
//                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
                    Pos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
		            GameObject Bottom = Instantiate(WallFab, Pos, Quaternion.identity);
		            Bottom.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);
 					m_Walls.Add(Bottom);
                }
            }

    }

    public void DrawTortuga()
	{
		if (TugaObj)
			Destroy(TugaObj);

        Vector3 Pos = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        TugaObj = Instantiate(WallFab, Pos, Quaternion.identity);
		TugaObj.transform.localScale = new Vector3(2.0f, 10.0f, 2.0f);

        MeshRenderer Mesh = TugaObj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.black;
	}

    public void DrawVecinos(CELDAINFO Tort)
	{
		ClearRuta();
		int nCant = Maze[Tort.x, Tort.z].m_Vecinos.Count;
		CELDAINFO Vec;

		for (int i = 0; i < nCant; i++)
		{
			Vec = Maze[Tort.x, Tort.z].m_Vecinos[i];

	        Vector3 Pos = new Vector3((Vec.x - m_x / 2) * Lado, 1.5f, (Vec.z - m_z / 2) * Lado);
    	    GameObject VecObj = Instantiate(WallFab, Pos, Quaternion.identity);
			VecObj.transform.localScale = new Vector3(2.0f, 10.0f, 2.0f);

    	    MeshRenderer Mesh = VecObj.GetComponent<MeshRenderer>();
        	Mesh.material.color = Color.blue;

			m_Ruta.Add(VecObj);
		}
	}

	void DrawRuta(CELDAINFO Tort)
	{
//		int nCant, i;			
//		CELDAINFO Ruta;				ClearRuta();

/*
		Ruta = Tort.m_Parent;

		while (Ruta != null)
		{
	        Vector3 Pos = new Vector3((Ruta.x - m_x / 2) * Lado, 1.5f, (Ruta.z - m_z / 2) * Lado);
    	    GameObject VecObj = Instantiate(WallFab, Pos, Quaternion.identity);
			VecObj.transform.localScale = new Vector3(2.0f, 10.0f, 2.0f);

    	    MeshRenderer Mesh = VecObj.GetComponent<MeshRenderer>();
        	Mesh.material.color = Color.blue;

			m_Ruta.Add(VecObj);

			Ruta = Ruta.m_Parent;
		}
*/
		
	}
	


	void ResolverMaze()
	{
		if (Tortuga.x == End.x && Tortuga.z == End.z)
		{
			Debug.Log("Destino Logrado .... GAME OVER");

			return;
		}

		Maze[Tortuga.x, Tortuga.z].Pared |= Celula.VISITED;

		if (m_OpenSet.Contains(Tortuga))		
		{			
	        int nIndex = m_OpenSet.IndexOf(Tortuga);
            m_OpenSet.RemoveAt(nIndex);
//			Debug.Log("Resolver ...Tortuga in OpenSet");
		}

		Tortuga = SetTortuga();
		Debug.Log("Tort Direccion " + Maze[Tortuga.x, Tortuga.z].Direction);
		SetVecindad(Tortuga);

			


		if (Tortuga.x == End.x && Tortuga.z == End.z)
			Debug.Log("FANTASTICO");


//		Debug.Log("Vecino " + Maze[Tortuga.x, Tortuga.z].m_Vecinos.Count);
//		Debug.Log("Resolver Costo " + (Maze[Tortuga.x, Tortuga.z].gCost + Maze[Tortuga.x, Tortuga.z].hCost));


/*
        Vector3 Positio = new Vector3((Tortuga.x - m_x / 2) * Lado, 1.5f, (Tortuga.z - m_z / 2) * Lado);
        GameObject Tuga = Instantiate(WallFab, Positio, Quaternion.identity);
        MeshRenderer Mesh = Tuga.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.yellow;
		m_Ruta.Add(Tuga);
*/
	}

}
