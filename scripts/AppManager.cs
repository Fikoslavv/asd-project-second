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
    private CheckButton cbtnEnableAnimatedMazeBuild;
    private LineEdit txtMazeBuildAnimationSpeed;
    private CheckButton cbtnEnableAnimatedPathBuild;
    private LineEdit txtPathBuildAnimationSpeed;

    private MazeCell[][] maze;
    private ICollection<Lib_Algorithm.MazeCellCoords> path;

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
        this.cbtnEnableAnimatedMazeBuild = this.generatorWindow.GetNode<CheckButton>(this.generatorWindow.GetMeta("cbtn_enable_animated_maze_build").As<NodePath>());
        this.txtMazeBuildAnimationSpeed = this.generatorWindow.GetNode<LineEdit>(this.generatorWindow.GetMeta("txt_maze_build_animation_speed").As<NodePath>());
        this.cbtnEnableAnimatedPathBuild = this.generatorWindow.GetNode<CheckButton>(this.generatorWindow.GetMeta("cbtn_enable_animated_path_build").As<NodePath>());
        this.txtPathBuildAnimationSpeed = this.generatorWindow.GetNode<LineEdit>(this.generatorWindow.GetMeta("txt_path_build_animation_speed").As<NodePath>());

        this.cbtnUseRandomSeed.Toggled += (state) => this.txtMazeSeed.Editable = !state;
        this.cbtnEnableAnimatedMazeBuild.Toggled += (state) => this.txtMazeBuildAnimationSpeed.Editable = !state;
        this.cbtnEnableAnimatedPathBuild.Toggled += (state) => this.txtPathBuildAnimationSpeed.Editable = !state;

        if (this.mazeRepRoot == null)
        {
            this.mazeRepRoot = new Node2D();
            this.GetTree().Root.CallDeferred(Node.MethodName.AddChild, this.mazeRepRoot);
            this.mazeRepRoot.CallDeferred(Node.MethodName.SetName, "MazeRepRoot");
        }
    }

    private void OnGenerateMazeBtn_Clicked()
    {
        this.path?.Clear();
        this.path = null;
        this.maze = null;

        Random random;
        if (this.cbtnUseRandomSeed.ButtonPressed)
        {
            int seed = Random.Shared.Next();
            random = new(seed);
            this.txtMazeSeed.Text = seed.ToString();
        }
        else
        {
            if (!int.TryParse(this.txtMazeSeed.Text, out int seed)) return;
            else random = new(seed);
        }

        double? animationSpeed = null;
        if (!this.cbtnEnableAnimatedMazeBuild.ButtonPressed)
        {
            if (!int.TryParse(this.txtMazeBuildAnimationSpeed.Text, out var animSpeed)) return;

            animationSpeed = animSpeed * 0.001;
        }

        this.SetControlsDisabled(true);

        IEnumerator<Lib_Algorithm.MazeAnimatedGenData> generator = null;
        string algorithm = this.obtnMazeAlgorithm.Text.ToString().ToLower();

        if (algorithm.Contains("growing tree"))
        {
            if (algorithm.Contains("dfs")) generator = Growing_Tree_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier.Last(), random);
            else if (algorithm.Contains("prim's")) generator = Growing_Tree_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, (frontier, random) => frontier.AsQueryable().ElementAt(random.Next(0, frontier.Count)), random);
        }
        else if (algorithm.Contains("wilson's")) generator = Wilsons_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("hunt-and-kill")) generator = HuntAddKill_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("aldous-broder")) generator = Aldous_Broder_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("prim's")) generator = Prims_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        else if (algorithm.Contains("kruskal's"))
        {
            if (algorithm.Contains("looping")) generator = Kruskals_Algorithm.GetMazeLoopingGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
            else generator = Kruskals_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);
        }
        else if (algorithm.Contains("depth-first search")) generator = Depth_First_Search_Algorithm.GetMazeGenerator((int)sboxWidth.Value, (int)sboxHeight.Value, random);

        foreach (var rep in this.mazeRepRoot.GetChildren()) rep.QueueFree();

        var builder = new MazeBuilder() { MazeParent = this.mazeRepRoot, MazeGen = generator, MazeCellPrefab = this.mazeCellRep, TimeThreshold = animationSpeed };
        var parent = this.GetParent();
        parent.CallDeferred(Node.MethodName.AddChild, builder);

        builder.OnBuildCompleted += (maze) =>
        {
            this.SetControlsDisabledDeferred(false);
            this.maze = maze;
        };
    }

    private void OnClearPathBtn_Clicked()
    {
        if (this.maze is null || this.path is null) return;
        this.path.Clear();
        this.path = null;

        this.SetControlsDisabled(true);
        var children = this.mazeRepRoot.GetChildren();
        var task = System.Threading.Tasks.Task.Run(() => { foreach (MazeCellNode cell in children) cell.Floor.SetThreadSafe(Polygon2D.PropertyName.Color, cell.DefaultFloorColor); });
        task.GetAwaiter().OnCompleted(() => this.SetControlsDisabledDeferred(false));
    }

    private void OnFindPathBtn_Clicked()
    {
        if (this.maze is null) return;
        if (this.path is not null) this.OnClearPathBtn_Clicked();

        double? animationSpeed = null;
        if (!this.cbtnEnableAnimatedPathBuild.ButtonPressed)
        {
            if (!int.TryParse(this.txtPathBuildAnimationSpeed.Text, out int value))
            {
                return;
            }
            else animationSpeed = value * 0.001;
        }

        this.SetControlsDisabled(true);

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
        IEnumerator<Lib_Algorithm.MazeAnimatedPathData> pathBuilder = null;

        if (algorithm.Contains("dijkstra's")) pathBuilder = Dijstras_Algorithm.GetPathGenerator(this.maze, from, to);

        var generator = new PathBuilder() { MazeParent = this.mazeRepRoot, PathGen = pathBuilder, TimeThreshold = animationSpeed, FromToCoords = new(from, to) };
        this.GetParent().CallDeferred(Node.MethodName.AddChild, generator);

        generator.OnBuildCompleted += (path) =>
        {
            this.SetControlsDisabledDeferred(false);
            this.path = path;
        };
    }

    protected void SetControlsDisabled(bool value)
    {
        this.btnGenerateMaze.Disabled = value;
        this.btnClearPath.Disabled = value;
        this.btnFindPath.Disabled = value;
    }

    private void SetControlsDisabledDeferred(bool value)
    {
        this.btnGenerateMaze.SetDeferred(Button.PropertyName.Disabled, value);
        this.btnClearPath.SetDeferred(Button.PropertyName.Disabled, value);
        this.btnFindPath.SetDeferred(Button.PropertyName.Disabled, value);
    }
}
