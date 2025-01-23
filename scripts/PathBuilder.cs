using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PathBuilder : Node
{
    private double elapsedTime = double.MaxValue;
    private double? timeThreshold = 0.125;
    private IEnumerator<Lib_Algorithm.MazeAnimatedPathData> pathGen;
    private Action<double> onProcess;
    private Node mazeParent;
    private Godot.Collections.Array<Node> mazeCellPool;

    private Polygon2D selCellPoly;
    private Polygon2D neighborCellPoly;

    internal event Action<ICollection<Lib_Algorithm.MazeCellCoords>> OnBuildCompleted;

    internal IEnumerator<Lib_Algorithm.MazeAnimatedPathData> PathGen { get => pathGen; init => pathGen = value; }
    internal Node MazeParent { get => mazeParent; init => mazeParent = value; }
    public double? TimeThreshold { get => timeThreshold; init => timeThreshold = value; }
    internal KeyValuePair<Lib_Algorithm.MazeCellCoords, Lib_Algorithm.MazeCellCoords> FromToCoords { get; init; }

    protected System.Threading.Tasks.Task asyncTask = null;

    public override void _Ready()
    {
        this.PathGen.MoveNext();
        Vector2I mazeSize = this.PathGen.Current.mazeSize;
        this.onProcess = this.BuildPath_Initialized;
    }

    public override void _Process(double delta)
    {
        if (this.onProcess is null) return;
        lock (this.onProcess) this.onProcess?.Invoke(delta);
    }

    private void BuildPath_Initialized(double delta)
    {
        this.mazeCellPool = this.MazeParent.GetChildren();
        var cellSize = (this.mazeCellPool[0] as MazeCellNode).CellSize;

        if (this.TimeThreshold is null)
        {
            this.onProcess = (_) => { };
            this.asyncTask = System.Threading.Tasks.Task.Run(this.BuildPath_Instant);
            this.asyncTask.GetAwaiter().OnCompleted(() => { lock (this.onProcess) this.onProcess = BuildPath_Completed; });
        }
        else
        {
            this.onProcess = this.BuildPath_Animated;

            var mazeSize = this.PathGen.Current.mazeSize;

            this.selCellPoly = new()
            {
                Polygon = new Vector2[4] { new(cellSize.X / -2, cellSize.Y / -2), new(cellSize.X / 2, -cellSize.Y / 2), new(cellSize.X / 2, cellSize.Y / 2), new(-cellSize.X / 2, cellSize.Y / 2) },
                Color = new("992331"),
                Scale = new(0.85f, 0.85f),
                GlobalPosition = new((mazeSize.X * cellSize.X / -2) + (this.FromToCoords.Key.x * cellSize.X), (mazeSize.Y * cellSize.Y / 2) - (this.FromToCoords.Key.y * cellSize.Y))
            };
            this.CallDeferred(MethodName.AddChild, this.selCellPoly);

            this.neighborCellPoly = new()
            {
                Polygon = new Vector2[4] { new(cellSize.X / -2, cellSize.Y / -2), new(cellSize.X / 2, -cellSize.Y / 2), new(cellSize.X / 2, cellSize.Y / 2), new(-cellSize.X / 2, cellSize.Y / 2) },
                Color = new("0f5e7c"),
                Scale = new(0.85f, 0.85f),
                GlobalPosition = new((mazeSize.X * cellSize.X / -2) + (this.FromToCoords.Value.x * cellSize.X), (mazeSize.Y * cellSize.Y / 2) - (this.FromToCoords.Value.y * cellSize.Y))
            };
            this.CallDeferred(MethodName.AddChild, this.neighborCellPoly);
        }
    }

    private void BuildPath_Instant()
    {
        while (this.PathGen.MoveNext()) ;

        foreach (Lib_Algorithm.MazeCellCoords coords in this.PathGen.Current.path)
        {
            var cell = this.mazeCellPool[this.PathGen.Current.mazeSize.X * coords.y + coords.x] as MazeCellNode;
            cell.Floor.SetThreadSafe(Polygon2D.PropertyName.Color, cell.PathFloorColor);
        }

        this.onProcess = this.BuildPath_Completed;
    }

    private void BuildMaze_Instant(double delta) => this.BuildPath_Instant();

    private void BuildPath_Completed(double delta)
    {
        this.OnBuildCompleted?.Invoke(this.PathGen.Current.path);
        this.onProcess = (_) => this.QueueFree();
    }

    private void BuildPath_Animated(double delta)
    {
        this.elapsedTime += delta;
        
        if (this.elapsedTime < this.TimeThreshold) return;
        else this.elapsedTime = 0;

        if (!this.BuildMaze_Step())
        {
            this.onProcess = (_) => { };
            this.asyncTask = System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    foreach (var coord in this.PathGen.Current.path)
                    {
                        var cell = this.mazeCellPool[this.PathGen.Current.mazeSize.X * coord.y + coord.x] as MazeCellNode;
                        cell.Floor.SetThreadSafe(Polygon2D.PropertyName.Color, cell.PathFloorColor);
                    }
                }
            );
            this.asyncTask.GetAwaiter().OnCompleted(() => { lock (this.onProcess) this.onProcess = this.BuildPath_Completed; });
        }
    }

    private bool BuildMaze_Step()
    {
        if (!this.PathGen.MoveNext()) return false;

        var data = this.PathGen.Current;
        var cell = this.mazeCellPool[data.mazeSize.X * data.lastSelCellCoords.y + data.lastSelCellCoords.x] as MazeCellNode;

        cell.Floor.SetDeferred(Polygon2D.PropertyName.Color, cell.ShadedPathFloorColor);
        return true;
    }
}
