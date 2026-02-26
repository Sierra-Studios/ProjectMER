namespace ProjectMER.Features.Enums;

/// <summary>
/// Type of camera (visually)
/// </summary>
public enum CameraType
{
    /// <summary>
    /// Camera used in Light Containment Zone
    /// </summary>
    Lcz = 0,
    
    /// <summary>
    /// Camera used in Heavy Containment Zone
    /// </summary>
    Hcz = 1,
    
    /// <summary>
    /// Camera used in Entrance Zone
    /// </summary>
    Ez = 2,
    
    /// <summary>
    /// Camera used in Entrance Zone with Arm.
    /// </summary>
    EzArm = 3,
    
    /// <summary>
    /// TODO: Wtf is Sz
    /// </summary>
    Sz = 4
}