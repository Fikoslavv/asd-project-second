using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AppManager : Node
{
    [Export] private Window generatorWindow;

    [Export] private PackedScene mazeCellRep;

    [Export] private Node2D mazeRepRoot;

    private Button btnGenerateMaze;
    private SpinBox sboxWidth;
    private SpinBox sboxHeight;
    private OptionButton obtnMazeAlgorithm;
    private LineEdit txtMazeSeed;
    private CheckButton cbtnUseRandomSeed;
    private Button btnFindPath;
    private Button btnClearPath;
    private OptionButton obtnPathAlgorithm;

    private IList<Node2D> mazeCellReps = new List<Node2D>();

    private MazeCell[][] maze;
    private Lib_Algorithm.MazeCellCoords[] path;

    public override void _Ready()
    {
        if (this.generatorWindow == null) throw new NullReferenceException($"Field \"{nameof(this.generatorWindow)}\" cannot be set to null !!!");

        this.generatorWindow.Show();

        this.btnGenerateMaze = this.generatorWindow.GetNode<Button>(this.generatorWindow.GetMeta("btn_generate_maze").As<NodePath>());
        this.btnGenerateMaze.Pressed += this.OnGenerateMazeBtn_Clicked;
        this.sboxWidth = this.generatorWindow.GetNode<SpinBox>(this.generatorWindow.GetMeta("sbox_maze_width").As<NodePath>());
        this.sboxHeight = this.generatorWindow.GetNode<SpinBox>(this.generatorWindow.GetMeta("sbox_maze_height").As<NodePath>());
        this.obtnMazeAlgorithm = this.generatorWindow.GetNode<OptionButton>(this.generatorWindow.GetMeta("obtn_maze_algorithm").As<NodePath>());
        this.txtMazeSeed = this.generatorWindow.GetNode<LineEdit>(this.generatorWindow.GetMeta("txt_maze_seed").As<NodePath>());
        this.cbtnUseRandomSeed = this.generatorWindow.GetNode<CheckButton>(this.generatorWindow.GetMeta("cbtn_use_random_seed").As<NodePath>());
        this.btnFindPath = this.generatorWindow.GetNode<Button>(this.generatorWindow.GetMeta("btn_find_path").As<NodePath>());
        this.btnFindPath.Pressed += this.OnFindPathBtn_Clicked;
        this.btnClearPath = this.generatorWindow.GetNode<Button>(this.generatorWindow.GetMeta("btn_clear_path").As<NodePath>());
        this.btnClearPath.Pressed += this.OnClearPathBtn_Clicked;
        this.obtnPathAlgorithm = this.generatorWindow.GetNode<OptionButton>(this.generatorWindow.GetMeta("obtn_path_algorithm").As<NodePath>());

        this.cbtnUseRandomSeed.Toggled += (state) =>
        {
            this.txtMazeSeed.Editable = !state;
        };

        if (this.mazeRepRoot == null)
        {
            this.mazeRepRoot = new Node2D();
            this.GetTree().Root.CallDeferred(Node.MethodName.AddChild, this.mazeRepRoot);
            this.mazeRepRoot.CallDeferred(Node.MethodName.SetName, "MazeRepRoot");
        }
    }

    private void OnGenerateMazeBtn_Clicked()
    {
        this.path = null;

        this.btnGenerateMaze.Disabled = true;
        Random random;
        if (this.cbtnUseRandomSeed.ButtonPressed)
        {
            int seed = Random.Shared.Next();
            random = new(seed);
            this.txtMazeSeed.Text = seed.ToString();
        }
        else
        {
            if (!int.TryParse(this.txtMazeSeed.Text, out int seed))
            {
                this.btnGenerateMaze.Disabled = false;
                return;
            }
            else random = new(seed);
        }

        string algorithm = this.obtnMazeAlgorithm.Text.ToString().ToLower();

        if (algorithm.Contains("growing tree"))
        {
            if (algorithm.Contains("dfs")) this.maze = Growing_Tree_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier.Last(), random);
            else if (algorithm.Contains("prim's")) this.maze = Growing_Tree_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier.AsQueryable().ElementAt(random.Next(0, frontier.Count)), random);
        }
        else if (algorithm.Contains("wilson's")) this.maze = Wilsons_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("hunt-and-kill")) this.maze = HuntAddKill_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("aldous-broder")) this.maze = Aldous_Broder_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("prim's")) this.maze = Prims_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("kruskal's"))
        {
            if (algorithm.Contains("looping")) this.maze = Kruskals_Algorithm.GenerateMazeLooping((int)sboxWidth.Value, (int)sboxHeight.Value, random);
            else this.maze = Kruskals_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        }
        else if (algorithm.Contains("depth-first search")) this.maze = Depth_First_Search_Algorithm.GenerateMaze((int)sboxWidth.Value, (int)sboxHeight.Value, random);

        foreach (var rep in this.mazeCellReps) rep.QueueFree();
        this.mazeCellReps.Clear();

        for (int y = this.maze.Length - 1; y >= 0; y--)
        {
            for (int x = this.maze[y].Length - 1; x >= 0; x--)
            {
                var mazeCellRep = this.mazeCellRep.Instantiate<Node2D>();
                var cellSize = mazeCellRep.GetMeta("cell_size").As<Vector2>();
                mazeCellRep.GlobalPosition = new Vector2((this.maze[y].Length * cellSize.X / -2) + (x * cellSize.X), (cellSize.Y * this.maze.Length / 2) - (y * cellSize.Y));

                Node2D wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_west").As<NodePath>());
                if ((this.maze[y][x] & MazeCell.WesternWall) != MazeCell.WesternWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_north").As<NodePath>());
                if ((this.maze[y][x] & MazeCell.NorthernWall) != MazeCell.NorthernWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_east").As<NodePath>());
                if ((this.maze[y][x] & MazeCell.EasternWall) != MazeCell.EasternWall) wall.QueueFree();

                wall = mazeCellRep.GetNode<Node2D>(mazeCellRep.GetMeta("wall_south").As<NodePath>());
                if ((this.maze[y][x] & MazeCell.SouthernWall) != MazeCell.SouthernWall) wall.QueueFree();

                mazeCellReps.Add(mazeCellRep);
                this.mazeRepRoot.CallDeferred(Node.MethodName.AddChild, mazeCellRep);
            }
        }

        this.btnGenerateMaze.Disabled = false;
    }

    private void OnClearPathBtn_Clicked()
    {
        if (this.maze is null || this.path is null) return;

        var tempCell = this.mazeCellRep.Instantiate();
        var cellSize = tempCell.GetMeta("cell_size").As<Vector2>();
        var floorColor = tempCell.GetMeta("color_floor_default").As<Color>();
        tempCell.CallDeferred(Node.MethodName.QueueFree);

        foreach (var coords in this.path)
        {
            var position = new Vector2((this.maze[coords.y].Length * cellSize.X / -2) + (coords.x * cellSize.X), (cellSize.Y * this.maze.Length / 2) - (coords.y * cellSize.Y));
            var cell = this.mazeCellReps.AsQueryable().Where(c => c.GlobalPosition == position).First();
            cell.GetNode<Polygon2D>(cell.GetMeta("floor").As<NodePath>()).Color = floorColor;
        }

        this.path = null;
    }

    private void OnFindPathBtn_Clicked()
    {
        if (this.maze is null) return;
        if (this.path is not null) this.OnClearPathBtn_Clicked();

        Lib_Algorithm.MazeCellCoords from;
        Lib_Algorithm.MazeCellCoords to;

        {
            int x = 0;
            MazeCell[] row = this.maze.Last();
            while (x < row.Length && (row[x] & MazeCell.NorthernWall) != 0) x++;
            from = new (x, this.maze.Length - 1);

            x = 0;
            row = this.maze[0];
            while (x < row.Length && (row[x] & MazeCell.SouthernWall) != 0) x++;
            to = new (x, 0);
        }

        string algorithm = this.obtnPathAlgorithm.Text.ToLower();

        if (algorithm.Contains("dijkstra's")) this.path = Dijstras_Algorithm.FindPath(this.maze, from, to);

        var tempCell = this.mazeCellRep.Instantiate();
        var cellSize = tempCell.GetMeta("cell_size").As<Vector2>();
        var pathColor = tempCell.GetMeta("color_floor_path").As<Color>();
        tempCell.CallDeferred(Node.MethodName.QueueFree);

        foreach (var coords in this.path)
        {
            var position = new Vector2((this.maze[coords.y].Length * cellSize.X / -2) + (coords.x * cellSize.X), (cellSize.Y * this.maze.Length / 2) - (coords.y * cellSize.Y));
            var cell = this.mazeCellReps.AsQueryable().Where(c => c.GlobalPosition == position).First();
            cell.GetNode<Polygon2D>(cell.GetMeta("floor").As<NodePath>()).Color = pathColor;
        }
    }
}
