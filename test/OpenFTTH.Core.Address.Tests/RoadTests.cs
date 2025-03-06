using OpenFTTH.EventSourcing;
using Xunit.Extensions.Ordering;

namespace OpenFTTH.Core.Address.Tests;

public record CreateRoadAddressExampleData
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public string Name { get; init; }
    public RoadStatus Status { get; init; }
    public DateTime ExternalCreatedDate { get; init; }
    public DateTime ExternalUpdatedDate { get; init; }

    public CreateRoadAddressExampleData(
        Guid id,
        string? externalId,
        string name,
        RoadStatus status,
        DateTime externalCreatedDate,
        DateTime externalUpdatedDate)
    {
        Id = id;
        ExternalId = externalId;
        Name = name;
        Status = status;
        ExternalCreatedDate = externalCreatedDate;
        ExternalUpdatedDate = externalUpdatedDate;
    }
}

[Order(0)]
public class RoadTests
{
    private readonly IEventStore _eventStore;

    public RoadTests(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public static IEnumerable<object[]> ExampleCreateValues()
    {
        yield return new object[]
        {
            new CreateRoadAddressExampleData(
                id: Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5"),
                externalId: "F12345",
                name: "Pilevej",
                status: RoadStatus.Effective,
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow)
        };

        yield return new object[]
        {
            new CreateRoadAddressExampleData(
                id: Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073"),
                externalId: "A12345",
                name: "Blomsterhaven",
                status: RoadStatus.Temporary,
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow)
        };
    }

    [Theory, Order(1)]
    [MemberData(nameof(ExampleCreateValues))]
    public void Create_is_success(CreateRoadAddressExampleData roadExampleData)
    {
        ArgumentNullException.ThrowIfNull(roadExampleData);

        var roadAR = new RoadAR();
        var createRoadResult = roadAR.Create(
            id: roadExampleData.Id,
            externalId: roadExampleData.ExternalId,
            name: roadExampleData.Name,
            status: roadExampleData.Status,
            externalCreatedDate: roadExampleData.ExternalCreatedDate,
            externalUpdatedDate: roadExampleData.ExternalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        createRoadResult.IsSuccess.Should().BeTrue();
        roadAR.Id.Should().Be(roadExampleData.Id);
        roadAR.ExternalId.Should().Be(roadExampleData.ExternalId);
        roadAR.Name.Should().Be(roadExampleData.Name);
        roadAR.Status.Should().Be(roadExampleData.Status);
        roadAR.ExternalCreatedDate.Should().Be(roadExampleData.ExternalCreatedDate);
        roadAR.ExternalUpdatedDate.Should().Be(roadExampleData.ExternalUpdatedDate);
    }

    [Fact, Order(1)]
    public void Create_default_id_is_invalid()
    {
        var id = Guid.Empty;
        var externalId = "F12345";
        var name = "Pilevej";
        var status = RoadStatus.Effective;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();
        var createRoadResult = roadAR.Create(
            id: id,
            externalId: externalId,
            name: name,
            status: status,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createRoadResult.IsFailed.Should().BeTrue();
        createRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)createRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.ID_CANNOT_BE_EMPTY_GUID);
    }

    [Theory, Order(1)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_empty_or_whitespace_external_id_is_invalid(string externalId)
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var name = "Pilevej";
        var status = RoadStatus.Effective;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();
        var createRoadResult = roadAR.Create(
            id: id,
            externalId: externalId,
            name: name,
            status: status,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createRoadResult.IsFailed.Should().BeTrue();
        createRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)createRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.EXTERNAL_ID_CANNOT_BE_WHITE_SPACE_OR_NULL);
    }

