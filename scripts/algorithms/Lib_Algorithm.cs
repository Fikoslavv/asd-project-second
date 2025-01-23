using System.Collections;
using Godot;

public static partial class Lib_Algorithm
{
    internal class MazeCellCoords
    {
        internal static readonly MazeCellCoords DEFAULT = new(0, 0);
        
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
            if (obj is null) return false;
            else if (obj is MazeCellCoords coords) return x == coords.x && y == coords.y;
            return false;
        }

        public override int GetHashCode() => System.HashCode.Combine(x, y);

        public override string ToString() => $"{nameof(MazeCellCoords)} => ({x}, {y})";

        public static bool operator ==(MazeCellCoords left, MazeCellCoords right) => left is not null && left.Equals(right);
        public static bool operator !=(MazeCellCoords left, MazeCellCoords right) => left is null || !left.Equals(right);
    }

    internal class MazeCellRep
    {
        public MazeCell value;

        public bool visited = false;
    }

    internal struct MazeAnimatedGenData
    {
        internal MazeCellRep[][] maze;
        internal MazeCellCoords lastSelCellCoords;
        internal bool? wasSelCellAdded;
        internal MazeCellCoords lastNeighborCoords;
        internal bool? wasNeighborCellAdded;
    }

    internal struct MazeAnimatedPathData
    {
        internal System.Collections.Generic.ICollection<MazeCellCoords> path;
        internal MazeCellCoords lastSelCellCoords;
        internal Vector2I mazeSize;
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

    internal static MazeCellCoords GetFirstUnvisitedCell(MazeCellRep[][] maze, int cachedX = 0, int cachedY = 0)
    {
        for (int y = cachedY; y < maze.Length; y++)
        {
            for (int x = cachedX; x < maze[y].Length; x++)
            {
                if (!maze[y][x].visited) return new(x, y);
            }
        }

        return new(-1, -1);
    }

    internal static bool AreCellsConnected(MazeCellRep[][] maze, MazeCellCoords from, MazeCellCoords to)
    {
        if (!from.isValid() || !to.isValid()) return false;
        else if (from == to) return true;
        else if (from.x == to.x)
        {
            if (from.y < to.y) return (maze[from.y][from.x].value & MazeCell.NorthernWall) == 0 && (maze[to.y][to.x].value & MazeCell.SouthernWall) == 0;
            else return (maze[from.y][from.x].value & MazeCell.SouthernWall) == 0 && (maze[to.y][to.x].value & MazeCell.NorthernWall) == 0;
        }
        else
        {
            if (from.x < to.x) return (maze[from.y][from.x].value & MazeCell.EasternWall) == 0 && (maze[to.y][to.x].value & MazeCell.WesternWall) == 0;
            else return (maze[from.y][from.x].value & MazeCell.WesternWall) == 0 && (maze[to.y][to.x].value & MazeCell.EasternWall) == 0;
        }
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

    internal static MazeCellRep[][] ConvertToMazeCellReps(MazeCell[][] maze)
    {
        MazeCellRep[][] output = new MazeCellRep[maze.Length][];

        for (int y = 0; y < output.Length; y++)
        {
            output[y] = new MazeCellRep[maze[y].Length];
            for (int x = 0; x < output[y].Length; x++) output[y][x] = new() { value = maze[y][x] };
        }

        return output;
    }

    internal static int GetCellsWallsCount(MazeCell cell)
    {
        int count = 0;
        var value = (int)cell;

        while (value > 0)
        {
            value >>= 1;
            count += value & 1;
        }

        return count;
    }
}
