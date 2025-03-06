using OpenFTTH.EventSourcing;
using Xunit.Extensions.Ordering;

namespace OpenFTTH.Core.Address.Tests;

public record CreateUnitAddressExampleData
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public Guid AccessAddressId { get; init; }
    public UnitAddressStatus Status { get; init; }
    public string? FloorName { get; init; }
    public string? SuiteName { get; init; }
    public DateTime ExternalCreatedDate { get; init; }
    public DateTime ExternalUpdatedDate { get; init; }
    public bool PendingOfficial { get; init; }

    public CreateUnitAddressExampleData(
        Guid id,
        string? externalId,
        Guid accessAddressId,
        UnitAddressStatus status,
        string? floorName,
        string? suiteName,
        DateTime externalCreatedDate,
        DateTime externalUpdatedDate,
        bool pendingOfficial)
    {
        Id = id;
        ExternalId = externalId;
        AccessAddressId = accessAddressId;
        Status = status;
        FloorName = floorName;
        SuiteName = suiteName;
        ExternalCreatedDate = externalCreatedDate;
        ExternalUpdatedDate = externalUpdatedDate;
        PendingOfficial = pendingOfficial;
    }
}

[Order(20)]
public class UnitAddressTests
{
    private readonly IEventStore _eventStore;

    public UnitAddressTests(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public static IEnumerable<object[]> ExampleCreateValues()
    {
        yield return new object[]
        {
            new CreateUnitAddressExampleData(
                id: Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3"),
                externalId: "8caafc15-331c-4ea8-a97e-26414351336f",
                accessAddressId: Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc"),
                status: UnitAddressStatus.Active,
                floorName: null,
                suiteName: null,
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow,
                pendingOfficial: true)
        };

        yield return new object[]
        {
            new CreateUnitAddressExampleData(
                id: Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6"),
                externalId: "7cc671cc-7d07-47a8-b375-5aa9199d7348",
                accessAddressId: Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc"),
                status: UnitAddressStatus.Active,
                floorName: "1 st.",
                suiteName: "mf",
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow,
                pendingOfficial: false)
        };
    }

    [Theory, Order(1)]
    [MemberData(nameof(ExampleCreateValues))]
    public void Create_is_success(CreateUnitAddressExampleData unitAddressExampleData)
    {
        ArgumentNullException.ThrowIfNull(unitAddressExampleData);

        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: unitAddressExampleData.Id,
            externalId: unitAddressExampleData.ExternalId,
            accessAddressId: unitAddressExampleData.AccessAddressId,
            status: unitAddressExampleData.Status,
            floorName: unitAddressExampleData.FloorName,
            suiteName: unitAddressExampleData.SuiteName,
            externalCreatedDate: unitAddressExampleData.ExternalCreatedDate,
            externalUpdatedDate: unitAddressExampleData.ExternalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: unitAddressExampleData.PendingOfficial);

        _eventStore.Aggregates.Store(unitAddressAR);

        createUnitAddressResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Id.Should().Be(unitAddressExampleData.Id);
        unitAddressAR.ExternalId.Should().Be(unitAddressExampleData.ExternalId);
        unitAddressAR.AccessAddressId.Should().Be(unitAddressExampleData.AccessAddressId);
        unitAddressAR.Status.Should().Be(unitAddressExampleData.Status);
        unitAddressAR.FloorName.Should().Be(unitAddressExampleData.FloorName);
        unitAddressAR.SuiteName.Should().Be(unitAddressExampleData.SuiteName);
        unitAddressAR.ExternalCreatedDate.Should()
            .Be(unitAddressExampleData.ExternalCreatedDate);
        unitAddressAR.ExternalUpdatedDate.Should()
            .Be(unitAddressExampleData.ExternalUpdatedDate);
        unitAddressAR.PendingOfficial.Should().Be(unitAddressExampleData.PendingOfficial);
    }

