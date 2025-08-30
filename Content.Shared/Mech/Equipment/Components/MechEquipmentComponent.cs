﻿using Content.Shared.DoAfter;
using Content.Shared.Mech.Components;
using Content.Shared.Mech;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Mech.Equipment.Components;

/// <summary>
/// A piece of equipment that can be installed into <see cref="MechComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class MechEquipmentComponent : Component
{
    /// <summary>
    /// How long does it take to install this piece of equipment
    /// </summary>
    [DataField("installDuration")] public float InstallDuration = 5;

    /// <summary>
    /// The mech that the equipment is inside of.
    /// </summary>
    [ViewVariables] public EntityUid? EquipmentOwner;

    [DataField("canBeUsed")] // Forge-Change
    public bool CanBeUsed = true;
}

// Forge-Change-Start
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechEquipmentActionComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public bool EquipmentToggled = false;
    
    [DataField]
    [AutoNetworkedField]
    public bool EquipmentComponentAdded = false;
    
    [DataField("actionId")]
    public EntProtoId EquipmentAction = "";
    
    [DataField] public EntityUid? EquipmentActionEntity;
}
// Forge-Change-End

/// <summary>
/// Raised on the equipment when the installation is finished successfully
/// </summary>
public sealed class MechEquipmentInstallFinished : EntityEventArgs
{
    public EntityUid Mech;

    public MechEquipmentInstallFinished(EntityUid mech)
    {
        Mech = mech;
    }
}

/// <summary>
/// Raised on the equipment when the installation fails.
/// </summary>
public sealed class MechEquipmentInstallCancelled : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed partial class GrabberDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class InsertEquipmentEvent : SimpleDoAfterEvent
{
}

