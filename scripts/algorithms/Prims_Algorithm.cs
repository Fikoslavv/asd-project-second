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
            int lCount = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited).Count();
            FetchNeighbors(neighbors, y, maze[0].Length, maze.Length);
            int rCount = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited).Count();
            return rCount.CompareTo(lCount);
        }
    }

    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        MazeCellRep selRep;
        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        MazeCellCoords[] neighbors = new MazeCellCoords[4];

        maze[selCell.y][selCell.x].visited = true;

        PriorityQueue<MazeCellRep, MazeCellCoords> frontier = new(new MazeCellCoordsComparer(maze));
        frontier.Enqueue(maze[selCell.y][selCell.x], selCell);

        while (frontier.TryDequeue(out selRep, out selCell))
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);

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

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }

    internal static IEnumerator<MazeAnimatedGenData> GetMazeGenerator(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);
        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        MazeAnimatedGenData output = new() { maze = maze, wasSelCellAdded = true, wasNeighborCellAdded = true };
        MazeCellRep selRep;

        yield return output;

        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        maze[selCell.y][selCell.x].visited = true;
        output.lastSelCellCoords = selCell;
        yield return output;

        PriorityQueue<MazeCellRep, MazeCellCoords> frontier = new(new MazeCellCoordsComparer(maze));
        frontier.Enqueue(maze[selCell.y][selCell.x], selCell);

        while (frontier.TryDequeue(out selRep, out selCell))
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);

            if (querry.Any())
            {
                var neighbor = querry.ElementAt(random.Next(0, querry.Count()));

                MergeCells(maze, selCell, neighbor);
                output.lastSelCellCoords = selCell;
                output.lastNeighborCoords = neighbor;
                yield return output;

                maze[neighbor.y][neighbor.x].visited = true;
                frontier.Enqueue(selRep, selCell);
                frontier.Enqueue(maze[neighbor.y][neighbor.x], neighbor);
            }
        }

        selCell = new(random.Next(0, width), 0);
        maze[selCell.y][selCell.x].value &= ~MazeCell.SouthernWall;
        output.lastSelCellCoords = selCell;

        selCell = new(random.Next(0, width), height - 1);
        maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
        output.lastNeighborCoords = selCell;

        yield return output;
    }
}