    [Fact, Order(2)]
    public void Cannot_create_when_it_has_already_been_created()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F12345";
        var name = "Pilevej";
        var status = RoadStatus.Effective;
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var createRoadResult = roadAR.Create(
            id: id,
            externalId: externalId,
            name: name,
            status: status,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createRoadResult.IsFailed.Should().BeTrue();
        createRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)createRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.ALREADY_CREATED);
    }

    [Fact, Order(2)]
    public void Update_is_success()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F123456";
        var name = "Kolding 2";
        var status = RoadStatus.Temporary;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Update(
            externalId: externalId,
            name: name,
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        updateRoadResult.IsSuccess.Should().BeTrue();
        roadAR.Id.Should().Be(id);
        roadAR.ExternalId.Should().Be(externalId);
        roadAR.Name.Should().Be(name);
        roadAR.Status.Should().Be(status);
        roadAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
    }

    [Fact, Order(2)]
    public void Update_is_invalid_not_created_yet()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F123456";
        var name = "Kolding 2";
        var status = RoadStatus.Temporary;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();

        var updateRoadResult = roadAR.Update(
            externalId: externalId,
            name: name,
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(3)]
    public void Update_is_invalid_when_no_changes()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F123456";
        var name = "Kolding 2";
        var status = RoadStatus.Temporary;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Update(
            externalId: externalId,
            name: name,
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NO_CHANGES);
    }

    [Fact, Order(4)]
    public void Can_change_name()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var name = "Kolding x";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.ChangeName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        updateRoadResult.IsSuccess.Should().BeTrue();

        // It is important that the name is now changed and the external updated date.
        reloadedRoadAR.Name.Should().Be(name);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);

        // It is important that the function did not change the other fields.
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.ExternalId.Should().Be(roadAR.ExternalId);
        reloadedRoadAR.Status.Should().Be(roadAR.Status);
    }

    [Fact, Order(4)]
    public void Can_change_external_id()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F123456789";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateExternalId(
            externalId: externalId,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        updateRoadResult.IsSuccess.Should().BeTrue();

        // It is important that the external id is now changed and the external updated date.
        reloadedRoadAR.ExternalId.Should().Be(externalId);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);

        // It is important that the function did not change the other fields.
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.Name.Should().Be(roadAR.Name);
        reloadedRoadAR.Status.Should().Be(roadAR.Status);
    }

    [Fact, Order(4)]
    public void Can_change_status()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var status = RoadStatus.Effective;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateStatus(
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        updateRoadResult.IsSuccess.Should().BeTrue();

        // It's important that the status is now changed and the external updated date.
        reloadedRoadAR.Status.Should().Be(status);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);

        // It is important that the function did not change the other fields.
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.ExternalId.Should().Be(roadAR.ExternalId);
        reloadedRoadAR.Name.Should().Be(roadAR.Name);
    }

    [Fact, Order(4)]
    public void Cannot_change_name_when_AR_is_not_created()
    {
        var id = Guid.Parse("e6768e82-2f1d-4743-93ce-30fa5ebee118");
        var name = "Kolding x";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();

        var updateRoadResult = roadAR.ChangeName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(4)]
    public void Cannot_change_external_id_when_AR_is_not_created()
    {
        var id = Guid.Parse("e6768e82-2f1d-4743-93ce-30fa5ebee118");
        var externalId = "abc1234";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();

        var updateRoadResult = roadAR.UpdateExternalId(
            externalId: externalId,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(4)]
    public void Cannot_change_status_when_AR_is_not_created()
    {
        var id = Guid.Parse("e6768e82-2f1d-4743-93ce-30fa5ebee118");
        var status = RoadStatus.Effective;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = new RoadAR();

        var updateRoadResult = roadAR.UpdateStatus(
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NOT_INITIALIZED);
    }

    [Fact, Order(5)]
    public void No_changes_to_name_returns_no_changes_result()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var name = "Kolding x";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.ChangeName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NO_CHANGES);

        // Load AR again to verify that no changes has been made
        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.ExternalId.Should().Be(roadAR.ExternalId);
        reloadedRoadAR.Name.Should().Be(roadAR.Name);
        reloadedRoadAR.Status.Should().Be(roadAR.Status);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(roadAR.ExternalUpdatedDate);
    }

    [Fact, Order(5)]
    public void No_changes_to_external_id_returns_no_changes_result()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var externalId = "F123456789";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateExternalId(
            externalId: externalId,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NO_CHANGES);

        // Load AR again to verify that no changes has been made
        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.ExternalId.Should().Be(roadAR.ExternalId);
        reloadedRoadAR.Name.Should().Be(roadAR.Name);
        reloadedRoadAR.Status.Should().Be(roadAR.Status);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(roadAR.ExternalUpdatedDate);
    }

  [Fact, Order(5)]
    public void No_changes_to_status_returns_no_changes_result()
    {
        var id = Guid.Parse("d309aa7b-81a3-4708-b1f5-e8155c29e5b5");
        var status = RoadStatus.Effective;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateStatus(
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NO_CHANGES);

        // Load AR again to verify that no changes has been made
        var reloadedRoadAR = _eventStore.Aggregates.Load<RoadAR>(id);
        reloadedRoadAR.Id.Should().Be(roadAR.Id);
        reloadedRoadAR.ExternalId.Should().Be(roadAR.ExternalId);
        reloadedRoadAR.Name.Should().Be(roadAR.Name);
        reloadedRoadAR.Status.Should().Be(roadAR.Status);
        reloadedRoadAR.ExternalUpdatedDate.Should().Be(roadAR.ExternalUpdatedDate);
    }

    [Fact, Order(6)]
    public void Delete_is_invalid_not_created()
    {
        var id = Guid.NewGuid();
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Delete(externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsFailed.Should().BeTrue();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.NOT_INITIALIZED);
        roadAR.Deleted.Should().BeFalse();
    }

    [Fact, Order(6)]
    public void Delete_is_success()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Delete(externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(roadAR);

        updateRoadResult.IsSuccess.Should().BeTrue();
        roadAR.Deleted.Should().BeTrue();
        roadAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
    }

    [Fact, Order(7)]
    public void Cannot_update_deleted()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var externalId = "F12345";
        var name = "Kolding";
        var status = RoadStatus.Effective;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Update(
            name: name,
            externalId: externalId,
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsSuccess.Should().BeFalse();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.CANNOT_UPDATE_DELETED);
        roadAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(7)]
    public void Cannot_change_status_when_AR_is_deleted()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var status = RoadStatus.Effective;
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateStatus(
            status: status,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsSuccess.Should().BeFalse();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.CANNOT_UPDATE_DELETED);
        roadAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(7)]
    public void Cannot_change_external_id_when_AR_is_deleted()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var externalId = "F12345";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.UpdateExternalId(
            externalId: externalId,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsSuccess.Should().BeFalse();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.CANNOT_UPDATE_DELETED);
        roadAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(7)]
    public void Cannot_change_name_when_AR_is_deleted()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var name = "Kolding x";
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.ChangeName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updateRoadResult.IsSuccess.Should().BeFalse();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.CANNOT_UPDATE_DELETED);
        roadAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(7)]
    public void Cannot_delete_already_deleted()
    {
        var id = Guid.Parse("40fc2260-870c-413e-953e-5b17daa57073");
        var externalUpdatedDate = DateTime.UtcNow;

        var roadAR = _eventStore.Aggregates.Load<RoadAR>(id);

        var updateRoadResult = roadAR.Delete(externalUpdatedDate);

        updateRoadResult.IsSuccess.Should().BeFalse();
        updateRoadResult.Errors.Should().HaveCount(1);
        ((RoadError)updateRoadResult.Errors.First())
            .Code
            .Should()
            .Be(RoadErrorCode.CANNOT_DELETE_ALREADY_DELETED);
        roadAR.Deleted.Should().BeTrue();
    }
}
