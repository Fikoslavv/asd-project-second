using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

using static Lib_Algorithm;

public static partial class Wilsons_Algorithm
{
    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        // IList<MazeCellCoords> path = new List<MazeCellCoords>() { new(0, 0) };
        ICollection<MazeCellCoords> path = new List<MazeCellCoords>() { new(random.Next(0, width), random.Next(0, height)) };
        var selCell = path.Last();
        
        // maze[height - 1][width - 1].visited = true;
        {
            MazeCellCoords seedCell;
            do seedCell = new(random.Next(0, width), random.Next(0, height)); while (seedCell == selCell);
            maze[seedCell.y][seedCell.x].visited = true;
        }

        MazeCellCoords neighbor;
        var neighbors = new MazeCellCoords[4];
        ICollection<MazeCellCoords> blacklistedCells = new LinkedList<MazeCellCoords>();
        while (maze.AsParallel().Select(r => r.AsEnumerable().Where(c => !c.visited).Any()).AsEnumerable().Where(row => row).Any())
        {
            selCell = path.Last();
            FetchNeighbors(neighbors, selCell, width, height);

            {
                var querry = neighbors.AsEnumerable().Where(n => n.isValid() && !path.Contains(n) && !blacklistedCells.Contains(n));

                if (querry.Any()) neighbor = querry.ElementAt(random.Next(0, querry.Count()));
                else
                {
                    do
                    {
                        var blacklistedCell = path.Last();
                        blacklistedCells.Add(blacklistedCell);
                        path.Remove(blacklistedCell);
                        selCell = path.Last();
                        FetchNeighbors(neighbors, selCell, width, height);
                        UnmergeCells(maze, selCell, blacklistedCell);
                        querry = neighbors.AsEnumerable().Where(n => n.isValid() && !path.Contains(n) && !blacklistedCells.Contains(n));
                    }
                    while (!querry.Any() && path.Count > 0);

                    if (path.Count == 0)
                    {
                        selCell = GetFirstUnvisitedCell(maze);

                        path.Clear();
                        blacklistedCells.Clear();
                        path.Add(selCell);
                        FetchNeighbors(neighbors, selCell, width, height);
                        querry = neighbors.AsEnumerable().Where(n => n.isValid() && !path.Contains(n) && !blacklistedCells.Contains(n));
                    }

                    neighbor = querry.ElementAt(random.Next(0, querry.Count()));
                }
            }

            MergeCells(maze, selCell, neighbor);

            if (maze[neighbor.y][neighbor.x].visited)
            {
                foreach (var cell in path) maze[cell.y][cell.x].visited = true;
                path.Clear();
                blacklistedCells.Clear();
                
                path.Add(GetFirstUnvisitedCell(maze));
            }
            else path.Add(neighbor);
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }
}
