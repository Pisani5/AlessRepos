using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Casilla      // : MonoBehaviour
{
    public float Lado;
    public GameObject[] m_Walls;
    public Vector3 m_Pos;
    public bool bVisited, bBlock;
    public bool[] bWall;
    public List<Casilla> m_Vecinos;
    public Casilla m_Parent;

    public float gCost, hCost;
    public int rumbo;
}


public class NavegarScript : MonoBehaviour
{
    public GameObject _Wall, _Block;

    public float Lado = 8.0f;

    //    [SerializeField]
    public Casilla[,] m_Celda;

    public Casilla m_Start, m_End, m_Tortuga;
    GameObject m_StartObject, m_EndObject, m_TortugaObject;

    public int m_x = 10;
    public int m_z = 10;

    public List<Casilla> m_TortList = new List<Casilla>();
    public List<Casilla> m_OpenSet = new List<Casilla>();
    public List<Casilla> m_ClosedSet = new List<Casilla>();

    public List<GameObject> m_Walls = new List<GameObject>();


    enum m_Estado { PS_RESET, PS_GENERAR, PS_RESOLVER, PS_DEFAULT }
    m_Estado GameState;


    // Start is called before the first frame update
    void Start()
    {
        CreateMaze();
        ResetMaze();
//        DrawMaze();

        GameState = m_Estado.PS_RESET;
    }

