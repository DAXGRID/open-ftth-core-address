using OpenFTTH.EventSourcing;
using Xunit.Extensions.Ordering;

namespace OpenFTTH.Core.Address.Tests;

public record CreateUnitAddressExampleData
{
    public Guid Id { get; init; }
    public string? OfficialId { get; init; }
    public Guid AccessAddressId { get; init; }
    public UnitAddressStatus Status { get; init; }
    public string? FloorName { get; init; }
    public string? SuitName { get; init; }
    public DateTime Created { get; init; }
    public DateTime Updated { get; init; }

    public CreateUnitAddressExampleData(
        Guid id,
        string? officialId,
        Guid accessAddressId,
        UnitAddressStatus status,
        string? floorName,
        string? suitName,
        DateTime created,
        DateTime updated)
    {
        Id = id;
        OfficialId = officialId;
        AccessAddressId = accessAddressId;
        Status = status;
        FloorName = floorName;
        SuitName = suitName;
        Created = created;
        Updated = updated;
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
                officialId: "8caafc15-331c-4ea8-a97e-26414351336f",
                accessAddressId: Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc"),
                status: UnitAddressStatus.Active,
                floorName: null,
                suitName: null,
                created: DateTime.UtcNow,
                updated: DateTime.UtcNow)
        };

        yield return new object[]
        {
            new CreateUnitAddressExampleData(
                id: Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6"),
                officialId: "7cc671cc-7d07-47a8-b375-5aa9199d7348",
                accessAddressId: Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc"),
                status: UnitAddressStatus.Active,
                floorName: "1 st.",
                suitName: "mf",
                created: DateTime.UtcNow,
                updated: DateTime.UtcNow)
        };
    }

    [Theory, Order(1)]
    [MemberData(nameof(ExampleCreateValues))]
    public void Create_is_success(CreateUnitAddressExampleData unitAddressExampleData)
    {
        if (unitAddressExampleData is null)
        {
            throw new ArgumentNullException(nameof(unitAddressExampleData));
        }

        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: unitAddressExampleData.Id,
            officialId: unitAddressExampleData.OfficialId,
            accessAddressId: unitAddressExampleData.AccessAddressId,
            status: unitAddressExampleData.Status,
            floorName: unitAddressExampleData.FloorName,
            suitName: unitAddressExampleData.SuitName,
            created: unitAddressExampleData.Created,
            updated: unitAddressExampleData.Updated,
            existingAccessAddressIds: existingAccessAddressIds);

        _eventStore.Aggregates.Store(unitAddressAR);

        createUnitAddressResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Id.Should().Be(unitAddressExampleData.Id);
        unitAddressAR.OfficialId.Should().Be(unitAddressExampleData.OfficialId);
        unitAddressAR.AccessAddressId.Should().Be(unitAddressExampleData.AccessAddressId);
        unitAddressAR.Status.Should().Be(unitAddressExampleData.Status);
        unitAddressAR.FloorName.Should().Be(unitAddressExampleData.FloorName);
        unitAddressAR.SuitName.Should().Be(unitAddressExampleData.SuitName);
        unitAddressAR.Created.Should().Be(unitAddressExampleData.Created);
        unitAddressAR.Updated.Should().Be(unitAddressExampleData.Updated);
    }

    [Fact, Order(1)]
    public void Create_default_id_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Empty;
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suitName = null;
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            created: created,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        _eventStore.Aggregates.Store(unitAddressAR);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.ID_CANNOT_BE_EMPTY_GUID);

    }

    [Fact, Order(1)]
    public void Create_access_address_default_id_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Empty;
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suitName = null;
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            created: created,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID);
    }

    [Fact, Order(1)]
    public void Create_default_created_date_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suitName = null;
        var created = new DateTime();
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            created: created,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.CREATED_CANNOT_BE_DEFAULT_DATE);
    }

    [Fact, Order(1)]
    public void Create_default_updated_date_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suitName = null;
        var created = DateTime.UtcNow;
        var updated = new DateTime();
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            created: created,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.UPDATED_CANNOT_BE_DEFAULT_DATE);
    }

    [Fact, Order(1)]
    public void Create_could_not_find_access_address_on_id()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("042cc296-ab4b-4cc1-8eed-f021361df6c3");
        var status = UnitAddressStatus.Active;
        string? floorName = null;
        string? suitName = null;
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var createUnitAddressResult = unitAddressAR.Create(
            id: id,
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            created: created,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        createUnitAddressResult.IsSuccess.Should().BeFalse();
        createUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)createUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.ACCESS_ADDRESS_DOES_NOT_EXISTS);
    }

    [Fact, Order(2)]
    public void Update_is_success()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Id.Should().Be(id);
        unitAddressAR.OfficialId.Should().Be(officialId);
        unitAddressAR.AccessAddressId.Should().Be(accessAddressId);
        unitAddressAR.Status.Should().Be(status);
        unitAddressAR.FloorName.Should().Be(floorName);
        unitAddressAR.SuitName.Should().Be(suitName);
        unitAddressAR.Updated.Should().Be(updated);
    }

    [Fact, Order(2)]
    public void Update_before_created_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var officialId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = new UnitAddressAR();

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code.Should().Be(UnitAddressErrorCodes.ID_NOT_SET);
    }

    [Fact, Order(2)]
    public void Update_access_address_id_being_default_guid_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Empty;
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code.Should().Be(UnitAddressErrorCodes.ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID);
    }

    [Fact, Order(2)]
    public void Update_updated_being_default_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suitName = null;
        var updated = new DateTime();
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code.Should().Be(UnitAddressErrorCodes.UPDATED_CANNOT_BE_DEFAULT_DATE);
    }

    [Fact, Order(2)]
    public void Update_access_address_id_does_not_exist_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "89852ac6-254f-4938-aec8-4fac7cb72901";
        var accessAddressId = Guid.Parse("c00a5940-0184-4b79-baa9-d59290fac67d");
        var status = UnitAddressStatus.Pending;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.ACCESS_ADDRESS_DOES_NOT_EXISTS);
    }

    [Fact, Order(3)]
    public void Update_with_no_changes_is_invalid()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.Today;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        updateUnitAddressResult.IsSuccess.Should().BeFalse();
        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)updateUnitAddressResult.Errors.First())
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.NO_CHANGES);
    }

    [Fact, Order(3)]
    public void Cannot_delete_when_it_not_been_created()
    {
        var id = Guid.Parse("d4de2559-066d-4492-8f84-712f4995b7a3");

        var unitAddressAR = new UnitAddressAR();

        var deleteResult = unitAddressAR.Delete();

        deleteResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(deleteResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.ID_NOT_SET);
    }

    [Fact, Order(3)]
    public void Delete_is_success()
    {
        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var deleteResult = unitAddressAR.Delete();

        _eventStore.Aggregates.Store(unitAddressAR);

        deleteResult.IsSuccess.Should().BeTrue();
        unitAddressAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(4)]
    public void Cannot_update_when_deleted()
    {
        var addressProjection = _eventStore.Projections.Get<AddressProjection>();

        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");
        var officialId = "d4de2559-066d-4492-8f84-712f4995b7a3";
        var accessAddressId = Guid.Parse("5bc2ad5b-8634-4b05-86b2-ea6eb10596dc");
        var status = UnitAddressStatus.Discontinued;
        string? floorName = null;
        string? suitName = null;
        var updated = DateTime.UtcNow;
        var existingAccessAddressIds = addressProjection.AccessAddressIds;

        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Update(
            officialId: officialId,
            accessAddressId: accessAddressId,
            status: status,
            floorName: floorName,
            suitName: suitName,
            updated: updated,
            existingAccessAddressIds: existingAccessAddressIds);

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(updateUnitAddressResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.CANNOT_UPDATE_DELETED);
        unitAddressAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(4)]
    public void Cannot_delete_already_deleted()
    {
        var id = Guid.Parse("9a171f9b-1d25-458e-b664-627fd15e14f6");

        var addressProjection = _eventStore.Projections.Get<AddressProjection>();
        var unitAddressAR = _eventStore.Aggregates.Load<UnitAddressAR>(id);

        var updateUnitAddressResult = unitAddressAR.Delete();

        _eventStore.Aggregates.Store(unitAddressAR);

        updateUnitAddressResult.Errors.Should().HaveCount(1);
        ((UnitAddressError)(updateUnitAddressResult.Errors.First()))
            .Code
            .Should()
            .Be(UnitAddressErrorCodes.CANNOT_DELETE_ALREADY_DELETED);
        unitAddressAR.Deleted.Should().BeTrue();
    }
}
