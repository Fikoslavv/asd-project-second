using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

using static Lib_Algorithm;

public static partial class Kruskals_Algorithm
{
    internal static MazeCell[][] GenerateMazeLooping(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);
        IList<HashSet<MazeCellCoords>> trees = new List<HashSet<MazeCellCoords>>();

        // IList<MazeCellCoords> path = new List<MazeCellCoords>() { new(0, 0) };
        ICollection<MazeCellCoords> path = new List<MazeCellCoords>() { new(random.Next(0, width), random.Next(0, height)) };
        var selCell = path.Last();
        
        // maze[height - 1][width - 1].visited = true;
        {
            MazeCellCoords seedCell;
            do seedCell = new(random.Next(0, width), random.Next(0, height)); while (seedCell == selCell);
            maze[seedCell.y][seedCell.x].visited = true;
            trees.Add(new(){ seedCell });
        }

        MazeCellCoords neighbor;
        var neighbors = new MazeCellCoords[4];
        while (maze.AsParallel().Select(row => row.AsEnumerable().Where(cell => !cell.visited).Any()).Where(row => row).Any())
        {
            selCell = path.Last();
            FetchNeighbors(neighbors, selCell, width, height);

            {
                var querry = neighbors.AsEnumerable().Where(n => n.isValid());
                neighbor = querry.ElementAt(random.Next(0, querry.Count()));
            }

            MergeCells(maze, selCell, neighbor);

            if (maze[neighbor.y][neighbor.x].visited)
            {
                foreach (var cell in path) maze[cell.y][cell.x].visited = true;
                trees.Where(t => t.Contains(neighbor)).First().UnionWith(path);
                path.Clear();
                
                path.Add(GetFirstUnvisitedCell(maze));
            }
            else if (path.Contains(neighbor))
            {
                foreach (var cell in path) maze[cell.y][cell.x].visited = true;
                trees.Add(path.ToHashSet());

                path.Clear();
                path.Add(GetFirstUnvisitedCell(maze));
            }
            else path.Add(neighbor);
        }

        while (trees.Count > 1)
        {
            HashSet<MazeCellCoords> neighborTree;
            var selTree = trees.ElementAt(random.Next(0, trees.Count));
            selCell = selTree.ElementAt(random.Next(0, selTree.Count));

            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(n => n.isValid() && trees.AsEnumerable().Where(t => t.Contains(n)).First() != selTree);
            if (querry.Any())
            {
                neighbor = querry.ElementAt(random.Next(0, querry.Count()));
                neighborTree = trees.AsEnumerable().Where(t => t.Contains(neighbor)).First();

                MergeCells(maze, selCell, neighbor);

                if (random.Next(0, 2) == 0)
                {
                    selTree.UnionWith(neighborTree);
                    trees.Remove(neighborTree);
                }
            }
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }
}
