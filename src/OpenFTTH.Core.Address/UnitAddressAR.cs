using OpenFTTH.Results;
using OpenFTTH.Core.Address.Events;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.Core.Address;

public enum UnitAddressStatus
{
    Active,
    Canceled,
    Pending,
    Discontinued
}

public class UnitAddressAR : AggregateBase
{
    public string? ExternalId { get; private set; }
    public Guid AccessAddressId { get; private set; }
    public UnitAddressStatus Status { get; private set; }
    public string? FloorName { get; private set; }
    public string? SuiteName { get; private set; }
    public DateTime? ExternalCreatedDate { get; private set; }
    public DateTime? ExternalUpdatedDate { get; private set; }
    public bool Deleted { get; private set; }
    public bool PendingOfficial { get; private set; }

    public UnitAddressAR()
    {
        Register<UnitAddressCreated>(Apply);
        Register<UnitAddressExternalIdChanged>(Apply);
        Register<UnitAddressAccessAddressIdChanged>(Apply);
        Register<UnitAddressStatusChanged>(Apply);
        Register<UnitAddressFloorNameChanged>(Apply);
        Register<UnitAddressSuiteNameChanged>(Apply);
        Register<UnitAddressPendingOfficialChanged>(Apply);
        Register<UnitAddressDeleted>(Apply);
    }

