using OpenFTTH.EventSourcing;
using Xunit.Extensions.Ordering;

namespace OpenFTTH.Core.Address.Tests;

public record CreatePostCodeExampleData
{
    public Guid Id { get; init; }
    public string Number { get; init; }
    public string Name { get; init; }
    public DateTime ExternalCreatedDate { get; init; }
    public DateTime ExternalUpdatedDate { get; init; }

    public CreatePostCodeExampleData(
        Guid id,
        string number,
        string name,
        DateTime externalCreatedDate,
        DateTime externalUpdatedDate)
    {
        Id = id;
        Number = number;
        Name = name;
        ExternalCreatedDate = externalCreatedDate;
        ExternalUpdatedDate = externalUpdatedDate;
    }
}

[Order(0)]
public class PostCodeTests
{
    private readonly IEventStore _eventStore;

    public PostCodeTests(IEventStore eventStore)
    {
        _eventStore = eventStore;
        _eventStore.ScanForProjections();
    }

    public static IEnumerable<object[]> ExamplePostCodeValues()
    {
        yield return new object[]
        {
            new CreatePostCodeExampleData(
                id: Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002"),
                number: "7000",
                name: "Fredericia",
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow)
        };

        yield return new object[]
        {
            new CreatePostCodeExampleData(
                id: Guid.Parse("7460bb7e-9d72-45f0-a5fa-6a92b7bb30dc"),
                number: "8660",
                name: "Skanderborg",
                externalCreatedDate: DateTime.UtcNow,
                externalUpdatedDate: DateTime.UtcNow)
        };
    }

    [Theory, Order(1)]
    [MemberData(nameof(ExamplePostCodeValues))]
    public void Create_is_success(CreatePostCodeExampleData postCodeExampleData)
    {
        ArgumentNullException.ThrowIfNull(postCodeExampleData);

        var postCodeAR = new PostCodeAR();

        var createPostCodeResult = postCodeAR.Create(
            id: postCodeExampleData.Id,
            number: postCodeExampleData.Number,
            name: postCodeExampleData.Name,
            externalCreatedDate: postCodeExampleData.ExternalCreatedDate,
            externalUpdatedDate: postCodeExampleData.ExternalUpdatedDate);

        _eventStore.Aggregates.Store(postCodeAR);

        createPostCodeResult.IsSuccess.Should().BeTrue();
        postCodeAR.Id.Should().Be(postCodeExampleData.Id);
        postCodeAR.Number.Should().Be(postCodeExampleData.Number);
        postCodeAR.Name.Should().Be(postCodeExampleData.Name);
        postCodeAR.ExternalUpdatedDate.Should().Be(postCodeExampleData.ExternalUpdatedDate);
    }

