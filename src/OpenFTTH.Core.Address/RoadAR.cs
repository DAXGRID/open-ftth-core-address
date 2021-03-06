using FluentResults;
using OpenFTTH.Core.Address.Events;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.Core.Address;

public enum RoadStatus
{
    Temporary,
    Effective,
}

public class RoadAR : AggregateBase
{
    public string? OfficialId { get; private set; }
    public string? Name { get; private set; }
    public RoadStatus Status { get; private set; }
    public bool Deleted { get; private set; }

    public RoadAR()
    {
        Register<RoadCreated>(Apply);
        Register<RoadUpdated>(Apply);
        Register<RoadDeleted>(Apply);
    }

    public Result Create(Guid id, string? officialId, string name, RoadStatus status)
    {
        if (id == Guid.Empty)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    $"{nameof(id)} cannot be empty guid."));
        }

        if (String.IsNullOrWhiteSpace(officialId))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.OFFICIAL_ID_CANNOT_BE_WHITE_SPACE_OR_NULL,
                    $"{nameof(officialId)} is not allowed to be whitespace or null."));
        }

        Id = id;

        RaiseEvent(new RoadCreated(
            id: id,
            officialId: officialId,
            name: name,
            status: status));

        return Result.Ok();
    }

    public Result Update(string name, string officialId, RoadStatus status)
    {
        if (Id == Guid.Empty)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    @$"{nameof(Id)}, being default guid is not valid,
 the AR has most likely not being created yet."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.CANNOT_UPDATE_DELETED,
                    @$"Cannot update deleted road with id: '{Id}'."));
        }

        var hasChanges = () =>
        {
            if (Name != name)
            {
                return true;
            }
            if (OfficialId != officialId)
            {
                return true;
            }
            if (Status != status)
            {
                return true;
            }

            return false;
        };

        if (!hasChanges())
        {
            return Result.Fail(
               new RoadError(
                   RoadErrorCode.NO_CHANGES,
                   $"No changes for road with id '{Id}'."));
        }

        RaiseEvent(new RoadUpdated(
            id: Id,
            officialId: officialId,
            name: name,
            status: status));

        return Result.Ok();
    }

    public Result Delete()
    {
        if (Id == Guid.Empty)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    @$"{nameof(Id)}, being default guid is not valid,
 the AR has most likely not being created yet."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.CANNOT_DELETE_ALREADY_DELETED,
                    $"Id: '{Id}' is already deleted."));
        }

        RaiseEvent(new RoadDeleted(Id));

        return Result.Ok();
    }

    private void Apply(RoadCreated roadCreated)
    {
        OfficialId = roadCreated.OfficialId;
        Name = roadCreated.Name;
        Status = roadCreated.Status;
    }

    private void Apply(RoadUpdated roadUpdated)
    {
        OfficialId = roadUpdated.OfficialId;
        Name = roadUpdated.Name;
        Status = roadUpdated.Status;
    }

    private void Apply(RoadDeleted roadDeleted)
    {
        Deleted = true;
    }
}
