using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class MazeBuilder : Node
{
    private double elapsedTime = double.MaxValue;
    private double? timeThreshold = 0.125;
    private IEnumerator<Lib_Algorithm.MazeAnimatedGenData> mazeGen;
    private Node mazeParent;
    private PackedScene mazeCellPrefab;
    private Action<double> mazeBuilder;
    private Lib_Algorithm.MazeCellRep[][] maze;

    internal event Action<MazeCell[][]> OnBuildCompleted;

    internal IEnumerator<Lib_Algorithm.MazeAnimatedGenData> MazeGen { get => mazeGen; init => mazeGen = value; }
    internal Node MazeParent { get => mazeParent; init => mazeParent = value; }
    internal PackedScene MazeCellPrefab { get => mazeCellPrefab; init => mazeCellPrefab = value; }
    internal Vector2 MazeCellSize { get; init; } = new(-1, -1);
    public double? TimeThreshold { get => timeThreshold; init => timeThreshold = value; }

    public override void _Ready()
    {
        this.mazeBuilder = this.TimeThreshold == null ? this.BuildMaze_Instant : this.BuildMaze_Animated;
    }

    public override void _Process(double delta) => this.mazeBuilder.Invoke(delta);

    private void BuildMaze_Instant(double delta)
    {
        bool building = true;
        this.OnBuildCompleted += (maze) => building = false;

        while (building) this.BuildMaze_Step();
    }

    private void BuildMaze_Animated(double delta)
    {
        this.elapsedTime += delta;
        
        if (this.elapsedTime <= this.TimeThreshold) return;
        else this.elapsedTime = 0;

        this.BuildMaze_Step();
    }

    private void BuildMaze_Step()
    {
        if (mazeGen.MoveNext())
        {
            var data = mazeGen.Current;
            this.maze = data.maze;

            this.ProcessMazeCell(data.maze, data.lastSelCellCoords, data.wasSelCellAdded);
            this.ProcessMazeCell(data.maze, data.lastNeighborCoords, data.wasNeighborCellAdded);
        }
        else
        {
            this.OnBuildCompleted?.Invoke(this.maze.AsParallel().Select(r => r.AsEnumerable().Select(c => c.value).ToArray()).ToArray());
            this.QueueFree();
        }
    }

    private void ProcessMazeCell(Lib_Algorithm.MazeCellRep[][] maze, Lib_Algorithm.MazeCellCoords cellCoords, bool wasAdded)
    {
        if (cellCoords is null) return;

        Node2D cell;
        var cellPosition = new Vector2((maze[cellCoords.y].Length * MazeCellSize.X / -2) + (cellCoords.x * MazeCellSize.X), (MazeCellSize.Y * maze.Length / 2) - (cellCoords.y * MazeCellSize.Y));
        var querry = this.MazeParent.GetChildren().Where(n => n is Node2D c && c.GlobalPosition == cellPosition);

        if (querry.Any()) cell = querry.FirstOrDefault() as Node2D;
        else
        {
            cell = this.MazeCellPrefab.Instantiate<Node2D>();
            cell.GlobalPosition = cellPosition;
            this.mazeParent.AddChild(cell);
        }

        if (cell is null) return;
        else if (wasAdded) this.RefreshWallsInTheCell(cell, maze[cellCoords.y][cellCoords.x].value);
        else cell.Free();
    }

    private void RefreshWallsInTheCell(Node2D cell, MazeCell walls)
    {
        if ((walls & MazeCell.WesternWall) == 0) this.QueueFreeMazeCellsWall(cell, "wall_west");
        if ((walls & MazeCell.NorthernWall) == 0) this.QueueFreeMazeCellsWall(cell, "wall_north");
        if ((walls & MazeCell.EasternWall) == 0) this.QueueFreeMazeCellsWall(cell, "wall_east");
        if ((walls & MazeCell.SouthernWall) == 0) this.QueueFreeMazeCellsWall(cell, "wall_south");
    }

    private void QueueFreeMazeCellsWall(Node2D cell, string wall)
    {
        if (!cell.HasMeta(wall)) return;

        cell.GetNode<Node2D>(cell.GetMeta(wall).As<NodePath>()).QueueFree();
        cell.RemoveMeta(wall);
    }
}
