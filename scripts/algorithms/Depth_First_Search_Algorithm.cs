using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class Depth_First_Search_Algorithm
{
    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        MazeCellCoords selCell = null;
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        Stack<MazeCellCoords> stack = new();
        stack.Push(new(random.Next(0, width), random.Next(0, height)));

        while (stack.Any())
        {
            selCell = stack.Pop();
            maze[selCell.y][selCell.x].visited = true;

            FetchNeighbors(neighbors, selCell, width, height);
            var querry = neighbors.AsQueryable().Where(c => c.isValid() && !maze[c.y][c.x].visited);

            if (querry.Any())
            {
                stack.Push(selCell);

                var neighbor = querry.ElementAt(random.Next(0, querry.Count()));

                MergeCells(maze, selCell, neighbor);
                stack.Push(neighbor);
            }
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.Select(r => r.Select(c => c.value).ToArray()).ToArray();
    }
}
