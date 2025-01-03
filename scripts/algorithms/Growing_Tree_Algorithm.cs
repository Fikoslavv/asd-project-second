using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using static Lib_Algorithm;

public static partial class Growing_Tree_Algorithm
{
    internal delegate T SelectFrontier<T>(IList<T> frontier, Random random);

    internal static MazeCell[][] GenerateMaze(int width, int height)
    {
        return GenerateMaze(width, height, (frontier, random) => frontier.Last());
    }

    internal static MazeCell[][] GenerateMaze(int width, int height, SelectFrontier<MazeCellCoords> selectFrontier, Random random = default)
    {
        var maze = new MazeCellRep[height][];

        for (int y = height - 1; y >= 0; y--)
        {
            maze[y] = new MazeCellRep[width];
            for (int x = width - 1; x >= 0; x--) maze[y][x] = new() { value = MazeCell.WesternWall | MazeCell.NorthernWall | MazeCell.EasternWall | MazeCell.SouthernWall};
        }

        IList<MazeCellCoords> frontier = new List<MazeCellCoords>() { new(random.Next(0, width), height - 1) };
        var selCell = frontier.First();

        maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
        maze[selCell.y][selCell.x].visited = true;

        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        while (frontier.Count > 0)
        {
            selCell = selectFrontier(frontier, random);
            // GD.Print($"Selected Cell: {selCell}");

            FetchNeighbors(neighbors, selCell, width, height);

            {
                bool allVisited = true;
                foreach (var neighbor in neighbors) if (neighbor.isValid()) allVisited &= maze[neighbor.y][neighbor.x].visited;
                if (allVisited)
                {
                    frontier.Remove(selCell);
                    // GD.Print($"Removed Cell: {selCell}");
                    continue;
                }
            }

            MazeCellCoords selNeighbor;
            {
                var querry = neighbors.AsQueryable().Where(n => n.isValid() && !maze[n.y][n.x].visited);
                selNeighbor = querry.ElementAt((int)(random.NextDouble() * querry.Count()));
            }
            // GD.Print($"Selected Neighbor: {selNeighbor}");

            MergeCells(maze, selCell, selNeighbor);

            maze[selNeighbor.y][selNeighbor.x].visited = true;
            frontier.Add(selNeighbor);
            // GD.Print($"Added Cell: {selNeighbor}");
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;

        return maze.AsQueryable().Select(row => row.AsQueryable().Select(cell => cell.value).ToArray()).ToArray();
    }
}
