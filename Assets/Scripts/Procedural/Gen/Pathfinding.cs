using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private GridCell[,] grid;
    private int gridWidth;
    private int gridHeight;

    public Pathfinding(GridCell[,] grid, int gridWidth, int gridHeight)
    {
        this.grid = grid;
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
    }

    public List<GridCell> FindPath(GridCell startCell, GridCell endCell, System.Func<GridCell, bool> isWalkable)
    {
        Queue<GridCell> queue = new Queue<GridCell>();
        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            if (current == endCell)
            {
                // Путь найден
                return ReconstructPath(cameFrom, startCell, endCell);
            }

            foreach (GridCell neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && isWalkable(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // Путь не найден
        return null;
    }

    private List<GridCell> ReconstructPath(Dictionary<GridCell, GridCell> cameFrom, GridCell startCell, GridCell endCell)
    {
        List<GridCell> path = new List<GridCell>();
        GridCell current = endCell;

        while (current != startCell)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(startCell);
        path.Reverse();
        return path;
    }

    private List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        int x = cell.gridX;
        int y = cell.gridY;

        // Соседи по четырем направлениям
        if (x > 0) neighbors.Add(grid[x - 1, y]);
        if (x < gridWidth - 1) neighbors.Add(grid[x + 1, y]);
        if (y > 0) neighbors.Add(grid[x, y - 1]);
        if (y < gridHeight - 1) neighbors.Add(grid[x, y + 1]);

        return neighbors;
    }
}
