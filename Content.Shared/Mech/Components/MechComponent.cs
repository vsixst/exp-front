using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mech.Components;

/// <summary>
/// A large, pilotable machine that has equipment that is
/// powered via an internal battery.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechComponent : Component
{
    /// <summary>
    /// Forge-Change: Whether or not an emag disables it.
    /// </summary>
    [DataField("breakOnEmag")]
    [AutoNetworkedField]
    public bool BreakOnEmag = true;

    /// <summary>
    /// Forge-Change:
    /// A multiplier used to calculate how much of the damage done to a mech
    /// is transfered to the pilot
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MechEnergyWaste = 10;

    /// <summary>
    /// Forge-Change: is the mech lights are toggled?
    /// </summary>
    [DataField("light")]
    [AutoNetworkedField]
    public bool Light = false;

    /// <summary>
    /// Forge-Change: is the mech internals enabled?
    /// </summary>
    [DataField("internals")]
    [AutoNetworkedField]
    public bool Internals = false;

    /// <summary>
    /// Forge-Change: A whitelist for inserting equipment items.
    /// </summary>
    [DataField]
    public EntityWhitelist? BatteryWhitelist;
    
    [DataField]
    public EntityWhitelist? GasTankWhitelist;

    /// <summary>
    /// How much "health" the mech has left.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 Integrity;

    /// <summary>
    /// The maximum amount of damage the mech can take.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 MaxIntegrity = 250;

    /// <summary>
    /// How much energy the mech has.
    /// Derived from the currently inserted battery.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 Energy = 0;

    /// <summary>
    /// The maximum amount of energy the mech can have.
    /// Derived from the currently inserted battery.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 MaxEnergy = 0;

    /// <summary>
    /// The slot the battery is stored in.
    /// </summary>
    [ViewVariables]
    public ContainerSlot BatterySlot = default!;

    [ViewVariables]
    public readonly string BatterySlotId = "mech-battery-slot";

    /// <summary>
    /// Forge-Change: The slot the gas tank is stored in.
    /// </summary>
    [ViewVariables]
    public ContainerSlot GasTankSlot = default!;

    [ViewVariables]
    public readonly string GasTankSlotId = "mech-gas-tank-slot";

    /// <summary>
    /// The slot the battery is stored in.
    /// </summary>
    [ViewVariables]
    public ContainerSlot CapacitorSlot = default!;

    [ViewVariables]
    public readonly string CapacitorSlotId = "mech-capacitor-slot";

    /// <summary>
    /// A multiplier used to calculate how much of the damage done to a mech
    /// is transfered to the pilot
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MechToPilotDamageMultiplier;

    /// <summary>
    /// Whether the mech has been destroyed and is no longer pilotable.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Broken = false;

    /// <summary>
    /// The slot the pilot is stored in.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public ContainerSlot PilotSlot = default!;

    [ViewVariables]
    public readonly string PilotSlotId = "mech-pilot-slot";

    /// <summary>
    /// The current selected equipment of the mech.
    /// If null, the mech is using just its fists.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentSelectedEquipment;

    /// <summary>
    /// The maximum amount of equipment items that can be installed in the mech
    /// </summary>
    [DataField("maxEquipmentAmount"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxEquipmentAmount = 3;

    /// <summary>
    /// A whitelist for inserting equipment items.
    /// </summary>
    [DataField]
    public EntityWhitelist? EquipmentWhitelist;

    [DataField]
    public EntityWhitelist? PilotWhitelist;

    /// <summary>
    /// A container for storing the equipment entities.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Container EquipmentContainer = default!;

    [ViewVariables]
    public readonly string EquipmentContainerId = "mech-equipment-container";

    /// <summary>
    /// How long it takes to enter the mech.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float EntryDelay = 3;

    /// <summary>
    /// How long it takes to pull *another person*
    /// outside of the mech. You can exit instantly yourself.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ExitDelay = 5;

    /// <summary>
    /// How long it takes to pull out the battery.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BatteryRemovalDelay = 2;

    /// <summary>
    /// Whether or not the mech is airtight.
    /// </summary>
    /// <remarks>
    /// This needs to be redone
    /// when mech internals are added
    /// </remarks>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Airtight;

    /// <summary>
    /// The equipment that the mech initially has when it spawns.
    /// Good for things like nukie mechs that start with guns.
    /// </summary>
    [DataField]
    public List<EntProtoId> StartingEquipment = new();

    #region Action Prototypes
    [DataField]
    public EntProtoId MechCycleAction = "ActionMechCycleEquipment";
    [DataField]
    public EntProtoId MechUiAction = "ActionMechOpenUI";
    [DataField]
    public EntProtoId MechEjectAction = "ActionMechEject";
    [DataField]
    public EntProtoId MechToggleLightAction = "ActionMechToggleLights"; // Forge-Change
    [DataField]
    public EntProtoId MechToggleInternalsAction = "ActionMechToggleInternals"; // Forge-Change
    [DataField]
    public EntProtoId MechToggleThrustersAction = "ActionMechToggleThrusters"; // Forge-Change
    #endregion

    #region Visualizer States
    [DataField]
    public string? BaseState;
    [DataField]
    public string? OpenState;
    [DataField]
    public string? BrokenState;
    #endregion

    // Forge-Change-Start
    #region Sounds
    [DataField]
    public SoundSpecifier ToggleLightSound = new SoundPathSpecifier("/Audio/Items/flashlight_pda.ogg");
    [DataField]
    public SoundSpecifier LowPowerSound = new SoundPathSpecifier("/Audio/Forge/Mecha/lowpower.ogg");
    [DataField]
    public SoundSpecifier NominalSound = new SoundPathSpecifier("/Audio/Forge/Mecha/nominal.ogg");
    [DataField]
    public SoundSpecifier NominalLongSound = new SoundPathSpecifier("/Audio/Forge/Mecha/longnanoactivation.ogg");
    [DataField]
    public SoundSpecifier PowerupSound = new SoundPathSpecifier("/Audio/Forge/Mecha/powerup.ogg");
    [DataField]
    public SoundSpecifier CriticalDamageSound = new SoundPathSpecifier("/Audio/Forge/Mecha/critnano.ogg");
    
    [DataField]
    public bool FirstStart = true;
    
    [DataField]
    public bool PlayPowerSound = true;
    [DataField]
    public bool PlayIntegritySound = true;
    #endregion
    // Forge-Change-End

    [DataField] public EntityUid? MechCycleActionEntity;
    [DataField] public EntityUid? MechUiActionEntity;
    [DataField] public EntityUid? MechEjectActionEntity;
    [DataField] public EntityUid? MechToggleLightActionEntity; // Forge-Change
    [DataField] public EntityUid? MechToggleInternalsActionEntity; // Forge-Change
    [DataField] public EntityUid? MechToggleThrustersActionEntity; // Forge-Change

    // Frontier: extra fields
    /// <summary>
    /// Whether or not the equipment in the mech can be removed.
    /// </summary>
    [DataField]
    public bool CanRemoveEquipment = true;

    [DataField(serverOnly: true)]
    public bool MobStateAdded = false;

    [DataField(serverOnly: true)]
    public bool MobThresholdsAdded = false;

    [DataField(serverOnly: true)]
    public bool NpcFactionAdded = false;
    // End Frontier: extra fields
}
