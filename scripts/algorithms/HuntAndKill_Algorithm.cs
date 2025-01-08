using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

using static Lib_Algorithm;

public static partial class HuntAddKill_Algorithm
{
    internal static MazeCell[][] GenerateMaze(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);

        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        MazeCellCoords[] neighbors = new MazeCellCoords[4];

        maze[selCell.y][selCell.x].visited = true;
        while (maze.AsParallel().Select(row => row.AsEnumerable().Where(c => !c.visited).Any()).AsEnumerable().Where(r => r).Any())
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);
            if (!querry.Any())
            {
                querry = null;
                MazeCellCoords[] tempNeighbors = new MazeCellCoords[neighbors.Length];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (maze[y][x].visited)
                        {
                            selCell = new(x, y);
                            FetchNeighbors(tempNeighbors, selCell, width, height);

                            var tempQuerry = tempNeighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);
                            if (tempQuerry.Any())
                            {
                                querry = tempQuerry;
                                x = width;
                                y = height;
                            }
                        }
                    }
                }
            }

            if (querry != null)
            {
                var neighbor = querry.ElementAt(random.Next(0, querry.Count()));
                MergeCells(maze, selCell, neighbor);
                selCell = neighbor;

                maze[selCell.y][selCell.x].visited = true;
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

        yield return output;

        MazeCellCoords selCell = new(random.Next(0, width), random.Next(0, height));
        maze[selCell.y][selCell.x].visited = true;
        output.lastSelCellCoords = selCell;
        yield return output;

        while (maze.AsParallel().Select(row => row.AsEnumerable().Where(c => !c.visited).Any()).AsEnumerable().Where(r => r).Any())
        {
            FetchNeighbors(neighbors, selCell, width, height);

            var querry = neighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);
            if (!querry.Any())
            {
                querry = null;
                MazeCellCoords[] tempNeighbors = new MazeCellCoords[neighbors.Length];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (maze[y][x].visited)
                        {
                            selCell = new(x, y);
                            FetchNeighbors(tempNeighbors, selCell, width, height);

                            var tempQuerry = tempNeighbors.AsEnumerable().Where(c => c.isValid() && !maze[c.y][c.x].visited);
                            if (tempQuerry.Any())
                            {
                                querry = tempQuerry;
                                x = width;
                                y = height;
                            }
                        }
                    }
                }
            }

            if (querry != null)
            {
                var neighbor = querry.ElementAt(random.Next(0, querry.Count()));
                MergeCells(maze, selCell, neighbor);

                output.lastSelCellCoords = selCell;
                output.lastNeighborCoords = neighbor;
                yield return output;

                selCell = neighbor;
                maze[selCell.y][selCell.x].visited = true;
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
