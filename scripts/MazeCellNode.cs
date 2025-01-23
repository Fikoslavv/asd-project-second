using Godot;
using System;

public partial class MazeCellNode : Node2D
{
    [Export]
    private Polygon2D floor;

    [Export]
    private Polygon2D westernWall;

    [Export]
    private Polygon2D northernWall;

    [Export]
    private Polygon2D easternWall;

    [Export]
    private Polygon2D southernWall;

    [Export]
    private Vector2 cellSize;

    [Export]
    private Color defaultFloorColor;

    [Export]
    private Color pathFloorColor;

    [Export]
    private Color shadedPathFloorColor;

    public Polygon2D Floor { get => floor; }
    public Polygon2D WesternWall { get => westernWall; }
    public Polygon2D NorthernWall { get => northernWall; }
    public Polygon2D EasternWall { get => easternWall; }
    public Polygon2D SouthernWall { get => southernWall; }
    public Vector2 CellSize { get => cellSize; }
    public Color DefaultFloorColor { get => defaultFloorColor; }
    public Color PathFloorColor { get => pathFloorColor; }
    public Color ShadedPathFloorColor { get => shadedPathFloorColor; }
}
