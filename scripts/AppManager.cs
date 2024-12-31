using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AppManager : Node
{
    [Export] private Window generatorWindow;

    [Export] private PackedScene mazeCellRep;

    [Export] private Node2D mazeRepRoot;

    private SpinBox sboxWidth;
    private SpinBox sboxHeight;
    private OptionButton obtnAlgorithm;

    private IList<Node2D> mazeCellReps = new List<Node2D>();

    public override void _Ready()
    {
        if (this.generatorWindow == null) throw new NullReferenceException($"Field \"{nameof(this.generatorWindow)}\" cannot be set to null !!!");

        this.generatorWindow.Show();

        this.generatorWindow.GetNode<Button>(this.generatorWindow.GetMeta("btn_generate_maze").As<NodePath>()).Pressed += this.OnGenerateMazeBtn_Clicked;
        this.sboxWidth = this.generatorWindow.GetNode<SpinBox>(this.generatorWindow.GetMeta("sbox_maze_width").As<NodePath>());
        this.sboxHeight = this.generatorWindow.GetNode<SpinBox>(this.generatorWindow.GetMeta("sbox_maze_height").As<NodePath>());
        this.obtnAlgorithm = this.generatorWindow.GetNode<OptionButton>(this.generatorWindow.GetMeta("obtn_maze_algorithm").As<NodePath>());

        if (this.mazeRepRoot == null)
        {
            this.mazeRepRoot = new Node2D();
            this.GetTree().Root.CallDeferred(Node.MethodName.AddChild, this.mazeRepRoot);
            this.mazeRepRoot.CallDeferred(Node.MethodName.SetName, "MazeRepRoot");
        }
    }

    private void OnGenerateMazeBtn_Clicked()
    {
        MazeCell[][] maze = null;
        string algorithm = this.obtnAlgorithm.Text.ToString().ToLower();

        if (algorithm.Contains("prim's"))
        {
            if (algorithm.Contains("last")) maze = Prims_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier.Last());
            else if (algorithm.Contains("random")) maze = Prims_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier[(int)Math.Clamp(random.NextDouble() * 1.2 * frontier.Count, 0, frontier.Count - 1)]);
        }

        foreach (var rep in this.mazeCellReps) rep.QueueFree();
        this.mazeCellReps.Clear();

        for (int y = maze.Length - 1; y >= 0; y--)
        {
            for (int x = maze[y].Length - 1; x >= 0; x--)
            {
                var mazeCellRep = this.mazeCellRep.Instantiate<Node2D>();
                var cellSize = mazeCellRep.GetMeta("cell_size").As<Vector2>();
                mazeCellRep.Position = new Vector2((maze[y].Length * cellSize.X / -2) + (x * cellSize.X), (cellSize.Y * maze.Length / 2) - (y * cellSize.Y));

                Node2D wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_west").As<NodePath>());
                if ((maze[y][x] & MazeCell.WesternWall) != MazeCell.WesternWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_north").As<NodePath>());
                if ((maze[y][x] & MazeCell.NorthernWall) != MazeCell.NorthernWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_east").As<NodePath>());
                if ((maze[y][x] & MazeCell.EasternWall) != MazeCell.EasternWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_south").As<NodePath>());
                if ((maze[y][x] & MazeCell.SouthernWall) != MazeCell.SouthernWall) wall.QueueFree();

                mazeCellReps.Add(mazeCellRep);
                this.mazeRepRoot.CallDeferred(Node.MethodName.AddChild, mazeCellRep);
            }
        }
    }
}