    [Fact, Order(1)]
    public void Create_default_id_is_invalid()
    {
        var id = Guid.Empty;
        var number = "7000";
        var name = "Fredericia";
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = new PostCodeAR();

        var createPostCodeResult = postCodeAR.Create(
            id: id,
            number: number,
            name: name,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createPostCodeResult.IsSuccess.Should().BeFalse();
        createPostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)createPostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.ID_CANNOT_BE_EMPTY_GUID);
    }

    [Theory, Order(1)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_number_empty_or_whitespace_is_invalid(string number)
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var name = "Fredericia";
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = new PostCodeAR();

        var createPostCodeResult = postCodeAR.Create(
            id: id,
            number: number,
            name: name,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createPostCodeResult.IsSuccess.Should().BeFalse();
        createPostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)createPostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NUMBER_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE);
    }

    [Theory, Order(1)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_name_empty_or_whitespace_invalid(string name)
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var number = "7000";
        var externalCreatedDate = DateTime.UtcNow;
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = new PostCodeAR();

        var createPostCodeResult = postCodeAR.Create(
            id: id,
            number: number,
            name: name,
            externalCreatedDate: externalCreatedDate,
            externalUpdatedDate: externalUpdatedDate);

        createPostCodeResult.IsSuccess.Should().BeFalse();
        createPostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)createPostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE);
    }

    [Fact, Order(2)]
    public void Update_is_success()
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var number = "7000";
        var name = "New Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.Update(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(postCodeAR);

        updatePostCodeResult.IsSuccess.Should().BeTrue();
        postCodeAR.Id.Should().Be(id);
        postCodeAR.Name.Should().Be(name);
        postCodeAR.Number.Should().Be(number);
        postCodeAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
    }

    [Fact, Order(2)]
    public void Cannot_update_AR_when_the_AR_is_not_initialized()
    {
        var name = "New Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = new PostCodeAR();

        var updatePostCodeResult = postCodeAR.Update(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeFalse();
        updatePostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updatePostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NOT_INITIALIZED);
    }

    [Fact, Order(2)]
    public void Update_name_is_empty_null_or_whitespace_is_invalid()
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var name = String.Empty;
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.Update(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeFalse();
        updatePostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updatePostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE);
    }

    [Fact, Order(2)]
    public void Update_is_invalid_when_no_changes()
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var name = "New Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.Update(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeFalse();
        updatePostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updatePostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NO_CHANGES);
    }

    [Fact, Order(3)]
    public void Name_changed_is_success()
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var number = "7000";
        var name = "NewNew Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.UpdateName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeTrue();
        postCodeAR.Id.Should().Be(id);
        postCodeAR.Name.Should().Be(name);
        postCodeAR.Number.Should().Be(number);
        postCodeAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
    }

    [Fact, Order(3)]
    public void Change_name_is_invalid_when_no_changes()
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var name = "New Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.UpdateName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeFalse();
        updatePostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updatePostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NO_CHANGES);
    }

    [Theory, Order(3)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Update_name_empty_or_whitespace_is_invalid(string name)
    {
        var id = Guid.Parse("1acef11e-fc4e-11ec-b939-0242ac120002");
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var changeNameResult = postCodeAR.UpdateName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        changeNameResult.IsSuccess.Should().BeFalse();
        changeNameResult.Errors.Should().HaveCount(1);
        ((PostCodeError)changeNameResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE);
    }

    [Fact, Order(3)]
    public void Cannot_change_name_not_initialized_AR()
    {
        var postCodeAR = new PostCodeAR();
        var externalUpdatedDate = DateTime.UtcNow;

        var changeNameResult = postCodeAR.UpdateName("New awesome city name", externalUpdatedDate);

        changeNameResult.IsSuccess.Should().BeFalse();
        changeNameResult.Errors.Should().HaveCount(1);
        ((PostCodeError)changeNameResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NOT_INITIALIZED);
    }

    [Fact, Order(4)]
    public void Cannot_delete_post_code_that_has_not_yet_been_created()
    {
        var id = Guid.Parse("53d46647-edb3-428e-8063-b25e1009029e");
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);
        var deleteResult = postCodeAR.Delete(externalUpdatedDate: externalUpdatedDate);

        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().HaveCount(1);
        ((PostCodeError)deleteResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.NOT_INITIALIZED);
        postCodeAR.Deleted.Should().BeFalse();
    }

    [Fact, Order(4)]
    public void Delete_is_successful()
    {
        var id = Guid.Parse("7460bb7e-9d72-45f0-a5fa-6a92b7bb30dc");
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var deleteResult = postCodeAR.Delete(externalUpdatedDate: externalUpdatedDate);

        _eventStore.Aggregates.Store(postCodeAR);

        deleteResult.IsSuccess.Should().BeTrue();
        postCodeAR.Deleted.Should().BeTrue();
        postCodeAR.ExternalUpdatedDate.Should().Be(externalUpdatedDate);
    }

    [Fact, Order(5)]
    public void Cannot_update_deleted()
    {
        var id = Guid.Parse("7460bb7e-9d72-45f0-a5fa-6a92b7bb30dc");
        var name = "New Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updateResult = postCodeAR.Update(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updateResult.IsSuccess.Should().BeFalse();
        updateResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updateResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.CANNOT_UPDATE_DELETED);
        postCodeAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(5)]
    public void Cannot_delete_when_already_deleted()
    {
        var id = Guid.Parse("7460bb7e-9d72-45f0-a5fa-6a92b7bb30dc");
        var updated = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var deleteResult = postCodeAR.Delete(externalUpdatedDate: updated);

        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().HaveCount(1);
        ((PostCodeError)deleteResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.CANNOT_DELETE_ALREADY_DELETED);
        postCodeAR.Deleted.Should().BeTrue();
    }

    [Fact, Order(5)]
    public void Cannot_cahnge_name_when_already_deleted()
    {
        var id = Guid.Parse("7460bb7e-9d72-45f0-a5fa-6a92b7bb30dc");
        var name = "NewNew Fredericia";
        var externalUpdatedDate = DateTime.UtcNow;

        var postCodeAR = _eventStore.Aggregates.Load<PostCodeAR>(id);

        var updatePostCodeResult = postCodeAR.UpdateName(
            name: name,
            externalUpdatedDate: externalUpdatedDate);

        updatePostCodeResult.IsSuccess.Should().BeFalse();
        updatePostCodeResult.Errors.Should().HaveCount(1);
        ((PostCodeError)updatePostCodeResult.Errors.First())
            .Code
            .Should()
            .Be(PostCodeErrorCodes.CANNOT_UPDATE_DELETED);
        postCodeAR.Deleted.Should().BeTrue();
    }
}
