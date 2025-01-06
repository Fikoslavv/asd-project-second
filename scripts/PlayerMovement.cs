using Godot;
using System;

public partial class PlayerMovement : Node
{
	[ExportGroup("Player Movement Settings")]
	[Export(PropertyHint.Range, "0,1000,0.2,or_greater")]
	private double movementSpeed;

	[ExportGroup("Camera Movement Settings")]
	[Export(PropertyHint.Range, "0,1000,0.2,or_greater")]
	private double zoomSpeed;

	[Export(PropertyHint.Range, "0,100,0.01, or_greater, or_lower")]
	private float zoomMin;

	[Export(PropertyHint.Range, "0,100,0.01, or_greater, or_lower")]
	private float zoomMax;

	[Export(PropertyHint.NodeType)]
	private Node2D playerRoot;

	[Export(PropertyHint.NodeType)]
	private Camera2D camera;

	[ExportGroup("Debug")]
	[Export]
	private bool printDebugMessages = false;

	public override void _Process(double delta)
	{
		this.PerformPlayerMovement(delta);
		this.PerformCameraMovement(delta);
	}

	private void PerformCameraMovement(double delta)
	{
		var zoomDirection = Input.GetAxis("movement_zoom_in", "movement_zoom_out");

		var zoomVector = new Vector2(zoomDirection, zoomDirection);
		zoomVector *= (float)(this.zoomSpeed * delta);

		this.GDPrint($"Zoom vector => {zoomVector}");

		zoomVector += this.camera.Zoom;

		zoomVector.X = Mathf.Clamp(zoomVector.X, this.zoomMax, this.zoomMin);
		zoomVector.Y = Mathf.Clamp(zoomVector.Y, this.zoomMax, this.zoomMin);

		this.camera.Zoom = zoomVector;
	}

	private void PerformPlayerMovement(double delta)
	{
		var horizontalDirection = Input.GetAxis("movement_left", "movement_right");
		var verticalDirection = Input.GetAxis("movement_up", "movement_down");

		var movement = new Vector2(horizontalDirection, verticalDirection);
		movement *= (float)(this.movementSpeed * delta);

		this.GDPrint($"Movement vector => {movement}");

		this.playerRoot.Position += movement;
	}

	private void GDPrint(string message)
	{
		if (this.printDebugMessages) GD.Print(message);
	}
}
