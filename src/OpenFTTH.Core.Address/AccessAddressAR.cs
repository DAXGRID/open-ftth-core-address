using OpenFTTH.Results;
using OpenFTTH.Core.Address.Events;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.Core.Address;

public enum AccessAddressStatus
{
    Active,
    Canceled,
    Pending,
    Discontinued
}

public class AccessAddressAR : AggregateBase
{
    public string? ExternalId { get; private set; }
    public DateTime? ExternalCreatedDate { get; private set; }
    public DateTime? ExternalUpdatedDate { get; private set; }
    public string MunicipalCode { get; private set; } = string.Empty;
    public AccessAddressStatus Status { get; private set; }
    public string RoadCode { get; private set; } = string.Empty;
    public string HouseNumber { get; private set; } = string.Empty;
    public Guid PostCodeId { get; private set; }
    public double EastCoordinate { get; private set; }
    public double NorthCoordinate { get; private set; }
    public string? SupplementaryTownName { get; private set; }
    public string? PlotId { get; private set; }
    public Guid RoadId { get; private set; }
    public bool Deleted { get; private set; }
    public bool PendingOfficial { get; private set; }

    public AccessAddressAR()
    {
        Register<AccessAddressCreated>(Apply);
        Register<AccessAddressExternalIdChanged>(Apply);
        Register<AccessAddressMunicipalCodeChanged>(Apply);
        Register<AccessAddressStatusChanged>(Apply);
        Register<AccessAddressRoadCodeChanged>(Apply);
        Register<AccessAddressHouseNumberChanged>(Apply);
        Register<AccessAddressPostCodeIdChanged>(Apply);
        Register<AccessAddressSupplementaryTownNameChanged>(Apply);
        Register<AccessAddressPlotIdChanged>(Apply);
        Register<AccessAddressRoadIdChanged>(Apply);
        Register<AccessAddressPendingOfficialChanged>(Apply);
        Register<AccessAddressCoordinateChanged>(Apply);
        Register<AccessAddressDeleted>(Apply);
    }

