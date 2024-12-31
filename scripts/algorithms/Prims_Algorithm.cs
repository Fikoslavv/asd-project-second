using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class Prims_Algorithm
{
    internal class MazeCellCoords
    {
        internal int x;
        internal int y;

        public MazeCellCoords()
        {
            this.x = 0;
            this.y = 0;
        }

        internal MazeCellCoords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        internal MazeCellCoords(MazeCellCoords coords)
        {
            x = coords.x;
            y = coords.y;
        }

        public bool isValid() => x >= 0 && y >= 0;

        public override bool Equals(object obj)
        {
            if (obj is MazeCellCoords coords) return x == coords.x && y == coords.y;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"{nameof(MazeCellCoords)} => ({x}, {y})";
        }
    }

    private class MazeCellRep
    {
        public MazeCell value;

        public bool visited = false;
    }

    internal delegate T SelectFrontier<T>(IList<T> frontier, Random random);

    internal static MazeCell[][] GenerateMaze(int width, int height)
    {
        return GenerateMaze(width, height, (frontier, random) => frontier.Last());
    }

    internal static MazeCell[][] GenerateMaze(int width, int height, SelectFrontier<MazeCellCoords> selectFrontier)
    {
        Random random = new();
        var maze = new MazeCellRep[height][];

        for (int y = height - 1; y >= 0; y--)
        {
            maze[y] = new MazeCellRep[width];
            for (int x = width - 1; x >= 0; x--) maze[y][x] = new() { value = MazeCell.WesternWall | MazeCell.NorthernWall | MazeCell.EasternWall | MazeCell.SouthernWall};
        }

        IList<MazeCellCoords> frontier = new List<MazeCellCoords>() { new(random.Next(0, width), height - 1) };
        var selCell = frontier.First();

        maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
        maze[selCell.y][selCell.x].visited = true;

        MazeCellCoords[] neighbors = new MazeCellCoords[4];
        while (frontier.Count > 0)
        {
            selCell = selectFrontier(frontier, random);
            // GD.Print($"Selected Cell: {selCell}");

            {
                if (selCell.x > 0) neighbors[0] = new MazeCellCoords(selCell.x - 1, selCell.y);
                else neighbors[0] = new MazeCellCoords(-1, -1);

                if (selCell.y < height - 1) neighbors[1] = new MazeCellCoords(selCell.x, selCell.y + 1);
                else neighbors[1] = new MazeCellCoords(-1, -1);

                if (selCell.x < width - 1) neighbors[2] = new MazeCellCoords(selCell.x + 1, selCell.y);
                else neighbors[2] = new MazeCellCoords(-1, -1);

                if (selCell.y > 0) neighbors[3] = new MazeCellCoords(selCell.x, selCell.y - 1);
                else neighbors[3] = new MazeCellCoords(-1, -1);
            }

            {
                bool allVisited = true;
                foreach (var neighbor in neighbors) if (neighbor.isValid()) allVisited &= maze[neighbor.y][neighbor.x].visited;
                if (allVisited)
                {
                    frontier.Remove(selCell);
                    // GD.Print($"Removed Cell: {selCell}");
                    continue;
                }
            }

            MazeCellCoords selNeighbor;
            {
                var selNeighborQuerry = neighbors.AsQueryable().Where(n => n.isValid() && !maze[n.y][n.x].visited);
                selNeighbor = selNeighborQuerry.ElementAt((int)(random.NextDouble() * selNeighborQuerry.Count()));
            }
            // GD.Print($"Selected Neighbor: {selNeighbor}");

            switch (neighbors.ToList().IndexOf(selNeighbor))
            {
                case 0:
                {
                    maze[selCell.y][selCell.x].value &= ~MazeCell.WesternWall;
                    maze[selNeighbor.y][selNeighbor.x].value &= ~MazeCell.EasternWall;
                }
                break;

                case 1:
                {
                    maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
                    maze[selNeighbor.y][selNeighbor.x].value &= ~MazeCell.SouthernWall;
                }
                break;

                case 2:
                {
                    maze[selCell.y][selCell.x].value &= ~MazeCell.EasternWall;
                    maze[selNeighbor.y][selNeighbor.x].value &= ~MazeCell.WesternWall;
                }
                break;

                case 3:
                {
                    maze[selCell.y][selCell.x].value &= ~MazeCell.SouthernWall;
                    maze[selNeighbor.y][selNeighbor.x].value &= ~MazeCell.NorthernWall;
                }
                break;
            }

            maze[selNeighbor.y][selNeighbor.x].visited = true;
            frontier.Add(selNeighbor);
            // GD.Print($"Added Cell: {selNeighbor}");
        }

        maze[0][random.Next(0, width)].value &= ~MazeCell.SouthernWall;

        return maze.AsQueryable().Select(row => row.AsQueryable().Select(cell => cell.value).ToArray()).ToArray();
    }
}
