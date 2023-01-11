using System.Collections;
using System.Collections.Generic;
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
    public GameObject[,,] particleGrid;

    void Start()
    {
        //Initialize grid
        cell_size = new Vector3(grid_size.x / cells_x, grid_size.y / cells_y, grid_size.z / cells_z);
        velocity = new Vector3[cells_x, cells_y, cells_z];
        density = new float[cells_x, cells_y, cells_z];
        pressure = new float[cells_x, cells_y, cells_z];
        particleGrid = new GameObject[cells_x,cells_y,cells_z];
        // Initialize grid cells with default values
        for (int i = 0; i < cells_x; i++) {
            for (int j = 0; j < cells_y; j++) {
                for (int k = 0; k < cells_z; k++)
                {
                    velocity[i, j, k] = Vector3.zero;
                    density[i, j, k] = 0.0f;
                    pressure[i, j, k] = 0.0f;
                    //Initialize bubulle
                    Vector3 pos = Vector3.Scale(new Vector3(i,j,k), cell_size);
                    GameObject bubulle = Instantiate(bubullePrefab, pos, Quaternion.identity);
                    bubulle.transform.parent = transform;
                    particleGrid[i, j, k] = bubulle;
                }
            }
        }
    } 
    
    //Updating the cells & particles
    void UpdateFluid(Grid3D grid, float dt) {
        // Step 1: Advection
        Vector3[,,] new_velocity = new Vector3[grid.cells_x, grid.cells_y, grid.cells_z];
        for (int i = 0; i < grid.cells_x; i++) {
            for (int j = 0; j < grid.cells_y; j++) {
                for (int k = 0; k < grid.cells_z; k++)
                {   
                    // Calculate new position of particle
                    //Vector3 pos = new Vector3(i, j, k) * grid.cell_size;
                    Vector3 pos = Vector3.Scale(new Vector3(i, j, k), grid.cell_size);
                    Vector3 new_pos = pos - grid.velocity[i, j, k] * dt;

                    // Use trilinear interpolation to estimate new velocity
                    //new_velocity[i, j, k] = TrilinearInterpolation(grid.velocity, new_pos, grid.grid_size, grid.cells_x, grid.cells_y, grid.cells_z);
                }
            }
        }

        // Step 2: Projection
        // Solve Poisson equation to calculate new pressure
        // Calculate new density
        // Calculate new velocity
        // ...
        // Calculate divergence of velocity field
        float[,,] divergence = new float[grid.cells_x, grid.cells_y, grid.cells_z];
        for (int i = 1; i < grid.cells_x-1; i++) {
            for (int j = 1; j < grid.cells_y-1; j++) {
                for (int k = 1; k < grid.cells_z-1; k++)
                {
                    divergence[i, j, k] = (grid.velocity[i+1, j, k].x - grid.velocity[i-1, j, k].x)/(2 * grid.cell_size.x)
                                          + (grid.velocity[i, j+1, k].y - grid.velocity[i, j-1, k].y)/(2 * grid.cell_size.y)
                                          + (grid.velocity[i, j, k+1].z - grid.velocity[i, j, k-1].z)/(2 * grid.cell_size.z);
                }
            }
        }

        // Solve Poisson equation to calculate pressure
        float[,,] pressure = new float[grid.cells_x, grid.cells_y, grid.cells_z];
        // Perform Jacobi iterations to solve Poisson equation
        // and calculate pressure

        // Correct velocity field using pressure
        for (int i = 1; i < grid.cells_x-1; i++) {
            for (int j = 1; j < grid.cells_y-1; j++) {
                for (int k = 1; k < grid.cells_z-1; k++)
                {
                    grid.velocity[i, j, k].x -= (pressure[i+1, j, k] - pressure[i-1, j, k]) * dt / grid.cell_size.x;
                    grid.velocity[i, j, k].y -= (pressure[i, j+1, k] - pressure[i, j-1, k]) * dt / grid.cell_size.y;
                    grid.velocity[i, j, k].z -= (pressure[i, j, k+1] - pressure[i, j, k-1]) * dt / grid.cell_size.z;
                }
            }
        }
        // Update grid with new properties
        grid.velocity = new_velocity;
        // grid.density = new_density;
        // grid.pressure = new_pressure;
    }
}


