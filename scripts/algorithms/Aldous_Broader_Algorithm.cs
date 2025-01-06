using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class Aldous_Broder_Algorithm
{
    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        MazeCellCoords[] neighbors = new MazeCellCoords[4];

        maze[selCell.y][selCell.x].visited = true;
        while (maze.AsParallel().Select(r => r.AsEnumerable().Where(c => !c.visited).Any()).AsEnumerable().Where(r => r).Any())
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(c => c.isValid());
            var neighbor = querry.ElementAt(random.Next(0, querry.Count()));
            if (!maze[neighbor.y][neighbor.x].visited) MergeCells(maze, selCell, neighbor);

            selCell = neighbor;
            maze[selCell.y][selCell.x].visited = true;
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }
}