    [Fact, Order(1)]
    public void Create_default_id_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Empty;
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suiteName = null;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        _eventStore.Aggregates.Store(unitAddressAR);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.ID_CANNOT_BE_EMPTY_GUID);
    }

    [Fact, Order(1)]
    public void Create_access_address_default_id_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Empty;
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suiteName = null;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID);
    }

    [Fact, Order(1)]
    public void Create_could_not_find_access_address_on_id()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("042cc296-ab4b-4cc1-8eed-f021361df6c3");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suiteName = null;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.ACCESS_ADDRESS_DOES_NOT_EXISTS);
    }

    [Fact, Order(2)]
    public void Cannot_create_AR_when_it_has_already_been_created()
    {
     var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suiteName = null;
        var externalCreatedDate = DateTime.Today;
        var externalUpdatedDate = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.ALREADY_CREATED);

        // We load again to make sure that nothing has been changed.
        var unitAddressARAfter = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(unitAddressARAfter);
    }

    [Fact, Order(2)]
    public void Update_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suiteName = null;
        var externalUpdatedDate = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Id.Should().Be(id);
        unitAddressAR.ExternalId.Should().Be(externalId);
        unitAddressAR.AccessAddressId.Should().Be(accessAddressId);
        unitAddressAR.Status.Should().Be(status);
        unitAddressAR.FloorName.Should().Be(floorName);
        unitAddressAR.SuiteName.Should().Be(suiteName);
        unitAddressAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
        unitAddressAR.PendingOfficial.Should().Be(pendingOfficial);
    }

    [Fact, Order(2)]
    public void Update_before_created_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var externalId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suiteName = null;
        var externalUpdatedDate = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = new UnitAddressAR();

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: externalUpdatedDate,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code.Should().Be(UnitAddressErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(2)]
    public void Update_access_address_id_being_default_guid_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Empty;
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suiteName = null;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: updated,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code.Should().Be(UnitAddressErrorCode.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID);
    }

    [Fact, Order(2)]
    public void Update_access_address_id_does_not_exist_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Parse("c00a5940-0184-4b79-baa9-d59290fac67d");
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suiteName = null;
        var updated = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: updated,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.ACCESS_ADDRESS_DOES_NOT_EXISTS);
    }

    [Fact, Order(3)]
    public void Update_with_no_changes_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suiteName = null;
        var updated = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: updated,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCode.NO_CHANGES);
    }

    [Fact, Order(4)]
    public void Update_external_id_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var externalId = "8d40d9ae-b711-4755-b095-c5c037d8dec8";
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdateExternalId(
            externalId: externalId,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.ExternalId.Should().Be(externalId);
    }

    [Fact, Order(4)]
    public void Update_access_address_id_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var accessAddressId = Guid.Parse("567265d6-bbf4-482c-977c-b5d8275962ff");
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdateAccessAddressId(
            accessAddressId: accessAddressId,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.AccessAddressId.Should().Be(accessAddressId);
    }

    [Fact, Order(4)]
    public void Update_status_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var status = UnitAddressStatus.Pending;
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdateStatus(
            status: status,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.Status.Should().Be(status);
    }

    [Fact, Order(4)]
    public void Update_floor_name_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var floorName = "2 st.";
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdateFloorName(
            floorName: floorName,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.FloorName.Should().Be(floorName);
    }

    [Fact, Order(4)]
    public void Update_suite_name_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var suiteName = "mf";
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdateSuiteName(
            suiteName: suiteName,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.SuiteName.Should().Be(suiteName);
    }

    [Fact, Order(4)]
    public void Update_pending_official_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var pendingOfficial = true;
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.UpdatePendingOfficial(
            pendingOfficial: pendingOfficial,
            externalUpdatedDate: updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();

        var reloadedUnitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);
        unitAddressAR.Should().BeEquivalentTo(reloadedUnitAddressAR);
        unitAddressAR.PendingOfficial.Should().Be(pendingOfficial);
    }

    [Fact, Order(5)]
    public void Cannot_delete_when_it_not_been_created()
    {
        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var updated = DateTime.UtcNow;

        var unitAddressAR = new UnitAddressAR();

        var deleteResult = unitAddressAR.Delete(updated);

        deleteResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(deleteResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(5)]
    public void Delete_is_success()
    {
        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");
        var updated = DateTime.UtcNow;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var deleteResult = unitAddressAR.Delete(updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        deleteResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Deleted.Should().BeTrue();
        unitAddressAR.ExternalUpdatedDate.Should().Be(updated);
    }

    [Fact, Order(6)]
    public void Cannot_update_when_deleted()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");
        var externalId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suiteName = null;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;
        var pendingOfficial = false;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            externalId: externalId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suiteName: suiteName,
            externalUpdatedDate: updated,
            existingAccessAddressIds: existingAccessAddressIds,
            pendingOfficial: pendingOfficial);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(updateUnitAddressResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCode.CANNOT_UPDATE_DELETED);
        unitAddressAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(6)]
    public void Cannot_delete_already_deleted()
    {
        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");
        var updated = DateTime.UtcNow;

        var addressProjection = _eventStore.Projections.Get<AddressProjection>();
        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Delete(updated);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(updateUnitAddressResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCode.CANNOT_DELETE_ALREADY_DELETED);
        unitAddressAR.Deleted.Should().BeTrue();
    }
}
