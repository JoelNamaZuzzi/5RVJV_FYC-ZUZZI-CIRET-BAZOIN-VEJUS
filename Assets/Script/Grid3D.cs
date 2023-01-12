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
    public float[,,] pressure;
    public GameObject bubullePrefab;
    public int nbBubulle;
    public GameObject[] bubulles;

    void Awake()
    {
        //Init Grid et bubulles
        cell_size = new Vector3(grid_size.x / cells_x, grid_size.y / cells_y, grid_size.z / cells_z);
        velocity = new Vector3[cells_x, cells_y, cells_z];
        density = new float[cells_x, cells_y, cells_z];
        pressure = new float[cells_x, cells_y, cells_z];
        //Init grid avec les cells à 0 partout
        for (int i = 0; i < cells_x; i++) {
            for (int j = 0; j < cells_y; j++) {
                for (int k = 0; k < cells_z; k++)
                {
                    velocity[i, j, k] = Vector3.zero;
                    density[i, j, k] = 0.0f;
                    pressure[i, j, k] = 0.0f;
                }
            }
        }
        //Init bubulles et les mettre dans la liste
        bubulles = new GameObject[nbBubulle];
        for(int i=0; i<nbBubulle; i++)
        {
            Vector3 gridOrg = transform.position;
            Vector3 pos = new Vector3(Random.Range(0, grid_size.x * cells_x+gridOrg.x), Random.Range(0, grid_size.y * cells_y+gridOrg.y),
                Random.Range(0, grid_size.z * cells_z+gridOrg.z));
            GameObject bubulle = Instantiate(bubullePrefab, pos, Quaternion.identity);
            bubulles.Append(bubulle);
            bubulle.transform.parent = transform;
        }
    }
    
    //Maj particules et fluides
    void UpdateFluid(Grid3D grid, float dt) {
        // Etape 1: Advection
        foreach (GameObject bubulle in bubulles)
        {
            Advection(bubulle, dt);
        }

        // Step 2: Projection
        // Solve Poisson equation to calculate new pressure
        // Calculate new density
        // Calculate new velocity
        // ...
        // Calculate divergence of velocity field
        float[,,] divergence = new float[cells_x, cells_y, cells_z];
        for (int i = 1; i < cells_x-1; i++) {
            for (int j = 1; j < cells_y-1; j++) {
                for (int k = 1; k < cells_z-1; k++)
                {
                    divergence[i, j, k] = (velocity[i+1, j, k].x - velocity[i-1, j, k].x)/(2 * cell_size.x)
                                          + (velocity[i, j+1, k].y - velocity[i, j-1, k].y)/(2 * cell_size.y)
                                          + (velocity[i, j, k+1].z - velocity[i, j, k-1].z)/(2 * cell_size.z);
                }
            }
        }

        // Solve Poisson equation to calculate pressure
        float[,,] pressure = new float[cells_x, cells_y, cells_z];
        // Perform Jacobi iterations to solve Poisson equation
        // and calculate pressure

        // Correct velocity field using pressure
        for (int i = 1; i < cells_x-1; i++) {
            for (int j = 1; j < cells_y-1; j++) {
                for (int k = 1; k < cells_z-1; k++)
                {
                    velocity[i, j, k].x -= (pressure[i+1, j, k] - pressure[i-1, j, k]) * dt / cell_size.x;
                    velocity[i, j, k].y -= (pressure[i, j+1, k] - pressure[i, j-1, k]) * dt / cell_size.y;
                    velocity[i, j, k].z -= (pressure[i, j, k+1] - pressure[i, j, k-1]) * dt / cell_size.z;
                }
            }
        }
        // Update grid with new properties
        //velocity = new_velocity;
        // density = new_density;
        // pressure = new_pressure;
    }
    //Advection Semi Lagrangienne, permet de mettre à jour de façon précise les positions et autres parametre des particules
    void Advection(GameObject bubulle, float dt)
    {
        Bubulle bubulleData = bubulle.GetComponent<Bubulle>();
        Vector3 pos = bubulleData.position;
        //Vector3 vel = TrilinearInterpolation
        //Vector3 newPos = pos - vel*dt;
        //vel = TrilinearInterpolation (en utilisant new pos)
        //bubulle.transform.position = pos -vel*dt;
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
    
    public float TrilinéairInterpolate(float[,,]gridData,Vector3 pos)
    {
        pos -= Vector3.Scale(Vector3.one , cell_size) * 0.5f;
        pos = Vector3.Scale(pos, new Vector3(1f / cell_size.x, 1f / cell_size.y, 1f / cell_size.z));

        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        float x_lerp = pos.x - x;
        float y_lerp = pos.y - y;
        float z_lerp = pos.z - z;

        float x00 = Mathf.Lerp(gridData[x, y, z], gridData[x+1, y, z], x_lerp);
        float x10 = Mathf.Lerp(gridData[x, y+1, z], gridData[x+1, y+1, z], x_lerp);
        float x01 = Mathf.Lerp(gridData[x, y, z+1], gridData[x+1, y, z+1], x_lerp);
        float x11 = Mathf.Lerp(gridData[x, y+1, z+1], gridData[x+1, y+1, z+1], x_lerp);

        float y0 = Mathf.Lerp(x00, x10, y_lerp);
        float y1 = Mathf.Lerp(x01, x11, y_lerp);

        return Mathf.Lerp(y0, y1, z_lerp);
    }
}



