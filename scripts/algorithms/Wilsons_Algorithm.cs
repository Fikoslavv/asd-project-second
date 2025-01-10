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

        ICollection<MazeCellCoords> path = new List<MazeCellCoords>() { new(random.Next(0, width), random.Next(0, height)) };
        var selCell = path.Last();
        
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

    internal static IEnumerator<MazeAnimatedGenData> GetMazeGenerator(int width, int height, Random random = default)
    {
        MazeCellRep[][] maze = GetMazeCellRepsWithAllWalls(width, height);
        MazeCellCoords neighbor;
        MazeCellCoords selCell;
        MazeAnimatedGenData output = new() { maze = maze, wasSelCellAdded = true, wasNeighborCellAdded = true };

        yield return output;

        ICollection<MazeCellCoords> path = new List<MazeCellCoords>() { new(random.Next(0, width), random.Next(0, height)) };
        selCell = path.Last();

        {
            MazeCellCoords seedCell;
            do seedCell = new(random.Next(0, width), random.Next(0, height)); while (seedCell == selCell);
            maze[seedCell.y][seedCell.x].visited = true;
            output.lastSelCellCoords = seedCell;
            yield return output;
        }

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
                        output.lastSelCellCoords = selCell;
                        output.wasSelCellAdded = false;
                        output.lastNeighborCoords = blacklistedCell;
                        output.wasNeighborCellAdded = false;
                        yield return output;
                    }
                    while (!querry.Any() && path.Any());

                    if (!path.Any())
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
            output.lastSelCellCoords = selCell;
            output.wasSelCellAdded = true;
            output.lastNeighborCoords = neighbor;
            output.wasNeighborCellAdded = true;
            yield return output;

            if (maze[neighbor.y][neighbor.x].visited)
            {
                foreach (var cell in path) maze[cell.y][cell.x].visited = true;
                path.Clear();
                blacklistedCells.Clear();
                
                path.Add(GetFirstUnvisitedCell(maze));
            }
            else path.Add(neighbor);
        }

        selCell = new(random.Next(0, width), 0);
        maze[selCell.y][selCell.x].value &= ~MazeCell.SouthernWall;
        output.lastSelCellCoords = selCell;
        output.wasSelCellAdded = true;

        selCell = new(random.Next(0, width), height - 1);
        maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
        output.lastNeighborCoords = selCell;
        output.wasNeighborCellAdded = true;
        yield return output;
    }
}