    public Result Create(
        Guid id,
        string? externalId,
        DateTime? externalCreatedDate,
        DateTime? externalUpdatedDate,
        string municipalCode,
        AccessAddressStatus status,
        string roadCode,
        string houseNumber,
        Guid postCodeId,
        double eastCoordinate,
        double northCoordinate,
        string? supplementaryTownName,
        string? plotId,
        Guid roadId,
        HashSet<Guid> existingRoadIds,
        HashSet<Guid> existingPostCodeIds,
        bool pendingOfficial)
    {
        if (IsInitialized(Id))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.ALREADY_CREATED,
                    $"Cannot create, it has already been created: {nameof(Id)}: '{Id}'."));
        }

        if (!IsIdValid(id))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.ID_CANNOT_BE_EMPTY_GUID,
                    $"Id cannot be empty guid."));
        }

        if (!IsRoadIdValid(existingRoadIds, roadId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.ROAD_DOES_NOT_EXIST,
                    $"No roads exist with id '{roadId}'."));
        }

        if (!IsPostCodeIdValid(existingPostCodeIds, postCodeId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.POST_CODE_DOES_NOT_EXIST,
                    $"No postcode exists with id '{postCodeId}'."));
        }

        RaiseEvent(
            new AccessAddressCreated(
                id: id,
                externalId: externalId,
                externalCreatedDate: externalCreatedDate,
                externalUpdatedDate: externalUpdatedDate,
                municipalCode: municipalCode,
                status: status,
                roadCode: roadCode,
                houseNumber: houseNumber,
                postCodeId: postCodeId,
                eastCoordinate: eastCoordinate,
                northCoordinate: northCoordinate,
                townName: supplementaryTownName,
                plotId: plotId,
                roadId: roadId,
                pendingOfficial: pendingOfficial,
                supplementaryTownName: supplementaryTownName));

        return Result.Ok();
    }

    public Result Update(
        string? externalId,
        DateTime? externalUpdatedDate,
        string municipalCode,
        AccessAddressStatus status,
        string roadCode,
        string houseNumber,
        Guid postCodeId,
        double eastCoordinate,
        double northCoordinate,
        string? supplementaryTownName,
        string? plotId,
        Guid roadId,
        HashSet<Guid> existingRoadIds,
        HashSet<Guid> existingPostCodeIds,
        bool pendingOfficial)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsRoadIdValid(existingRoadIds, roadId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.ROAD_DOES_NOT_EXIST,
                    @$"No roads exist with id '{roadId}'."));
        }

        if (!IsPostCodeIdValid(existingPostCodeIds, postCodeId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.POST_CODE_DOES_NOT_EXIST,
                    $"No postcode exists with id '{postCodeId}'."));
        }

        if (!HasChanges(
                this,
                externalId,
                municipalCode,
                status,
                roadCode,
                houseNumber,
                postCodeId,
                eastCoordinate,
                northCoordinate,
                supplementaryTownName,
                plotId,
                roadId,
                pendingOfficial))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"Cannot update access address, no changes for id '{Id}'."));
        }

        var updateExternalIdResult = UpdateExternalId(externalId, externalUpdatedDate);
        if (updateExternalIdResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateExternalIdResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateExternalIdResult;
            }
        }

        var updateMunicipalCodeResult = UpdateMunicipalCode(municipalCode, externalUpdatedDate);
        if (updateMunicipalCodeResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateMunicipalCodeResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateMunicipalCodeResult;
            }
        }

        var updateStatusResult = UpdateStatus(status, externalUpdatedDate);
        if (updateStatusResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateStatusResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateStatusResult;
            }
        }

        var updateRoadCodeResult = UpdateRoadCode(roadCode, externalUpdatedDate);
        if (updateRoadCodeResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateRoadCodeResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateRoadCodeResult;
            }
        }

        var updateHouseNumberResult = UpdateHouseNumber(houseNumber, externalUpdatedDate);
        if (updateHouseNumberResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateHouseNumberResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateHouseNumberResult;
            }
        }

        var updatePostCodeIdResult = UpdatePostCodeId(postCodeId, externalUpdatedDate);
        if (updatePostCodeIdResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updatePostCodeIdResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updatePostCodeIdResult;
            }
        }

        var updateSupplementaryTownNameResult = UpdateSupplementaryTownName(supplementaryTownName, externalUpdatedDate);
        if (updateSupplementaryTownNameResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateSupplementaryTownNameResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateSupplementaryTownNameResult;
            }
        }

        var updatePlotIdResult = UpdatePlotId(plotId, externalUpdatedDate);
        if (updatePlotIdResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updatePlotIdResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updatePlotIdResult;
            }
        }

        var updateRoadIdResult = UpdateRoadId(roadId, externalUpdatedDate);
        if (updateRoadIdResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateRoadIdResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateRoadIdResult;
            }
        }

        var updatePendingOfficialResult = UpdatePendingOfficial(pendingOfficial, externalUpdatedDate);
        if (updatePendingOfficialResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updatePendingOfficialResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updatePendingOfficialResult;
            }
        }

        var updateCoordinateResult = UpdateCoordinate(eastCoordinate, northCoordinate, externalUpdatedDate);
        if (updateCoordinateResult.Errors.Count > 0)
        {
            var error = (AccessAddressError)updateCoordinateResult.Errors.First();
            if (error.Code != AccessAddressErrorCode.NO_CHANGES)
            {
                return updateCoordinateResult;
            }
        }

        return Result.Ok();
    }

    public Result UpdateExternalId(string? externalId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsExternalIdChanged(oldExternalId: ExternalId, newExternalId: externalId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(externalId)}."));
        }

        RaiseEvent(
            new AccessAddressExternalIdChanged(
                id: Id,
                externalId: externalId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateMunicipalCode(string municipalCode, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsMunicipalCodeChanged(oldMunicipalCode: MunicipalCode, newMunicipalCode: municipalCode))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(municipalCode)}."));
        }

        RaiseEvent(
            new AccessAddressMunicipalCodeChanged(
                id: Id,
                municipalCode: municipalCode,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateStatus(AccessAddressStatus status, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsStatusChanged(oldStatus: Status, newStatus: status))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(status)}."));
        }

        RaiseEvent(
            new AccessAddressStatusChanged(
                id: Id,
                status: status,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateRoadCode(string roadCode, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsRoadCodeChanged(oldRoadCode: RoadCode, newRoadCode: roadCode))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(roadCode)}."));
        }

        RaiseEvent(
            new AccessAddressRoadCodeChanged(
                id: Id,
                roadCode: roadCode,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateHouseNumber(string houseNumber, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsHouseNumberChanged(oldHouseNumber: HouseNumber, newHouseNumber: houseNumber))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(houseNumber)}."));
        }

        RaiseEvent(
            new AccessAddressHouseNumberChanged(
                id: Id,
                houseNumber: houseNumber,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdatePostCodeId(Guid postCodeId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsPostCodeIdChanged(oldPostCodeId: PostCodeId, newPostCodeId: postCodeId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(postCodeId)}."));
        }

        RaiseEvent(
            new AccessAddressPostCodeIdChanged(
                id: Id,
                postCodeId: postCodeId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateSupplementaryTownName(string? supplementaryTownName, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsSupplementaryTownNameChanged(oldSupplementaryTownName: SupplementaryTownName, newSupplementaryTownName: supplementaryTownName))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(supplementaryTownName)}."));
        }

        RaiseEvent(
            new AccessAddressSupplementaryTownNameChanged(
                id: Id,
                supplementaryTownName: supplementaryTownName,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdatePlotId(string? plotId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsPlotIdChanged(oldPlotId: PlotId, newPlotId: plotId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(plotId)}."));
        }

        RaiseEvent(
            new AccessAddressPlotIdChanged(
                id: Id,
                plotId: plotId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateRoadId(Guid roadId, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsRoadIdChanged(oldRoadId: RoadId, newRoadId: roadId))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(roadId)}."));
        }

        RaiseEvent(
            new AccessAddressRoadIdChanged(
                id: Id,
                roadId: roadId,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdatePendingOfficial(bool pendingOfficial, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsPendingOfficialChanged(oldPendingOfficial: PendingOfficial, newPendingOfficial: pendingOfficial))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(pendingOfficial)}."));
        }

        RaiseEvent(
            new AccessAddressPendingOfficialChanged(
                id: Id,
                pendingOfficial: pendingOfficial,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result UpdateCoordinate(double eastCoordinate, double northCoordinate, DateTime? externalUpdatedDate)
    {
        var canBeUpdatedResult = CanBeUpdated(Id, Deleted);
        if (canBeUpdatedResult.IsFailed)
        {
            return canBeUpdatedResult;
        }

        if (!IsCoordinateChanged(
                oldEastCoordinate: EastCoordinate,
                newEastCoordinate: eastCoordinate,
                oldNorthCoordinate: NorthCoordinate,
                newNorthCoordinate: northCoordinate))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NO_CHANGES,
                    $"There was no changes to {nameof(eastCoordinate)} or {nameof(northCoordinate)}."));
        }

        RaiseEvent(
            new AccessAddressCoordinateChanged(
                id: Id,
                eastCoordinate: eastCoordinate,
                northCoordinate: northCoordinate,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Delete(DateTime? externalUpdatedDate)
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NOT_INITIALIZED,
                    "Cannot delete, before it as been initialized."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.CANNOT_DELETE_ALREADY_DELETED,
                    @$"Cannot delete, it is already deleted."));
        }

        RaiseEvent(new AccessAddressDeleted(Id, externalUpdatedDate));

        return Result.Ok();
    }

    private void Apply(AccessAddressCreated accessAddressCreated)
    {
        Id = accessAddressCreated.Id;
        ExternalId = accessAddressCreated.ExternalId;
        ExternalCreatedDate = accessAddressCreated.ExternalCreatedDate;
        ExternalUpdatedDate = accessAddressCreated.ExternalUpdatedDate;
        MunicipalCode = accessAddressCreated.MunicipalCode;
        Status = accessAddressCreated.Status;
        RoadCode = accessAddressCreated.RoadCode;
        HouseNumber = accessAddressCreated.HouseNumber;
        PostCodeId = accessAddressCreated.PostCodeId;
        EastCoordinate = accessAddressCreated.EastCoordinate;
        NorthCoordinate = accessAddressCreated.NorthCoordinate;
        SupplementaryTownName = accessAddressCreated.TownName;
        PlotId = accessAddressCreated.PlotId;
        RoadId = accessAddressCreated.RoadId;
        PendingOfficial = accessAddressCreated.PendingOfficial;
    }

    private void Apply(AccessAddressExternalIdChanged accessAddressExternalIdChanged)
    {
        ExternalId = accessAddressExternalIdChanged.ExternalId;
        ExternalUpdatedDate = accessAddressExternalIdChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressMunicipalCodeChanged accessAddressMunicipalCodeChanged)
    {
        MunicipalCode = accessAddressMunicipalCodeChanged.MunicipalCode;
        ExternalUpdatedDate = accessAddressMunicipalCodeChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressStatusChanged accessAddressStatusChanged)
    {
        Status = accessAddressStatusChanged.Status;
        ExternalUpdatedDate = accessAddressStatusChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressRoadCodeChanged accessAddressRoadCodeChanged)
    {
        RoadCode = accessAddressRoadCodeChanged.RoadCode;
        ExternalUpdatedDate = accessAddressRoadCodeChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressHouseNumberChanged accessAddressHouseNumberChanged)
    {
        HouseNumber = accessAddressHouseNumberChanged.HouseNumber;
        ExternalUpdatedDate = accessAddressHouseNumberChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressSupplementaryTownNameChanged accessAddressSupplementaryTownNameChanged)
    {
        SupplementaryTownName = accessAddressSupplementaryTownNameChanged.SupplementaryTownName;
        ExternalUpdatedDate = accessAddressSupplementaryTownNameChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressPlotIdChanged accessAddressPlotIdChanged)
    {
        PlotId = accessAddressPlotIdChanged.PlotId;
        ExternalUpdatedDate = accessAddressPlotIdChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressRoadIdChanged accessAddressRoadIdChanged)
    {
        RoadId = accessAddressRoadIdChanged.RoadId;
        ExternalUpdatedDate = accessAddressRoadIdChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressPendingOfficialChanged accessAddressPendingOfficialChanged)
    {
        PendingOfficial = accessAddressPendingOfficialChanged.PendingOfficial;
        ExternalUpdatedDate = accessAddressPendingOfficialChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressCoordinateChanged accessAddressCoordinateChanged)
    {
        EastCoordinate = accessAddressCoordinateChanged.EastCoordinate;
        NorthCoordinate = accessAddressCoordinateChanged.NorthCoordinate;
        ExternalUpdatedDate = accessAddressCoordinateChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressPostCodeIdChanged accessAddressPostCodeIdChanged)
    {
        PostCodeId = accessAddressPostCodeIdChanged.PostCodeId;
        ExternalUpdatedDate = accessAddressPostCodeIdChanged.ExternalUpdatedDate;
    }

    private void Apply(AccessAddressDeleted accessAddressDeleted)
    {
        Deleted = true;
        ExternalUpdatedDate = accessAddressDeleted.ExternalUpdatedDate;
    }

    public static Result CanBeUpdated(Guid id, bool deleted)
    {
        if (!IsInitialized(id))
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.NOT_INITIALIZED,
                    "Cannot update, before it as been initialized."));
        }

        if (deleted)
        {
            return Result.Fail(
                new AccessAddressError(
                    AccessAddressErrorCode.CANNOT_UPDATE_DELETED,
                    @$"The access address is deleted, and cannot be updated."));
        }

        return Result.Ok();
    }

    private static bool IsExternalIdChanged(string? oldExternalId, string? newExternalId)
    {
        return oldExternalId != newExternalId;
    }

    private static bool IsMunicipalCodeChanged(string oldMunicipalCode, string newMunicipalCode)
    {
        return oldMunicipalCode != newMunicipalCode;
    }

    private static bool IsStatusChanged(AccessAddressStatus oldStatus, AccessAddressStatus newStatus)
    {
        return oldStatus != newStatus;
    }

    private static bool IsRoadCodeChanged(string oldRoadCode, string newRoadCode)
    {
        return oldRoadCode != newRoadCode;
    }

    private static bool IsHouseNumberChanged(string oldHouseNumber, string newHouseNumber)
    {
        return oldHouseNumber != newHouseNumber;
    }

    private static bool IsPostCodeIdChanged(Guid oldPostCodeId, Guid newPostCodeId)
    {
        return oldPostCodeId != newPostCodeId;
    }

    private static bool IsSupplementaryTownNameChanged(string? oldSupplementaryTownName, string? newSupplementaryTownName)
    {
        return oldSupplementaryTownName != newSupplementaryTownName;
    }

    private static bool IsPlotIdChanged(string? oldPlotId, string? newPlotId)
    {
        return oldPlotId != newPlotId;
    }

    private static bool IsRoadIdChanged(Guid oldRoadId, Guid newRoadId)
    {
        return oldRoadId != newRoadId;
    }

    private static bool IsPendingOfficialChanged(bool oldPendingOfficial, bool newPendingOfficial)
    {
        return oldPendingOfficial != newPendingOfficial;
    }

    private static bool IsCoordinateChanged(
        double oldEastCoordinate,
        double newEastCoordinate,
        double oldNorthCoordinate,
        double newNorthCoordinate)
        {
            return oldEastCoordinate != newEastCoordinate
                || oldNorthCoordinate != newNorthCoordinate;
        }

    private static bool HasChanges(
        AccessAddressAR current,
        string? externalId,
        string municipalCode,
        AccessAddressStatus status,
        string roadCode,
        string houseNumber,
        Guid postCodeId,
        double eastCoordinate,
        double northCoordinate,
        string? supplementaryTownName,
        string? plotId,
        Guid roadId,
        bool pendingOfficial)
    {
        return IsExternalIdChanged(current.ExternalId, externalId)
            || IsMunicipalCodeChanged(current.MunicipalCode, municipalCode)
            || IsStatusChanged(current.Status, status)
            || IsRoadCodeChanged(current.RoadCode, roadCode)
            || IsHouseNumberChanged(current.HouseNumber, houseNumber)
            || IsPostCodeIdChanged(current.PostCodeId, postCodeId)
            || IsSupplementaryTownNameChanged(current.SupplementaryTownName, supplementaryTownName)
            || IsPlotIdChanged(current.PlotId, plotId)
            || IsRoadIdChanged(current.RoadId, roadId)
            || IsPendingOfficialChanged(current.PendingOfficial, pendingOfficial)
            || IsCoordinateChanged(
                current.EastCoordinate,
                eastCoordinate,
                current.NorthCoordinate,
                northCoordinate);
    }

    private static bool IsInitialized(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsIdValid(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsRoadIdValid(HashSet<Guid> roadIds, Guid roadId)
    {
        return roadIds is not null && roadIds.Contains(roadId);
    }

    private static bool IsPostCodeIdValid(HashSet<Guid> postCodeIds, Guid postCodeId)
    {
        return postCodeIds is not null && postCodeIds.Contains(postCodeId);
    }
}
