using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class Dijstras_Algorithm
{
    internal static MazeCellCoords[] FindPath(MazeCell[][] maze, MazeCellCoords from, MazeCellCoords to)
    {
        MazeCellRep[][] mazeReps = ConvertToMazeCellReps(maze);
        var neighbors = new MazeCellCoords[4];
        int height = maze.Length;
        int width = maze[0].Length;

        HashSet<MazeCellCoords> path;
        // List<MazeCellCoords> path;
        List<KeyValuePair<MazeCellCoords, MazeCellCoords>> predecessor = new(width * height);

        PriorityQueue<MazeCellCoords, int> queue = new();
        queue.Enqueue(from, 0);

        MazeCellCoords selCell = null;
        while (queue.Count > 0 && selCell != to)
        {
            queue.TryDequeue(out selCell, out var distance);
            var selRep = mazeReps[selCell.y][selCell.x];

            if (!selRep.visited)
            {
                selRep.visited = true;

                FetchNeighbors(neighbors, selCell, width, height);

                foreach (var neighbor in neighbors.Where(n => n.isValid() && !mazeReps[n.y][n.x].visited && AreCellsConnected(mazeReps, selCell, n)))
                {
                    queue.Enqueue(neighbor, distance + 1);
                    predecessor.Add(new(neighbor, selCell));
                }
            }
        }

        path = new();

        while (selCell is not null)
        {
            path.Add(selCell);
            selCell = predecessor.Where(p => p.Key == selCell).FirstOrDefault().Value;
        }

        // GD.Print($"The path => {string.Join(" → ", path)}");

        return path.ToArray();
    }
}
