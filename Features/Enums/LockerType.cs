namespace ProjectMER.Features.Enums;

/// <summary>
/// Type of locker.
/// </summary>
public enum LockerType
{
	/// <summary>
	/// None value used if unknown
	/// </summary>
	None = -1,
	
	/// <summary>
	/// Pedestal Locker with label SCP-500
	/// </summary>
	PedestalScp500 = 0,
	
	/// <summary>
	/// Locker for Large Gun
	/// </summary>
	LargeGun = 1,
	
	/// <summary>
	/// Locker containing Epsilon firearm.
	/// </summary>
	RifleRack = 2,
	
	/// <summary>
	/// Double door locker used in Gr18 etc.
	/// </summary>
	Misc = 3,
	
	/// <summary>
	/// Small locker with red sign containing Medkit
	/// </summary>
	Medkit = 4,
	
	/// <summary>
	/// Small locker with blue sign containing Adrenaline
	/// </summary>
	Adrenaline = 5,
	
	/// <summary>
	/// Pedestal Locker with label SCP-018
	/// </summary>
	PedestalScp018 = 6,
	
	/// <summary>
	/// Pedestal Locker with label SCP-207
	/// </summary>
	PedestalScp207 = 7,
	
	/// <summary>
	/// Pedestal Locker with label SCP-244
	/// </summary>
	PedestalScp244 = 8,
	
	/// <summary>
	/// Pedestal Locker with label SCP-268
	/// </summary>
	PedestalScp268 = 9,
	
	/// <summary>
	/// Pedestal Locker with label SCP-1853
	/// </summary>
	PedestalScp1853 = 10,
	
	/// <summary>
	/// Pedestal Locker with label SCP-2176
	/// </summary>
	PedestalScp2176 = 11,
	
	/// <summary>
	/// Pedestal Locker with label SCP-1576
	/// </summary>
	PedestalScpScp1576 = 12,
	
	/// <summary>
	/// Pedestal Locker with label SCP-207
	/// </summary>
	PedestalAntiScp207 = 13,
	
	/// <summary>
	/// Pedestal Locker with label SCP-1344
	/// </summary>
	PedestalScp1344 = 14,
	
	/// <summary>
	/// Locker for Experimental Weapon, which is Jailbird & Particle Disruptor.
	/// </summary>
	ExperimentalWeapon = 15,
}
