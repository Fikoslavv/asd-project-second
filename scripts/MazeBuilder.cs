using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MazeBuilder : Node
{
    private double elapsedTime = double.MaxValue;
    private double? timeThreshold = 0.125;
    private IEnumerator<Lib_Algorithm.MazeAnimatedGenData> mazeGen;
    private PackedScene mazeCellPrefab;
    private Action<double> mazeBuilder;
    private Lib_Algorithm.MazeCellRep[][] maze;
    private Node mazeParent;
    private Godot.Collections.Array<Node> mazeCellPool;

    private Polygon2D selCellPoly;
    private Polygon2D neighborCellPoly;

    internal event Action<MazeCell[][]> OnBuildCompleted;

    internal IEnumerator<Lib_Algorithm.MazeAnimatedGenData> MazeGen { get => mazeGen; init => mazeGen = value; }
    internal Node MazeParent { get => mazeParent; init => mazeParent = value; }
    internal PackedScene MazeCellPrefab { get => mazeCellPrefab; init => mazeCellPrefab = value; }
    public double? TimeThreshold { get => timeThreshold; init => timeThreshold = value; }

    protected System.Threading.Tasks.Task asyncTask = null;

    public override void _Ready()
    {
        this.MazeGen.MoveNext();
        this.maze = this.MazeGen.Current.maze;
        Vector2I mazeSize = new(this.maze[0].Length, this.maze.Length);
        this.asyncTask = System.Threading.Tasks.Task.Run(() => this.Task_Create_MazeCell_Pool(mazeSize, this.MazeCellPrefab, this.MazeParent));
        this.asyncTask.GetAwaiter().OnCompleted(() => this.mazeBuilder = this.BuildMaze_PoolReady);
    }

    public override void _Process(double delta)
    {
        if (this.mazeBuilder is null) return;
        lock (this.mazeBuilder) this.mazeBuilder?.Invoke(delta);
    }

    private void BuildMaze_PoolReady(double delta)
    {
        this.mazeCellPool = this.MazeParent.GetChildren();
        var cellSize = (this.mazeCellPool[0] as MazeCellNode).CellSize;

        if (this.TimeThreshold is null)
        {
            this.asyncTask = System.Threading.Tasks.Task.Run(this.BuildMaze_Instant);
            this.asyncTask.GetAwaiter().OnCompleted(() => { lock (this.mazeBuilder) this.mazeBuilder = BuildMaze_Completed; });
        }
        else
        {
            this.mazeBuilder = this.BuildMaze_Animated;

            this.selCellPoly = new()
            {
                Polygon = new Vector2[4] { new(cellSize.X / -2, cellSize.Y / -2), new(cellSize.X / 2, -cellSize.Y / 2), new(cellSize.X / 2, cellSize.Y / 2), new(-cellSize.X / 2, cellSize.Y / 2) },
                Color = new("992331"),
                Scale = new(0.85f, 0.85f)
            };
            this.CallDeferred(MethodName.AddChild, this.selCellPoly);

            this.neighborCellPoly = new()
            {
                Polygon = new Vector2[4] { new(cellSize.X / -2, cellSize.Y / -2), new(cellSize.X / 2, -cellSize.Y / 2), new(cellSize.X / 2, cellSize.Y / 2), new(-cellSize.X / 2, cellSize.Y / 2) },
                Color = new("0f5e7c"),
                Scale = new(0.85f, 0.85f)
            };
            this.CallDeferred(MethodName.AddChild, this.neighborCellPoly);
        }
    }

    private void BuildMaze_Instant()
    {
        var mazeSize = new Vector2I(this.maze[0].Length, this.maze.Length);
        int i = 0;

        while (this.MazeGen.MoveNext()) ;

        foreach (MazeCellNode cell in this.mazeCellPool)
        {
            this.RefreshWallsInTheCell(cell, this.maze[i / mazeSize.Y][i % mazeSize.Y].value);
            cell.SetThreadSafe(MazeCellNode.PropertyName.Visible, true);
            i++;
        }

        this.mazeBuilder = this.BuildMaze_Completed;
    }

    private void BuildMaze_Instant(double delta) => this.BuildMaze_Instant();

    private void BuildMaze_Completed(double delta)
    {
        this.OnBuildCompleted?.Invoke(this.maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray());
        this.mazeBuilder = (_) => this.QueueFree();
    }

    private void BuildMaze_Animated(double delta)
    {
        this.elapsedTime += delta;
        
        if (this.elapsedTime < this.TimeThreshold) return;
        else this.elapsedTime = 0;

        if (!this.BuildMaze_Step()) this.mazeBuilder = this.BuildMaze_Completed;
    }

    private bool BuildMaze_Step()
    {
        if (!mazeGen.MoveNext()) return false;

        var data = mazeGen.Current;
        this.maze = data.maze;

        this.ProcessMazeCell(data.maze, data.lastSelCellCoords, data.wasSelCellAdded, this.selCellPoly);
        this.ProcessMazeCell(data.maze, data.lastNeighborCoords, data.wasNeighborCellAdded, this.neighborCellPoly);
        return true;
    }

    private void ProcessMazeCell(Lib_Algorithm.MazeCellRep[][] maze, Lib_Algorithm.MazeCellCoords cellCoords, bool? wasAdded, Polygon2D pointer)
    {
        if (cellCoords is null) return;

        MazeCellNode cell = this.mazeCellPool[maze[cellCoords.y].Length * cellCoords.y + cellCoords.x] as MazeCellNode;

        if (wasAdded is null)
        {
            pointer.Visible = false;
            cell.Visible = true;
            return;
        }

        if ((bool)wasAdded)
        {
            this.RefreshWallsInTheCell(cell, maze[cellCoords.y][cellCoords.x].value);
            pointer.GlobalPosition = new Vector2((maze[cellCoords.y].Length * cell.CellSize.X / -2) + (cellCoords.x * cell.CellSize.X), (cell.CellSize.Y * maze.Length / 2) - (cellCoords.y * cell.CellSize.Y));
            pointer.Visible = true;
            cell.Visible = true;
        }
        else
        {
            pointer.Visible = false;
            cell.Visible = false;
        }
    }

    private void RefreshWallsInTheCell(MazeCellNode cell, MazeCell walls)
    {
        cell.WesternWall.SetThreadSafe(Polygon2D.PropertyName.Visible, (walls & MazeCell.WesternWall) != 0);
        cell.NorthernWall.SetThreadSafe(Polygon2D.PropertyName.Visible, (walls & MazeCell.NorthernWall) != 0);
        cell.EasternWall.SetThreadSafe(Polygon2D.PropertyName.Visible, (walls & MazeCell.EasternWall) != 0);
        cell.SouthernWall.SetThreadSafe(Polygon2D.PropertyName.Visible, (walls & MazeCell.SouthernWall) != 0);
    }

    protected void Task_Create_MazeCell_Pool(Vector2I mazeSize, PackedScene cellPrefab, Node cellsRoot)
    {
        for (int y = 0; y < mazeSize.Y; y++)
        {
            for (int x = 0; x < mazeSize.X; x++)
            {
                var cell = cellPrefab.Instantiate<MazeCellNode>();
                cell.Visible = false;
                cell.GlobalPosition = new((mazeSize.X * cell.CellSize.X / -2) + (x * cell.CellSize.X), (mazeSize.Y * cell.CellSize.Y / 2) - (y * cell.CellSize.Y));
                cellsRoot.CallThreadSafe(Node.MethodName.AddChild, cell);
            }
        }
    }
}
