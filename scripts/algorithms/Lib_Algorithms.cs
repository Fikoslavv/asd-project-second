using Godot;

public static partial class Lib_Algorithm
{
    internal class MazeCellCoords
    {
        internal int x;
        internal int y;

        internal MazeCellCoords()
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

        public override int GetHashCode() => System.HashCode.Combine(x, y);

        public override string ToString() => $"{nameof(MazeCellCoords)} => ({x}, {y})";

        public static bool operator ==(MazeCellCoords left, MazeCellCoords right) => left.Equals(right);
        public static bool operator !=(MazeCellCoords left, MazeCellCoords right) => !left.Equals(right);
    }

    internal class MazeCellRep
    {
        public MazeCell value;

        public bool visited = false;
    }

    internal static void FetchNeighbors(MazeCellCoords[] neighbors, MazeCellCoords selCell, int width, int height)
    {
        if (selCell.x > 0) neighbors[0] = new(selCell.x - 1, selCell.y);
        else neighbors[0] = new(-1, -1);

        if (selCell.y < height - 1) neighbors[1] = new(selCell.x, selCell.y + 1);
        else neighbors[1] = new(-1, -1);

        if (selCell.x < width - 1) neighbors[2] = new(selCell.x + 1, selCell.y);
        else neighbors[2] = new(-1, -1);

        if (selCell.y > 0) neighbors[3] = new(selCell.x, selCell.y - 1);
        else neighbors[3] = new(-1, -1);
    }

    internal static void MergeCells(MazeCellRep[][] maze, MazeCellCoords selCell, MazeCellCoords neighbor)
    {
        if (selCell == neighbor || !selCell.isValid() || !neighbor.isValid()) return;
        else if (selCell.x == neighbor.x)
        {
            if (selCell.y < neighbor.y)
            {
                maze[selCell.y][selCell.x].value &= ~MazeCell.NorthernWall;
                maze[neighbor.y][neighbor.x].value &= ~MazeCell.SouthernWall;
            }
            else
            {
                maze[selCell.y][selCell.x].value &= ~MazeCell.SouthernWall;
                maze[neighbor.y][neighbor.x].value &= ~MazeCell.NorthernWall;
            }
        }
        else
        {
            if (selCell.x < neighbor.x)
            {
                maze[selCell.y][selCell.x].value &= ~MazeCell.EasternWall;
                maze[neighbor.y][neighbor.x].value &= ~MazeCell.WesternWall;
            }
            else
            {
                maze[selCell.y][selCell.x].value &= ~MazeCell.WesternWall;
                maze[neighbor.y][neighbor.x].value &= ~MazeCell.EasternWall;
            }
        }
    }

    internal static void UnmergeCells(MazeCellRep[][] maze, MazeCellCoords selCell, MazeCellCoords neighbor)
    {
        if (selCell == neighbor || !selCell.isValid() || !neighbor.isValid()) return;
        else if (selCell.x == neighbor.x)
        {
            if (selCell.y < neighbor.y)
            {
                maze[selCell.y][selCell.x].value |= MazeCell.NorthernWall;
                maze[neighbor.y][neighbor.x].value |= MazeCell.SouthernWall;
            }
            else
            {
                maze[selCell.y][selCell.x].value |= MazeCell.SouthernWall;
                maze[neighbor.y][neighbor.x].value |= MazeCell.NorthernWall;
            }
        }
        else
        {
            if (selCell.x < neighbor.x)
            {
                maze[selCell.y][selCell.x].value |= MazeCell.EasternWall;
                maze[neighbor.y][neighbor.x].value |= MazeCell.WesternWall;
            }
            else
            {
                maze[selCell.y][selCell.x].value |= MazeCell.WesternWall;
                maze[neighbor.y][neighbor.x].value |= MazeCell.EasternWall;
            }
        }
    }

    internal static MazeCellCoords GetFirstUnvisitedCell(MazeCellRep[][] maze)
    {
        for (int y = 0; y < maze.Length; y++)
        {
            for (int x = 0; x < maze[y].Length; x++)
            {
                if (!maze[y][x].visited) return new(x, y);
            }
        }

        return new(-1, -1);
    }

    internal static MazeCellRep[][] GetMazeCellRepsWithAllWalls(int width, int height, MazeCell value = MazeCell.WesternWall | MazeCell.NorthernWall | MazeCell.EasternWall | MazeCell.SouthernWall)
    {
        MazeCellRep[][] maze = new MazeCellRep[height][];

        for (int y = 0; y < height; y++)
        {
            maze[y] = new MazeCellRep[width];
            for (int x = 0; x < width; x++) maze[y][x] = new() { value = value };
        }

        return maze;
    }
}
