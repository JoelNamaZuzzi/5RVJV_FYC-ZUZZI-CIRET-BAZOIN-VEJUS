using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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
        //Initialize grid
        cell_size = new Vector3(grid_size.x / cells_x, grid_size.y / cells_y, grid_size.z / cells_z);
        velocity = new Vector3[cells_x, cells_y, cells_z];
        density = new float[cells_x, cells_y, cells_z];
        pressure = new float[cells_x, cells_y, cells_z];
        // Initialize grid cells with default values
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

        bubulles = new GameObject[nbBubulle];
        for(int i=0; i<nbBubulle; i++)
        {
            Vector3 pos = new Vector3(Random.Range(0, grid_size.x * cells_x+transform.position.x), Random.Range(0, grid_size.y * cells_y+transform.position.y),
                Random.Range(0, grid_size.z * cells_z+transform.position.z));
            GameObject bubulle = Instantiate(bubullePrefab, pos, Quaternion.identity);
            bubulles.Append(bubulle);
            bubulle.transform.parent = transform;
        }
    }
    
    //Updating the cells & particles
    void UpdateFluid(Grid3D grid, float dt) {
        // Step 1: Advection
        Vector3[,,] new_velocity = new Vector3[cells_x, cells_y, cells_z];
        for (int i = 0; i < cells_x; i++) {
            for (int j = 0; j < cells_y; j++) {
                for (int k = 0; k < cells_z; k++)
                {   
                    // Calculate new position of particle
                    //Vector3 pos = new Vector3(i, j, k) * cell_size;
                    Vector3 pos = Vector3.Scale(new Vector3(i, j, k), cell_size);
                    Vector3 new_pos = pos - velocity[i, j, k] * dt;

                    // Use trilinear interpolation to estimate new velocity
                    //new_velocity[i, j, k] = TrilinearInterpolation(velocity, new_pos, grid_size, cells_x, cells_y, cells_z);
                }
            }
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
        velocity = new_velocity;
        // density = new_density;
        // pressure = new_pressure;
    }
}