    public Result Create(
        Guid id,
        string? externalId,
        Guid accessAddressId,
        UnitAddressStatus status,
        string? floorName,
        string? suiteName,
        DateTime? externalCreatedDate,
        DateTime? externalUpdatedDate,
        HashSet<Guid> existingAccessAddressIds,
        bool pendingOfficial)
    {
        if (IsInitialized(Id))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ALREADY_CREATED,
                    $"Cannot create, it has already been created: {nameof(Id)}: '{Id}'."));
        }

        if (!IsIdValid(id))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    "{nameof(id)} cannot be empty guid."));
        }

        if (!IsAccessAddressIdValid(accessAddressId))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID,
                    $"{nameof(accessAddressId)} cannot be empty guid."));
        }

        if (!AccessAddressExist(accessAddressId, existingAccessAddressIds))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ACCESS_ADDRESS_DOES_NOT_EXISTS,
                    $"Cannot reference an access-address that does not exist ('{accessAddressId}')."));
        }

        RaiseEvent(new UnitAddressCreated(
                       id: id,
                       externalId: externalId,
                       accessAddressId: accessAddressId,
                       status: status,
                       floorName: floorName,
                       suiteName: suiteName,
                       externalCreatedDate: externalCreatedDate,
                       externalUpdatedDate: externalUpdatedDate,
                       pendingOfficial: pendingOfficial));

        return Result.Ok();
    }

    public Result Update(
        string? externalId,
        Guid accessAddressId,
        UnitAddressStatus status,
        string? floorName,
        string? suiteName,
        DateTime? externalUpdatedDate,
        HashSet<Guid> existingAccessAddressIds,
        bool pendingOfficial)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsAccessAddressIdValid(accessAddressId))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID,
                    $"{nameof(accessAddressId)} cannot be an empty guid."));
        }

        if (!AccessAddressExist(accessAddressId, existingAccessAddressIds))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.ACCESS_ADDRESS_DOES_NOT_EXISTS,
                    "Cannot reference to a access-address that does not exist, {nameof(accessAddressI)}: ('{accessAddressId}')."));
        }

        if (!HasChanges(this,
                        externalId,
                        accessAddressId,
                        status,
                        floorName,
                        suiteName,
                        externalUpdatedDate,
                        pendingOfficial))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There was no changes, no new events emittet."));
        }

        var updateExternalIdResult = UpdateExternalId(externalId, externalUpdatedDate);
        if (updateExternalIdResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updateExternalIdResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updateExternalIdResult;
            }
        }

        var updateAccessAddressIdResult = UpdateAccessAddressId(accessAddressId, externalUpdatedDate);
        if (updateAccessAddressIdResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updateAccessAddressIdResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updateAccessAddressIdResult;
            }
        }

        var updateStatusResult = UpdateStatus(status, externalUpdatedDate);
        if (updateStatusResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updateStatusResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updateStatusResult;
            }
        }

        var updateFloorNameResult = UpdateFloorName(floorName, externalUpdatedDate);
        if (updateFloorNameResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updateFloorNameResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updateFloorNameResult;
            }
        }

        var updateSuiteNameResult = UpdateSuiteName(suiteName, externalUpdatedDate);
        if (updateSuiteNameResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updateSuiteNameResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updateSuiteNameResult;
            }
        }

        var updatePendingOfficialResult = UpdatePendingOfficial(pendingOfficial, externalUpdatedDate);
        if (updatePendingOfficialResult.Errors.Count > 0)
        {
            var error = (UnitAddressError)updatePendingOfficialResult.Errors.First();
            if (error.Code != UnitAddressErrorCode.NO_CHANGES)
            {
                return updatePendingOfficialResult;
            }
        }

        return Result.Ok();
    }

    public Result UpdateExternalId(string? externalId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsExternalIdChanged(ExternalId, externalId))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(externalId)}."));
        }

        RaiseEvent(
            new UnitAddressExternalIdChanged(
                id: Id,
                externalId: externalId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateAccessAddressId(Guid accessAddressId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsAccessAddressIdChanged(AccessAddressId, accessAddressId))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There are no changes to the {nameof(accessAddressId)}."));
        }

        RaiseEvent(
            new UnitAddressAccessAddressIdChanged(
                id: Id,
                accessAddressId: accessAddressId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateStatus(UnitAddressStatus status, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsStatusChanged(Status, status))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There are no changes to the {nameof(status)}."));
        }

        RaiseEvent(
            new UnitAddressStatusChanged(
                id: Id,
                status: status,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateFloorName(string? floorName, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsFloorNameChanged(FloorName, floorName))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There are no changes to the {nameof(floorName)}."));
        }

        RaiseEvent(
            new UnitAddressFloorNameChanged(
                id: Id,
                floorName: floorName,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateSuiteName(string? suiteName, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsSuiteNameChanged(SuiteName, suiteName))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There are no changes to the {nameof(suiteName)}."));
        }

        RaiseEvent(
            new UnitAddressSuiteNameChanged(
                id: Id,
                suiteName: suiteName,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdatePendingOfficial(bool pendingOfficial, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated();
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsPendingOfficialChanged(PendingOfficial, pendingOfficial))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NO_CHANGES,
                    $"There are no changes to the {nameof(pendingOfficial)}."));
        }

        RaiseEvent(
            new UnitAddressPendingOfficialChanged(
                id: Id,
                pendingOfficial: pendingOfficial,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Delete(DateTime? externalUpdatedDate)
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NOT_INITIALIZED,
                    "Cannot delete, before it has been initialized."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.CANNOT_DELETE_ALREADY_DELETED,
                    @$"Cannot delete already deleted."));
        }

        RaiseEvent(new UnitAddressDeleted(Id, externalUpdatedDate));

        return Result.Ok();
    }

    private void Apply(UnitAddressCreated unitAddressCreated)
    {
        Id = unitAddressCreated.Id;
        ExternalId = unitAddressCreated.ExternalId;
        AccessAddressId = unitAddressCreated.AccessAddressId;
        Status = unitAddressCreated.Status;
        FloorName = unitAddressCreated.FloorName;
        SuiteName = unitAddressCreated.SuiteName;
        ExternalCreatedDate = unitAddressCreated.ExternalCreatedDate;
        ExternalUpdatedDate = unitAddressCreated.ExternalUpdatedDate;
        PendingOfficial = unitAddressCreated.PendingOfficial;
    }

    private void Apply(UnitAddressExternalIdChanged unitAddressExternalIdChanged)
    {
        ExternalId = unitAddressExternalIdChanged.ExternalId;
        ExternalUpdatedDate = unitAddressExternalIdChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressAccessAddressIdChanged unitAddressAccessAddressIdChanged)
    {
        AccessAddressId = unitAddressAccessAddressIdChanged.AccessAddressId;
        ExternalUpdatedDate = unitAddressAccessAddressIdChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressStatusChanged unitAddressStatusChanged)
    {
        Status = unitAddressStatusChanged.Status;
        ExternalUpdatedDate = unitAddressStatusChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressFloorNameChanged unitAddressFloorNameChanged)
    {
        FloorName = unitAddressFloorNameChanged.FloorName;
        ExternalUpdatedDate = unitAddressFloorNameChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressSuiteNameChanged unitAddressSuiteNameChanged)
    {
        SuiteName = unitAddressSuiteNameChanged.SuiteName;
        ExternalUpdatedDate = unitAddressSuiteNameChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressPendingOfficialChanged unitAddressPendingOfficialChanged)
    {
        PendingOfficial = unitAddressPendingOfficialChanged.PendingOfficial;
        ExternalUpdatedDate = unitAddressPendingOfficialChanged.ExternalUpdatedDate;
    }

    private void Apply(UnitAddressDeleted unitAddressDeleted)
    {
        Deleted = true;
        ExternalUpdatedDate = unitAddressDeleted.ExternalUpdatedDate;
    }

    private Result CanBeUpdated()
    {
        if (Id == Guid.Empty)
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.NOT_INITIALIZED,
                    "Cannot update, before it has been initialized."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new UnitAddressError(
                    UnitAddressErrorCode.CANNOT_UPDATE_DELETED,
                    "Cannot update when it has been deleted."));
        }

        return Result.Ok();
    }

    private static bool IsInitialized(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsIdValid(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsAccessAddressIdValid(Guid accessAddressId)
    {
        return accessAddressId != Guid.Empty;
    }

    private static bool AccessAddressExist(
        Guid accessAddressId,
        HashSet<Guid> existingAccessAddressIds)
    {
        return existingAccessAddressIds is not null
            && existingAccessAddressIds.Contains(accessAddressId);
    }

    private static bool IsExternalIdChanged(
        string? oldExternalId,
        string? newExternalId)
    {
        return oldExternalId != newExternalId;
    }

    private static bool IsAccessAddressIdChanged(
        Guid oldAccessAddressId,
        Guid newAccessAddressId)
    {
        return oldAccessAddressId != newAccessAddressId;
    }

    private static bool IsStatusChanged(
        UnitAddressStatus oldStatus,
        UnitAddressStatus newStatus)
    {
        return oldStatus != newStatus;
    }

    private static bool IsFloorNameChanged(
        string? oldFloorName,
        string? newFloorName)
    {
        return oldFloorName != newFloorName;
    }

    private static bool IsSuiteNameChanged(
        string? oldSuiteName,
        string? newSuiteName)
    {
        return oldSuiteName != newSuiteName;
    }

    private static bool IsPendingOfficialChanged(
        bool oldPendingOfficial,
        bool newPendingOfficial)
    {
        return oldPendingOfficial != newPendingOfficial;
    }

    private static bool HasChanges(
        UnitAddressAR current,
        string? externalId,
        Guid accessAddressId,
        UnitAddressStatus status,
        string? floorName,
        string? suiteName,
        DateTime? externalUpdatedDate,
        bool pendingOfficial)
    {
        return IsExternalIdChanged(current.ExternalId, externalId)
            || IsAccessAddressIdChanged(current.AccessAddressId, accessAddressId)
            || IsStatusChanged(current.Status, status)
            || IsFloorNameChanged(current.FloorName, floorName)
            || IsSuiteNameChanged(current.SuiteName, suiteName)
            || IsPendingOfficialChanged(current.PendingOfficial, pendingOfficial);
    }
}
