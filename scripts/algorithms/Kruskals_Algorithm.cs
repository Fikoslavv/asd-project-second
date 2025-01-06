using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class Kruskals_Algorithm
{
    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        MazeCellCoords selCell = null;
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        IList<HashSet<MazeCellCoords>> trees = new List<HashSet<MazeCellCoords>>(width * height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) trees.Add(new HashSet<MazeCellCoords> { new(x, y) });
        }

        IList<KeyValuePair<MazeCellCoords, MazeCellCoords>> edges = new List<KeyValuePair<MazeCellCoords, MazeCellCoords>>(4 * width * height - 2 * (width + height));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                selCell = new(x, y);
                FetchNeighbors(neighbors, selCell, width, height);
                foreach (var neighbor in neighbors) if (neighbor.isValid()) edges.Add(new(selCell, neighbor));
            }
        }

        KeyValuePair<MazeCellCoords, MazeCellCoords> selEdge;

        while (edges.Any())
        {
            {
                int index = random.Next(0, edges.Count);
                selEdge = edges[index];
                edges.RemoveAt(index);
            }

            var selTree = trees.AsEnumerable().Where(t => t.Contains(selEdge.Key)).First();
            var neighborTree = trees.AsEnumerable().Where(t => t.Contains(selEdge.Value)).First();

            if (selTree != neighborTree)
            {
                MergeCells(maze, selEdge.Key, selEdge.Value);
                selTree.UnionWith(neighborTree);
                trees.Remove(neighborTree);
            }
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }
}
