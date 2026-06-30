using Godot;

public partial class uwMeshInstance3D : MeshInstance3D
{

    public override void _Process(double delta)
    {
        base._Process(delta);
        LookAt(main.cameraPitchGimbal_world.GlobalTransform.Origin, useModelFront:true);
    }
}