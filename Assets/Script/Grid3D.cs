using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid3D : MonoBehaviour
{
    public int cells_x, cells_y, cells_z;
    public Vector3 grid_size;
    public Vector3 cell_size;
    public Vector3[,,] velocity;
    public float[,,] density;
    public float[,,] pressures;
    public GameObject bubullePrefab;
    public int nbBubulle;
    public List<GameObject> bubulles;

    void Awake()
    {
        //Init Grid et bubulles
        cell_size = new Vector3(grid_size.x / cells_x, grid_size.y / cells_y, grid_size.z / cells_z);
        velocity = new Vector3[cells_x, cells_y, cells_z];
        density = new float[cells_x, cells_y, cells_z];
        pressures = new float[cells_x, cells_y, cells_z];
        //Init grid avec les cells à 0 partout
        for (int i = 0; i < cells_x; i++)
        {
            for (int j = 0; j < cells_y; j++)
            {
                for (int k = 0; k < cells_z; k++)
                {
                    //velocity[i, j, k] = Vector3.zero;
                    velocity[i, j, k] = new Vector3(100, 5, 0);
                    density[i, j, k] = 0.0f;
                    pressures[i, j, k] = 0.0f;
                }
            }
        }

        //Init bubulles et les mettre dans la liste
        bubulles = new List<GameObject>();
        for (int i = 0; i < nbBubulle; i++)
        {
            Vector3 gridOrg = transform.position;
            Vector3 pos = new Vector3(Random.Range(0, grid_size.x * cells_x + gridOrg.x),
                Random.Range(0, grid_size.y * cells_y + gridOrg.y),
                Random.Range(0, grid_size.z * cells_z + gridOrg.z));
            GameObject bubulle = Instantiate(bubullePrefab, pos, Quaternion.identity);
            bubulles.Add(bubulle);
            bubulle.transform.parent = transform;
            bubulle.GetComponent<Bubulle>().position = pos;
            bubulle.GetComponent<Bubulle>().velocity = new Vector3(10, 0, 0);
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
        foreach (GameObject bubulle in bubulles)
        {
            Projection(bubulle);
        }
    }


    //Advection Semi Lagrangienne, permet de mettre à jour de façon précise les positions et autres parametre des particules
    void Advection(GameObject bubulle, float dt)
    {
        Vector3 bubullepos = bubulle.transform.position;
        Vector3 vel = TrilinéairInterpolate(velocity, bubullepos);
        Vector3 newPos = bubullepos - dt*vel;
        vel = TrilinéairInterpolate(velocity, newPos);
        //Debug.Log(bubulle.name + newPos);
        bubullepos = new Vector3(newPos.x*(grid_size.x * cells_x+transform.position.x), 
            newPos.y*(grid_size.y * cells_y+transform.position.y), 
            newPos.z*(grid_size.z * cells_z+transform.position.z))-vel*dt;
        //Debug.Log(bubullepos);
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
                    float count = density[x, y, z];
                    if (count > 0)
                    {
                        density[x, y, z] /= count;
                    }
                }
            }
        }
    }
    
    //Interpolation trilinéaire retournant un float 
    public float TrilinéairInterpolate(float[,,]gridData,Vector3 pos)
    {
        Vector3 gridPos = pos - transform.position;
        gridPos = new Vector3(gridPos.x / grid_size.x, gridPos.x / grid_size.y, gridPos.x / grid_size.z);

        int x0 = Mathf.FloorToInt(gridPos.x);
        int y0 = Mathf.FloorToInt(gridPos.y);
        int z0 = Mathf.FloorToInt(gridPos.z);

        float x1 = x0+1;
        float y1 = y0+1;
        float z1 = z0+1;

        float xd = gridPos.x - x0;
        float yd = gridPos.y - y0;
        float zd = gridPos.z - z0;

        float c00 = gridData[x0, y0, z0] * (1 - xd) + gridData[(int)x1, y0, z0] * xd;
        float c10 = gridData[x0, (int)y1, z0] * (1 - xd) + gridData[(int)x1, (int)y1, z0] * xd;
        float c01 = gridData[x0, y0, (int)z1] * (1 - xd) + gridData[(int)x1, y0, (int)z1] * xd;
        float c11 = gridData[x0, (int)y1, (int)z1] * (1 - xd) + gridData[(int)x1, (int)y1, (int)z1] * xd;

        float c0 = c00 * (1 - yd) + c10 * yd;
        float c1 = c01 * (1 - yd) + c11 * yd;

        float c = c0 * (1 - zd) + c1 * zd;
        
        return c;
    }
    
    //Interpolation trilinéaire retournant un Vector3
    public Vector3 TrilinéairInterpolate(Vector3[,,] gridData, Vector3 pos)
    {

        Vector3 gridPosition = (pos - transform.position); // gridSize;
        gridPosition = new Vector3(pos.x/(grid_size.x*cells_x), pos.y/(grid_size.y*cells_y), pos.z/(grid_size.z*cells_z));
        int x0 = Mathf.FloorToInt(gridPosition.x);
        int y0 = Mathf.FloorToInt(gridPosition.y);
        int z0 = Mathf.FloorToInt(gridPosition.z);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        int z1 = z0 + 1;

        float xd = gridPosition.x - x0;
        float yd = gridPosition.y - y0;
        float zd = gridPosition.z - z0;
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

    void Projection(GameObject bubulle)
    {
        int[,,] ParticleCounts = new int[cells_x, cells_y, cells_z];
        // Boucle sur toutes les particules
        for (int i = 0; i < nbBubulle; i++)
        {
            // Position de la particule
            Vector3 particlePosition = bubulles[i].transform.position;

            // Converti la position de la particule en coordonnées de grille
            int gridX = (int)(particlePosition.x / cells_x);
            int gridY = (int)(particlePosition.y / cells_y);
            int gridZ = (int)(particlePosition.z / cells_z);

            // Récupère la vélocité de la particule
            Vector3 particleVelocity = bubulles[i].GetComponent<Bubulle>().velocity;

            // Ajoute la vélocité de la particule à la vélocité de la cellule correspondante
            velocity[gridX, gridY, gridZ] += particleVelocity;

            // Incrémente le compteur de particules pour cette cellule
            ParticleCounts[gridX, gridY, gridZ]++;
        }

// Boucle sur toutes les cellules de la grille
        for (int x = 0; x < cells_x; x++)
        {
            for (int y = 0; y < cells_y; y++)
            {
                for (int z = 0; z < cells_z; z++)
                {
                    // Nombre de particules pour cette cellule
                    int count = ParticleCounts[x, y, z];

                    // Si il y a des particules pour cette cellule
                    if (count > 0)
                    {
                        // Calcule la moyenne de la vélocité pour cette cellule
                        velocity[x, y, z] /= count;
                    }

                    // Réinitialise le compteur de particules pour cette cellule
                    ParticleCounts[x, y, z] = 0;
                }
            }
        }

        // Boucle sur toutes les cellules de la grille
        for (int x = 0; x < cells_x; x++)
        {
            for (int y = 0; y < cells_y; y++)
            {
                for (int z = 0; z < cells_z; z++)
                {

                    //Calcul de la divergence
                    float divergence =
                        (velocity[x + 1, y, z].x - velocity[x - 1, y, z].x +
                            velocity[x, y + 1, z].y - velocity[x, y - 1, z].y +
                            velocity[x, y, z + 1].z - velocity[x, y, z - 1].z) / (2 * 1.0f);
                    //Applique les contraintes pour divergence nulle
                    pressures[x, y, z] += divergence;

                    //Calcul de la variation de pression pour chaque direction
                    float pressureX = (pressures[x + 1, y, z] - pressures[x - 1, y, z]) / (2 * cells_x);
                    float pressureY = (pressures[x, y + 1, z] - pressures[x, y - 1, z]) / (2 * cells_y);
                    float pressureZ = (pressures[x, y, z + 1] - pressures[x, y, z - 1]) / (2 * cells_z);

                    //Correction de la vélocité pour chaque direction
                    velocity[x, y, z].x -= pressureX;
                    velocity[x, y, z].y -= pressureY;
                    velocity[x, y, z].z -= pressureZ;
                }
            }
        }
    }
}



