using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using static Lib_Algorithm;

public static partial class Growing_Tree_Algorithm
{
    internal delegate T SelectFrontier<T>(HashSet<T> frontier, Random random);

    internal static MazeCell[][] GenerateMaze(int width, int height, SelectFrontier<MazeCellCoords> selectFrontier, Random random = default)
    {
        var maze = GetMazeCellRepsWithAllWalls(width, height);

        HashSet<MazeCellCoords> frontier = new() { new(random.Next(0, width), random.Next(0, height)) };
        var selCell = frontier.First();

        maze[selCell.y][selCell.x].visited = true;

        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        while (frontier.Count > 0)
        {
            selCell = selectFrontier(frontier, random);

            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(n => n.isValid() && !maze[n.y][n.x].visited);

            if (querry.Any())
            {
                MazeCellCoords selNeighbor;
                selNeighbor = querry.ElementAt((int)(random.NextDouble() * querry.Count()));

                MergeCells(maze, selCell, selNeighbor);

                maze[selNeighbor.y][selNeighbor.x].visited = true;
                frontier.Add(selNeighbor);
            }
            else frontier.Remove(selCell);
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;
        maze[height - 1][random.Next(0, width)].value &= ~MazeCell.NorthernWall;

        return maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray();
    }

    internal static IEnumerator<MazeAnimatedGenData> GetMazeGenerator(int width, int height, SelectFrontier<MazeCellCoords> selectFrontier, Random random = default)
    {
        var maze = GetMazeCellRepsWithAllWalls(width, height);
        var output = new MazeAnimatedGenData() { maze = maze, wasSelCellAdded = true, wasNeighborCellAdded = true };
        MazeCellCoords selCell;

        yield return output;

        HashSet<MazeCellCoords> frontier = new() { new(random.Next(0, width), random.Next(0, height)) };
        selCell = frontier.First();

        maze[selCell.y][selCell.x].visited = true;

        output.lastSelCellCoords = selCell;
        yield return output;

        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        while (frontier.Count > 0)
        {
            selCell = selectFrontier(frontier, random);

            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(n => n.isValid() && !maze[n.y][n.x].visited);

            if (querry.Any())
            {
                MazeCellCoords selNeighbor;
                selNeighbor = querry.ElementAt((int)(random.NextDouble() * querry.Count()));

                MergeCells(maze, selCell, selNeighbor);

                maze[selNeighbor.y][selNeighbor.x].visited = true;
                frontier.Add(selNeighbor);

                output.lastSelCellCoords = selCell;
                output.lastNeighborCoords = selNeighbor;
                yield return output;
            }
            else frontier.Remove(selCell);
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
