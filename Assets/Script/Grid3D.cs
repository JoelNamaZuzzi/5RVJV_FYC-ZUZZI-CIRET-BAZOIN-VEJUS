using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid3D : MonoBehaviour
{
    public int cells_x, cells_y, cells_z;

    //public Vector3 grid_size;
    public float cell_size = 1.0f;
    public int nbBubulle;
    public int maxIterProjection = 5;
    public Vector3[,,] velocity;
    public float[,,] density;
    public float[,,] pressures;
    public GameObject bubullePrefab;
    public List<GameObject> bubulles;
    private float minx, maxx, miny, maxy, minz, maxz;
    private Vector3[,,] divergence;

    void Awake()
    {
        //Init Grid et bubulles
        Vector3 gridOrg = transform.position;
        velocity = new Vector3[cells_x, cells_y, cells_z];
        density = new float[cells_x, cells_y, cells_z];
        pressures = new float[cells_x, cells_y, cells_z];
        divergence = new Vector3[cells_x, cells_y, cells_z];
        //Init grid avec les cells à 0 partout
        for (int i = 0; i < cells_x; i++)
        {
            for (int j = 0; j < cells_y; j++)
            {
                for (int k = 0; k < cells_z; k++)
                {
                    //velocity[i, j, k] = Vector3.zero;
                    velocity[i, j, k] = new Vector3(Random.Range(-1,1), 0, Random.Range(-1,1));
                    density[i, j, k] = 0.0f;
                    pressures[i, j, k] = 0.0f;
                    divergence[i, j, k] = new Vector3(0, 0, 0);
                }
            }
        }

        //Init gridOrg pour les calculs de position
        minx = gridOrg.x;
        miny = gridOrg.y;
        minz = gridOrg.z;
        maxx = gridOrg.x + cells_x;
        maxy = gridOrg.y + cells_y;
        maxz = gridOrg.z + cells_z;

        //Init bubulles et les mettre dans la liste
        bubulles = new List<GameObject>();
        for (int i = 0; i < nbBubulle; i++)
        {
            Vector3 pos = new Vector3(Random.Range(0 + gridOrg.x, cells_x + gridOrg.x),
                Random.Range(0 + gridOrg.y, cells_y + gridOrg.y),
                Random.Range(0 + gridOrg.z, cells_z + gridOrg.z));
            GameObject bubulle = Instantiate(bubullePrefab, pos, Quaternion.identity);
            bubulles.Add(bubulle);
            bubulle.transform.parent = transform;
            bubulle.GetComponent<Bubulle>().position = pos;
            bubulle.GetComponent<Bubulle>().velocity = new Vector3(Random.Range(-0.1f,0.1f), 0, Random.Range(-0.1f,0.1f));
            bubulle.name = "bubulle" + i;
        }
    }

    void Update()
    {
        UpdateFluid(Time.deltaTime);
    }

    //Maj particules et fluides
    void UpdateFluid(float dt)
    {
        // Etape 1: Advection
        foreach (GameObject bubulle in bubulles)
        {
            Advection(bubulle, dt);
        }

        // Etape 2 Projection
        Projection();
    }


    //Advection Semi Lagrangienne, permet de mettre à jour de façon précise les positions
    //et autres parametre des particules sauf la vélocité
    void Advection(GameObject bubulle, float dt)
    {
        Vector3 bubullepos = bubulle.transform.position;
        Vector3 bubullevec = bubulle.GetComponent<Bubulle>().velocity;
        Debug.Log(bubulle.name);
        Vector3 vel = TrilinéairInterpolate(velocity, bubulle, bubullepos);
        Vector3 newPos = bubullepos - dt * vel;
        vel = TrilinéairInterpolate(velocity, bubulle, newPos);
        //Debug.Log(bubulle.name + newPos);
        newPos = new Vector3(newPos.x + bubullevec.x, newPos.y + bubullevec.y, newPos.z + bubullevec.z) - vel * dt;
        newPos.x = Mathf.Repeat(newPos.x - minx, maxx) + minx;
        //newPos.y = Mathf.Repeat(newPos.y - miny, maxy)+miny;
        newPos.z = Mathf.Repeat(newPos.z - minz, maxz) + minz;
        bubulle.transform.position = newPos;
        //Debug.Log(bubulle.name + bubullepos);
    }

    // exmple étape 7 pour densité
    public void GridDataDensity()
    {

        int gridResolution = density.GetLength(0);
        // Initialiser toutes les cellules de la grille à zéro
        Array.Clear(density, 0, density.Length);

        // Pour chaque particule, ajouter sa densité et sa vitesse à la cellule de la grille la plus proche
        foreach (GameObject particle in bubulles)
        {
            Bubulle particledata = particle.GetComponent<Bubulle>();
            int x = (int)(particledata.position.x * gridResolution);
            int y = (int)(particledata.position.y * gridResolution);
            int z = (int)(particledata.position.z * gridResolution);
            density[x, y, z] += particledata.density;
        }

        // Pour chaque cellule de la grille, diviser par le nombre de particules qui ont contribué à la cellule pour obtenir la densité et la vitesse moyennes
        for (int x = 0; x < gridResolution; x++)
        {
            for (int y = 0; y < gridResolution; y++)
            {
                for (int z = 0; z < gridResolution; z++)
                {
                    float dcount = density[x, y, z];
                    if (dcount > 0)
                    {
                        density[x, y, z] /= dcount;
                    }
                }
            }
        }
    }

    //Interpolation trilinéaire retournant un float
    public float TrilinéairInterpolate(float[,,] gridData, GameObject bubulle, Vector3 pos)
    {
        pos = bubulle.transform.position;
        Vector3 gridPosition = (pos - transform.position); // gridSize;

        Debug.Log(gridPosition);
        //Debug.Log("x: "+gridPosition.x+" y: "+gridPosition.y+" z: "+gridPosition.z);
        int x0 = Mathf.FloorToInt(gridPosition.x);
        int y0 = Mathf.FloorToInt(gridPosition.y);
        int z0 = Mathf.FloorToInt(gridPosition.z);
        if (x0 < 0)
        {
            x0 = (int)transform.position.x;
            bubulle.GetComponent<Bubulle>().velocity.x = -bubulle.GetComponent<Bubulle>().velocity.x;
        }
        else if (x0 > cells_x)
        {
            x0 = cells_x;
            bubulle.GetComponent<Bubulle>().velocity.x = -bubulle.GetComponent<Bubulle>().velocity.x;
        }

        if (y0 < 0)
        {
            y0 = (int)transform.position.y;
            bubulle.GetComponent<Bubulle>().velocity.y = -bubulle.GetComponent<Bubulle>().velocity.y;
        }
        else if (y0 > cells_y)
        {
            y0 = cells_y;
            bubulle.GetComponent<Bubulle>().velocity.y = -bubulle.GetComponent<Bubulle>().velocity.y;
        }

        if (z0 < 0)
        {
            z0 = (int)transform.position.z;
            bubulle.GetComponent<Bubulle>().velocity.z = -bubulle.GetComponent<Bubulle>().velocity.z;
        }
        else if (z0 > cells_z)
        {
            z0 = cells_z;
            bubulle.GetComponent<Bubulle>().velocity.z = -bubulle.GetComponent<Bubulle>().velocity.z;
        }

        //Debug.Log("x0: "+x0+" y0: "+y0+" z0: "+z0);
        int x1 = Mathf.Clamp(x0 + 1, 0, cells_x - 1);
        int y1 = Mathf.Clamp(y0 + 1, 0, cells_y - 1);
        int z1 = Mathf.Clamp(z0 + 1, 0, cells_z - 1);

        float xd = Mathf.Clamp(gridPosition.x - x0, 0, cells_x - 1);
        float yd = Mathf.Clamp(gridPosition.y - y0, 0, cells_y - 1);
        float zd = Mathf.Clamp(gridPosition.z - z0, 0, cells_z - 1);
        //Interpolation en x
        float c00 = gridData[x0, y0, z0] * (1 - xd) + gridData[x1, y0, z0] * xd;
        float c10 = gridData[x0, y1, z0] * (1 - xd) + gridData[x1, y1, z0] * xd;
        float c01 = gridData[x0, y0, z1] * (1 - xd) + gridData[x1, y0, z1] * xd;
        float c11 = gridData[x0, y1, z1] * (1 - xd) + gridData[x1, y1, z1] * xd;
        //Interpolation en y
        float c0 = c00 * (1 - yd) + c10 * yd;
        float c1 = c01 * (1 - yd) + c11 * yd;
        //Interpolation en z
        float c = c0 * (1 - zd) + c1 * zd;
        //Debug.Log(c);
        return c;
    }

    //Interpolation trilinéaire retournant un Vector3
    public Vector3 TrilinéairInterpolate(Vector3[,,] gridData, GameObject bubulle, Vector3 pos)
    {
        pos = bubulle.transform.position;
        Vector3 gridPosition = (pos - transform.position); // gridSize;

        Debug.Log(gridPosition);
        //Debug.Log("x: "+gridPosition.x+" y: "+gridPosition.y+" z: "+gridPosition.z);
        int x0 = Mathf.FloorToInt(gridPosition.x);
        int y0 = Mathf.FloorToInt(gridPosition.y);
        int z0 = Mathf.FloorToInt(gridPosition.z);
        if (x0 < 0)
        {
            x0 = (int)transform.position.x;
            bubulle.GetComponent<Bubulle>().velocity.x = -bubulle.GetComponent<Bubulle>().velocity.x;
        }
        else if (x0 > cells_x)
        {
            x0 = cells_x;
            bubulle.GetComponent<Bubulle>().velocity.x = -bubulle.GetComponent<Bubulle>().velocity.x;
        }

        if (y0 < 0)
        {
            y0 = (int)transform.position.y;
            bubulle.GetComponent<Bubulle>().velocity.y = -bubulle.GetComponent<Bubulle>().velocity.y;
        }
        else if (y0 > cells_y)
        {
            y0 = cells_y;
            bubulle.GetComponent<Bubulle>().velocity.y = -bubulle.GetComponent<Bubulle>().velocity.y;
        }

        if (z0 < 0)
        {
            z0 = (int)transform.position.z;
            bubulle.GetComponent<Bubulle>().velocity.z = -bubulle.GetComponent<Bubulle>().velocity.z;
        }
        else if (z0 > cells_z)
        {
            z0 = cells_z;
            bubulle.GetComponent<Bubulle>().velocity.z = -bubulle.GetComponent<Bubulle>().velocity.z;
        }

        //Debug.Log("x0: "+x0+" y0: "+y0+" z0: "+z0);
        int x1 = Mathf.Clamp(x0 + 1, 0, cells_x - 1);
        int y1 = Mathf.Clamp(y0 + 1, 0, cells_y - 1);
        int z1 = Mathf.Clamp(z0 + 1, 0, cells_z - 1);

        float xd = Mathf.Clamp(gridPosition.x - x0, 0, cells_x - 1);
        float yd = Mathf.Clamp(gridPosition.y - y0, 0, cells_y - 1);
        float zd = Mathf.Clamp(gridPosition.z - z0, 0, cells_z - 1);
        //Interpolation en x
        Vector3 c00 = gridData[x0, y0, z0] * (1 - xd) + gridData[x1, y0, z0] * xd;
        Vector3 c10 = gridData[x0, y1, z0] * (1 - xd) + gridData[x1, y1, z0] * xd;
        Vector3 c01 = gridData[x0, y0, z1] * (1 - xd) + gridData[x1, y0, z1] * xd;
        Vector3 c11 = gridData[x0, y1, z1] * (1 - xd) + gridData[x1, y1, z1] * xd;
        //Interpolation en y
        Vector3 c0 = c00 * (1 - yd) + c10 * yd;
        Vector3 c1 = c01 * (1 - yd) + c11 * yd;
        //Interpolation en z
        Vector3 c = c0 * (1 - zd) + c1 * zd;
        //Debug.Log(c);
        return c;
    }

    //Projection pour mettre à jour les vélocités des particules et des cellules basée
    //sur la méthode de Staggered Grid utilisée pour résoudre les équations de Navier Strokes
    void Projection()
    {
        //Init pressures à 0
        for (int i = 0; i < cells_x; i++)
        {
            for (int j = 0; j < cells_y; j++)
            {
                for (int k = 0; k < cells_z; k++)
                {
                    pressures[i, j, k] = 0.0f;
                }
            }
        }
        //On boucle sur un certain nombre d'itérations pour avoir le résultat le plus fin que possible
        for (int i = 0; i < maxIterProjection; i++)
        {
            //Applcations des contraintes de divergence nulle
            for (int x = 1; x < cells_x - 1; x++)
            {
                for (int y = 1; y < cells_y - 1; y++)
                {
                    for (int z = 1; z < cells_z - 1; z++)
                    {
                        divergence[x, y, z] = (velocity[x + 1,y,z] - velocity[x - 1,y,z]) / cells_x + 
                                              (velocity[x,y + 1,z] - velocity[x,y - 1,z]) / cells_y +
                                              (velocity[x,y,z + 1] - velocity[x,y,z - 1]) / cells_z;
                    }
                }
            }
            //SolvePoisson()
            // Correct the velocity for each cell
            for (int x = 1; x < cells_x - 1; x++)
            {
                for (int y = 1; y < cells_y - 1; y++)
                {
                    for (int z = 1; z < cells_z - 1; z++)
                    {
                        Vector3 pressureForce = new Vector3((pressures[x + 1,y,z] - pressures[x - 1,y,z]) / (2 * cells_x),
                            (pressures[x,y + 1,z] - pressures[x,y - 1,z]) / (2 * cells_y),
                            (pressures[x,y,z + 1] - pressures[x,y,z - 1]) / (2 * cells_z));
                        velocity[x,y,z] -= pressureForce;
                    }
                }
            }

            for (int j = 0; j < bubulles.Count(); j++)
            {
                
            }
        }
    }
}