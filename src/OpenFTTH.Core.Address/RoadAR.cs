using OpenFTTH.Results;
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
    public string? ExternalId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public RoadStatus Status { get; private set; }
    public bool Deleted { get; private set; }
    public DateTime? ExternalCreatedDate { get; private set; }
    public DateTime? ExternalUpdatedDate { get; private set; }

    public RoadAR()
    {
        Register<RoadCreated>(Apply);
        Register<RoadNameChanged>(Apply);
        Register<RoadExternalIdChanged>(Apply);
        Register<RoadStatusChanged>(Apply);
        Register<RoadDeleted>(Apply);
    }

    public Result Create(
        Guid id,
        string? externalId,
        string name,
        RoadStatus status,
        DateTime? externalCreatedDate,
        DateTime? externalUpdatedDate)
    {
        if (IsInitialized(Id))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.ALREADY_CREATED,
                    $"Cannot create, it has already been created: {nameof(Id)}: '{Id}'"));
        }

        if (!IsIdValid(id))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    $"{nameof(id)} cannot be empty guid."));
        }

        if (!IsExternalIdValid(externalId))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.EXTERNAL_ID_CANNOT_BE_WHITE_SPACE_OR_NULL,
                    $"{nameof(externalId)} is not allowed to be whitespace or null."));
        }

        RaiseEvent(new RoadCreated(
                       id: id,
                       externalId: externalId ?? throw new InvalidOperationException("This should have been catched in the function '{nameof(IsExternalIdValid)}'."),
                       name: name,
                       status: status,
                       externalCreatedDate: externalCreatedDate,
                       externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Update(
        string name,
        string externalId,
        RoadStatus status,
        DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!HasChanges(this, name, externalId, status))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.NO_CHANGES,
                    $"No changes for road with id '{Id}'."));
        }

        var changeNameResult = ChangeName(name, externalUpdatedDate);
        if (changeNameResult.Errors.Count > 0)
        {
            var error = (RoadError)changeNameResult.Errors.First();
            if (error.Code != RoadErrorCode.NO_CHANGES)
            {
                return changeNameResult;
            }
        }

        var changeExternalIdResult = UpdateExternalId(externalId, externalUpdatedDate);
        if (changeExternalIdResult.Errors.Count > 0)
        {
            var error = (RoadError)changeExternalIdResult.Errors.First();
            if (error.Code != RoadErrorCode.NO_CHANGES)
            {
                return changeExternalIdResult;
            }
        }

        var changeRoadStatusResult = UpdateStatus(status, externalUpdatedDate);
        if (changeRoadStatusResult.Errors.Count > 0)
        {
            var error = (RoadError)changeRoadStatusResult.Errors.First();
            if (error.Code != RoadErrorCode.NO_CHANGES)
            {
                return changeRoadStatusResult;
            }
        }

        return Result.Ok();
    }

    public Result UpdateStatus(RoadStatus status, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsRoadStatusChanged(oldRoadStatus: Status, newRoadStatus: status))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.NO_CHANGES,
                    $"No changes to the {nameof(status)} of the road with id '{Id}'."));
        }

        RaiseEvent(
            new RoadStatusChanged(
                id: Id,
                status: status,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateExternalId(string externalId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsExternalIdChanged(oldExternalId: ExternalId, newExternalId: externalId))
        {
             return Result.Fail(
                new RoadError(
                    RoadErrorCode.NO_CHANGES,
                    $"No changes to the {nameof(externalId)} of the road with id '{Id}'."));
        }

        RaiseEvent(
            new RoadExternalIdChanged(
                id: Id,
                externalId: externalId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result ChangeName(string name, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsNameChanged(oldName: Name, newName: name))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.NO_CHANGES,
                    $"No changes to the {nameof(name)} of the road with id '{Id}'."));
        }

        RaiseEvent(
            new RoadNameChanged(
                id: Id,
                name: name,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Delete(DateTime? externalUpdatedDate)
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.NOT_INITIALIZED,
                    "Cannot delete the AR, when it has not been initialized/created."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.CANNOT_DELETE_ALREADY_DELETED,
                    $"Id: '{Id}' is already deleted."));
        }

        RaiseEvent(new RoadDeleted(Id, externalUpdatedDate));

        return Result.Ok();
    }

    private void Apply(RoadCreated roadCreated)
    {
        Id = roadCreated.Id;
        ExternalId = roadCreated.ExternalId;
        Name = roadCreated.Name;
        Status = roadCreated.Status;
        ExternalCreatedDate = roadCreated.ExternalCreatedDate;
        ExternalUpdatedDate = roadCreated.ExternalUpdatedDate;
    }

    private void Apply(RoadNameChanged roadNameChanged)
    {
        Name = roadNameChanged.Name;
        ExternalUpdatedDate = roadNameChanged.ExternalUpdatedDate;
    }

    private void Apply(RoadExternalIdChanged roadExternalIdChanged)
    {
        ExternalId = roadExternalIdChanged.ExternalId;
        ExternalUpdatedDate = roadExternalIdChanged.ExternalUpdatedDate;
    }

    private void Apply(RoadStatusChanged roadStatusChanged)
    {
        Status = roadStatusChanged.Status;
        ExternalUpdatedDate = roadStatusChanged.ExternalUpdatedDate;
    }

    private void Apply(RoadDeleted roadDeleted)
    {
        Deleted = true;
        ExternalUpdatedDate = roadDeleted.ExternalUpdatedDate;
    }

    private Result CanBeUpdated()
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.NOT_INITIALIZED,
                    "Cannot update the AR, when it has not been initialized/created."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new RoadError(
                    RoadErrorCode.CANNOT_UPDATE_DELETED,
                    @$"Cannot update deleted road with id: '{Id}'."));
        }

        return Result.Ok();
    }

    private static bool HasChanges(
        RoadAR current,
        string name,
        string externalId,
        RoadStatus status)
    {
        return IsNameChanged(current.Name, name)
            || IsExternalIdChanged(current.ExternalId, externalId)
            || IsRoadStatusChanged(current.Status, status);
    }

    private static bool IsNameChanged(string oldName, string newName)
    {
        return oldName != newName;
    }

    private static bool IsExternalIdChanged(string? oldExternalId, string? newExternalId)
    {
        return oldExternalId != newExternalId;
    }

    private static bool IsRoadStatusChanged(RoadStatus oldRoadStatus, RoadStatus newRoadStatus)
    {
        return oldRoadStatus != newRoadStatus;
    }

    private static bool IsInitialized(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsIdValid(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsExternalIdValid(string? externalId)
    {
        return !String.IsNullOrWhiteSpace(externalId);
    }
}
