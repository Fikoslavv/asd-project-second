using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class Prims_Algorithm
{
    private class MazeCellCoordsComparer : IComparer<MazeCellCoords>
    {
        private MazeCellRep[][] maze;
        private MazeCellCoords[] neighbors = new MazeCellCoords[4];

        public MazeCellCoordsComparer(MazeCellRep[][] maze)
        {
            this.maze = maze;
        }

        public int Compare(MazeCellCoords x, MazeCellCoords y)
        {
            FetchNeighbors(neighbors, x, maze[0].Length, maze.Length);
            int lCount = neighbors.AsQueryable().Where(c => c.isValid() && !maze[c.y][c.x].visited).Count();
            FetchNeighbors(neighbors, y, maze[0].Length, maze.Length);
            int rCount = neighbors.AsQueryable().Where(c => c.isValid() && !maze[c.y][c.x].visited).Count();
            return rCount.CompareTo(lCount);
        }
    }

    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = new MazeCellRep[height][];
        for (int y = 0; y < height; y++)
        {
            maze[y] = new MazeCellRep[width];
            for (int x = 0; x < width; x++) maze[y][x] = new() { value = MazeCell.WesternWall | MazeCell.NorthernWall | MazeCell.EasternWall | MazeCell.SouthernWall };
        }

        MazeCellRep selRep;
        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        MazeCellCoords[] neighbors = new MazeCellCoords[4];

        maze[selCell.y][selCell.x].visited = true;

        PriorityQueue<MazeCellRep, MazeCellCoords> frontier = new(new MazeCellCoordsComparer(maze));
        frontier.Enqueue(maze[selCell.y][selCell.x], selCell);

        while (frontier.TryDequeue(out selRep, out selCell))
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsQueryable().Where(c => c.isValid() && !maze[c.y][c.x].visited);

            if (querry.Any())
            {
                var neighbor = querry.ElementAt(random.Next(0, querry.Count()));

                MergeCells(maze, selCell, neighbor);
                maze[neighbor.y][neighbor.x].visited = true;
                frontier.Enqueue(selRep, selCell);
                frontier.Enqueue(maze[neighbor.y][neighbor.x], neighbor);
            }
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsQueryable().Select(row => row.AsQueryable().Select(cell => cell.value).ToArray()).ToArray();
    }
}