    // Update is called once per frame
    void Update()
    {
//        int x, z;

        switch (GameState)
        {
            case m_Estado.PS_RESET:
                break;
            case m_Estado.PS_GENERAR:
                if (GenCompleto())
                    GameState = m_Estado.PS_DEFAULT;

//                GenLab();
                GenerateMaze();
                break;
            case m_Estado.PS_RESOLVER:
                ResolverMaze();
//                DrawTortuga();
                break;
            case m_Estado.PS_DEFAULT:
//                DrawMaze();
                break;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearMaze();        ResetMaze();        //DrawMaze();         DrawTortuga();
            m_TortList.Clear();         m_OpenSet.Clear();          m_ClosedSet.Clear();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            GameState = m_Estado.PS_GENERAR;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            m_Tortuga = m_Start;        m_OpenSet.Add(m_Start);

            GameState = m_Estado.PS_RESOLVER;
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
		{
            GenerateMaze();
        }

        ClearMaze();        DrawMaze();     DrawTortuga();      DrawEnds();         DrawBlocks();

        
        if (GameState == m_Estado.PS_DEFAULT && m_Tortuga == m_End)
            DrawRuta();

    }

    public void CreateMaze()
    {
        Casilla Cell;        int x, z;

        m_Celda = new Casilla[m_x, m_z];


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                Cell = new Casilla();

//                Cell.m_Walls = new GameObject[4];

//                Cell.m_Walls[0] = null;
//                Cell.m_Walls[1] = null;
//                Cell.m_Walls[2] = null;
//                Cell.m_Walls[3] = null;

                Cell.m_Pos = new Vector3((x - m_x / 2) * Lado, 0.5f, (z - m_z / 2) * Lado);
                Cell.bVisited = false;
                Cell.bBlock = false;

                Cell.bWall = new bool[4];

                Cell.bWall[0] = true;
                Cell.bWall[1] = true;
                Cell.bWall[2] = true;
                Cell.bWall[3] = true;


                Cell.m_Vecinos = new List<Casilla>();
                Cell.m_Parent = null;

                m_Celda[x, z] = Cell;
            }


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (x - 1 >= 0)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x - 1, z]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (z - 1 >= 0)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x, z - 1]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (x + 1 < m_x)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x + 1, z]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (z + 1 < m_z)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x, z + 1]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);

            }

        m_Tortuga       = new Casilla();
        m_Start         = new Casilla();
        m_End           = new Casilla();
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


    public void ResetMaze()
    {
        int x, z; int nBlocks = 0;

        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_End = m_Celda[x, z];


        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_Start = m_Celda[x, z];



        while (m_Start == m_End)
        {
            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_End = m_Celda[x, z];
        }



        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_Tortuga = m_Celda[x, z];


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                m_Celda[x, z].bVisited = false;
                m_Celda[x, z].bBlock   = false;

                m_Celda[x, z].bWall[0] = true;
                m_Celda[x, z].bWall[1] = true;
                m_Celda[x, z].bWall[2] = true;
                m_Celda[x, z].bWall[3] = true;

                m_Celda[x, z].m_Parent = null;

                m_Celda[x, z].gCost = Vector3.Distance(m_Celda[x, z].m_Pos, m_Start.m_Pos);       // 1500.0f;
                m_Celda[x, z].hCost = Vector3.Distance(m_Celda[x, z].m_Pos, m_End.m_Pos);

                m_Celda[x, z].rumbo = -1;
            }

        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);

        while (nBlocks < (int)(m_x * m_z / 20))
        {
            while (m_Celda[x, z] == m_Start || m_Celda[x, z] == m_End)
            { 
                x = Random.Range(0, m_x);
                z = Random.Range(0, m_z);

                m_Celda[x, z].bBlock = false;
                Debug.Log("Repeticion");
            }


            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_Celda[x, z].bBlock = true;

            nBlocks++;
        }
    }

    public bool GenCompleto()
    {
        bool bVisited = true; int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (bVisited)
                    bVisited = m_Celda[x, z].bVisited;

        return bVisited;
    }

    public int GenPercent()
    {
        int pc = 0;
        int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (m_Celda[x, z].bVisited)
                    pc++;

        return (int)(100 * pc / m_x / m_z);
    }


    public void GenerateMaze()
    {
        if (m_Tortuga == null || GenCompleto())
            return;


        m_Tortuga.bVisited = true;
        m_TortList.Add(m_Tortuga);

        int nCant = m_Tortuga.m_Vecinos.Count;

        int nVec = Random.Range(0, nCant);

        Casilla Vec = m_Tortuga.m_Vecinos[nVec];

        while (Vec == null)
        {
            nVec = Random.Range(0, nCant);
            Vec = m_Tortuga.m_Vecinos[nVec];
        }


        if (Vec != null)
        {
            if (Vec.bVisited == false)      // && m_Tortuga.bWall[nVec])
            {
                m_Tortuga.bWall[nVec] = false;
                nVec = (nVec + 2) % nCant;
                Vec.bWall[nVec] = false;

                m_Tortuga = Vec;
            }
            else if (m_TortList.Count > 0)// && !GenCompleto())
            {
                m_Tortuga = m_TortList[0];
                m_TortList.RemoveAt(0);
            }
        }

        Debug.Log("Tortugas " + m_TortList.Count);
        Debug.Log("Gen " + GenPercent());
    }


    public void GenLab()
    {
        if (m_Tortuga == null || GenCompleto())     // || m_TortList.Count < 1)
            return;


        m_Tortuga.bVisited = true;
//        m_TortList.Add(m_Tortuga);

        int nCant = m_Tortuga.m_Vecinos.Count;

        int nVec = Random.Range(0, nCant);

        Casilla Vec = m_Tortuga.m_Vecinos[nVec];        bool bVisited = true;       //Vec.bVisited;

        int Num = 0;

        while (bVisited && Vec != null)
        {
            nVec = Random.Range(0, nCant);
            Vec = m_Tortuga.m_Vecinos[nVec];             
            bVisited = Vec.bVisited;

            Num ++;
        }


            if (bVisited == false)      // && m_Tortuga.bWall[nVec])
            {
                m_Tortuga.bWall[nVec] = false;
                nVec = (nVec + 2) % nCant;
                Vec.bWall[nVec] = false;

                m_TortList.Add(m_Tortuga);
                m_Tortuga = Vec;
            }
            else if (m_TortList.Count > 0)// && !GenCompleto())
            {
                m_Tortuga = m_TortList[0];
                m_TortList.RemoveAt(0);
            }
        

        Debug.Log("Tortugas " + m_TortList.Count);
        Debug.Log("Gen " + GenPercent());
    }



    public void DrawMaze()
    {
        int x, z;        Vector3 WallPos;        GameObject go;
        float yScala = 6.0f;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
//                for (i = 0; i < 4; i++)
//                {
//                    if (m_Celda[x, z].m_Walls[i])
//                        Destroy(m_Celda[x, z].m_Walls[i]);
//                }


                if (m_Celda[x, z].bWall[0])
                {
                    WallPos = new Vector3((x - m_x / 2 - 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
//                    m_Celda[x, z].m_Walls[0] = Instantiate(_Wall, WallPos, Quaternion.identity);
//                    m_Celda[x, z].m_Walls[0].transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);


                    go = Instantiate(_Wall, WallPos, Quaternion.identity);
                    go.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);

                    m_Walls.Add(go);
                }


                if (m_Celda[x, z].bWall[1])
                {
                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
//                    m_Celda[x, z].m_Walls[1] = Instantiate(_Wall, WallPos, Quaternion.identity);
//                    m_Celda[x, z].m_Walls[1].transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);
                    
                    go = Instantiate(_Wall, WallPos, Quaternion.identity);
                    go.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);

                    m_Walls.Add(go);
                }

                if (m_Celda[x, z].bWall[2])
                {
                    WallPos = new Vector3((x - m_x / 2 + 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
//                    m_Celda[x, z].m_Walls[2] = Instantiate(_Wall, WallPos, Quaternion.identity);
//                    m_Celda[x, z].m_Walls[2].transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);


                    go = Instantiate(_Wall, WallPos, Quaternion.identity);
                    go.transform.localScale = new Vector3(1.5f, yScala, Lado + 1.0f);

                    m_Walls.Add(go);
                }

                if (m_Celda[x, z].bWall[3])
                {
                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
//                    m_Celda[x, z].m_Walls[3] = Instantiate(_Wall, WallPos, Quaternion.identity);
//                    m_Celda[x, z].m_Walls[3].transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);


                    go = Instantiate(_Wall, WallPos, Quaternion.identity);
                    go.transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.5f);

                    m_Walls.Add(go);
                }
            }
    }

    public void DrawTortuga()
    {
        if (m_Tortuga == null)
            return;


        Vector3 Pos = new Vector3((m_Tortuga.m_Pos.x), 1.5f, (m_Tortuga.m_Pos.z));

/*
        if (m_TortugaObject)
            Destroy(m_TortugaObject);


        m_TortugaObject = Instantiate(_Wall, Pos, Quaternion.identity);
        m_TortugaObject.transform.localScale = new Vector3(3.0f, 6.0f, 3.0f);

        MeshRenderer Mesh = m_TortugaObject.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.black;
*/


        GameObject tort = Instantiate(_Wall, Pos, Quaternion.identity);
        tort.transform.localScale = new Vector3(3.0f, 6.0f, 3.0f);

        MeshRenderer Mesh = tort.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.black;              m_Walls.Add(tort);
    }

    public void DrawEnds()
    {
        Vector3 Pos = new Vector3((m_Start.m_Pos.x), 1.5f, (m_Start.m_Pos.z));

        GameObject Obj = Instantiate(_Wall, Pos, Quaternion.identity);
        Obj.transform.localScale = new Vector3(3.0f, 6.0f, 3.0f);

        MeshRenderer Mesh = Obj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.green;          m_Walls.Add(Obj);

        Pos = new Vector3((m_End.m_Pos.x), 1.5f, (m_End.m_Pos.z));

        Obj = Instantiate(_Wall, Pos, Quaternion.identity);
        Obj.transform.localScale = new Vector3(3.0f, 6.0f, 3.0f);

        Mesh = Obj.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.red;          m_Walls.Add(Obj);
    }

    public void DrawBlocks()
    {
        int x, z;        //Vector3 WallPos;
        float yScala = 6.0f;
        GameObject go;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (m_Celda[x, z].bBlock)
                {
                    //                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
                    go = Instantiate(_Wall, m_Celda[x, z].m_Pos, Quaternion.identity);
                    m_Walls.Add(go);
                    go.transform.localScale = new Vector3(2.0f, yScala, 2.0f);
                    MeshRenderer Mesh = go.GetComponent<MeshRenderer>();

                    Mesh.material.color = Color.white;
                }
            }
    }

    public void DrawRuta()
    {
        Vector3 Pos;

        if (m_Tortuga == null)
        {
            Debug.Log("Ruta Imposible");
            return;
        }

        Casilla par = m_Tortuga.m_Parent;
        GameObject go;

        while (par != null && par != m_Start)
        {
            Pos = new Vector3(par.m_Pos.x, 1.5f, (par.m_Pos.z));
            go = Instantiate(_Wall, Pos, Quaternion.identity);
            m_Walls.Add(go);
            //            go.transform.localScale = new Vector3(1.0f, 2.0f, 1.0f);
            MeshRenderer Mesh = go.GetComponent<MeshRenderer>();
            Mesh.material.color = Color.yellow;

            par = par.m_Parent;
        }
    }


    public void ResolverMaze()
    {
        if (m_Tortuga == m_End)
        {
            DrawRuta();
            Debug.Log("Logrado");
            GameState = m_Estado.PS_DEFAULT;
            return;
        }

        m_Tortuga = SetTortuga();
        SetVecinos(m_Tortuga);

        m_ClosedSet.Add(m_Tortuga);


        if (m_OpenSet.Count > 0)
        {
            int nIndex = m_OpenSet.IndexOf(m_Tortuga);
            m_OpenSet.RemoveAt(nIndex);
        }
        else
        {
            Debug.Log("Imposible");
            GameState = m_Estado.PS_DEFAULT;
        }

//        SetVecinos(m_Tortuga);
    }

    Casilla SetTortuga()
    {
        int i;          Casilla tort;         
        Casilla salida = null;      //m_Start;

        float nCost = 10000.0f;

        for (i = 0; i < m_OpenSet.Count; i++)
        {
            tort = m_OpenSet[i];

            if (tort.gCost + tort.hCost < nCost)
            {
                nCost = tort.gCost + tort.hCost;

                salida = tort;      // m_OpenSet[i];
            }
        }

        return salida;
    }

    public Casilla SetVecinos(Casilla tort)
    {
        if (tort == null)
        {
//            Debug.Log("Rutina");
            return null;
        }

        float dist;

        int i, vec;         Casilla cVec = null;// = new Celda();

        vec = tort.m_Vecinos.Count;

        for (i = 0; i < 4; i++)
        {
            cVec = tort.m_Vecinos[i];

            if (cVec != null)
            {
                if (!m_ClosedSet.Contains(cVec) && !tort.bWall[i] && !cVec.bBlock)
//                if (!m_ClosedSet.Contains(cVec) && !cVec.bBlock)
                {
                    if (m_OpenSet.Contains(cVec))
                    {
                        dist = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);

                        if (dist < cVec.gCost)
                        {
                            cVec.gCost = dist;
                            cVec.m_Parent = tort;
                            //                            cVec.gCost = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);
                            cVec.hCost = Vector3.Distance(cVec.m_Pos, m_End.m_Pos);
                            cVec.rumbo = i;
                        }
                    }
                    else
                    {
                        cVec.m_Parent = tort;
                        cVec.gCost = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);
                        cVec.hCost = Vector3.Distance(cVec.m_Pos, m_End.m_Pos);
                        cVec.rumbo = i;

                        m_OpenSet.Add(cVec);
                    }
                }
            }
        }

        return cVec;
    }

    public void ResetParents()
    {
        for (int x = 0; x < m_x; x++)
            for (int z = 0; z < m_z; z++)
                m_Celda[x, z].m_Parent = null;
    }
}
