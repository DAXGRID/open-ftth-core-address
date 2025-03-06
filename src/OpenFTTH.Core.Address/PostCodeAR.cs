using OpenFTTH.Results;
using OpenFTTH.Core.Address.Events;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.Core.Address;

public class PostCodeAR : AggregateBase
{
    public string Number { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool Deleted { get; private set; }
    public DateTime? ExternalCreatedDate { get; private set; }
    public DateTime? ExternalUpdatedDate { get; private set; }

    public PostCodeAR()
    {
        Register<PostCodeCreated>(Apply);
        Register<PostCodeNameChanged>(Apply);
        Register<PostCodeDeleted>(Apply);
    }

    public Result Create(
        Guid id,
        string number,
        string name,
        DateTime? externalCreatedDate,
        DateTime? externalUpdatedDate)
    {
        var canCreateResult = CanCreateAR(id, number, name);
        if (canCreateResult.IsFailed)
        {
            return canCreateResult;
        }

        RaiseEvent(new PostCodeCreated(
                       id: id,
                       number: number,
                       name: name,
                       externalCreatedDate: externalCreatedDate,
                       externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Update(string name, DateTime? externalUpdatedDate)
    {
        var canUpdateResult = CanUpdateAR();
        if (canUpdateResult.IsFailed)
        {
            return canUpdateResult;
        }

        if (!HasChanges(newName: name, postCode: this))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NO_CHANGES,
                    "No changes to the AR doing update."));
        }

        var changeNameResult = UpdateName(name, externalUpdatedDate);
        if (changeNameResult.Errors.Count > 0)
        {
            var error = (PostCodeError)changeNameResult.Errors.First();
            if (error.Code != PostCodeErrorCodes.NO_CHANGES)
            {
                return changeNameResult;
            }
        };

        return Result.Ok();
    }

    public Result UpdateName(string name, DateTime? externalUpdatedDate)
    {
        var canUpdateResult = CanUpdateAR();
        if (canUpdateResult.IsFailed)
        {
            return canUpdateResult;
        }

        if (!IsValidName(name))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE,
                    $"{nameof(name)} cannot be null empty or whitespace."));
        }

        if (!IsNameChanged(oldName: Name, newName: name))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NO_CHANGES,
                    $"No changes to field '{nameof(Name)}'."));
        }

        RaiseEvent(
            new PostCodeNameChanged(
                id: Id,
                name: name,
                externalUpdatedDate: externalUpdatedDate));

        return Result.Ok();
    }

    public Result Delete(DateTime? externalUpdatedDate)
    {
        var canDeleteResult = CanDeleteAR();
        if (canDeleteResult.IsFailed)
        {
            return canDeleteResult;
        }

        RaiseEvent(new PostCodeDeleted(Id, externalUpdatedDate));

        return Result.Ok();
    }

    private void Apply(PostCodeCreated postCodeCreated)
    {
        Id = postCodeCreated.Id;
        Number = postCodeCreated.Number;
        Name = postCodeCreated.Name;
        ExternalCreatedDate = postCodeCreated.ExternalCreatedDate;
        ExternalUpdatedDate = postCodeCreated.ExternalUpdatedDate;
    }

    private void Apply(PostCodeNameChanged postCodeNameChanged)
    {
        Name = postCodeNameChanged.Name;
        ExternalUpdatedDate = postCodeNameChanged.ExternalUpdatedDate;
    }

    private void Apply(PostCodeDeleted postCodeDeleted)
    {
        Deleted = true;
        ExternalUpdatedDate = postCodeDeleted.ExternalUpdatedDate;
    }

    private static Result CanCreateAR(Guid id, string number, string name)
    {
        if (!IsValidInitialId(id))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.ID_CANNOT_BE_EMPTY_GUID,
                    $"{nameof(id)} cannot be empty guid."));
        }

        if (!IsValidPostalNumber(number))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NUMBER_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE,
                    $"{nameof(number)} cannot be null empty or whitespace."));
        }

        if (!IsValidName(name))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE,
                    $"{nameof(name)} cannot be null empty or whitespace."));
        }

        return Result.Ok();
    }

    private Result CanUpdateAR()
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NOT_INITIALIZED,
                    "Cannot update, before it has been initialized."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.CANNOT_UPDATE_DELETED,
                    "Cannot update when it has been deleted."));
        }

        return Result.Ok();
    }

    private Result CanDeleteAR()
    {
        if (!IsInitialized(Id))
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.NOT_INITIALIZED,
                    @$"{nameof(Id)}, being default guid is not valid, the AR has most likely not being created yet."));
        }

        if (Deleted)
        {
            return Result.Fail(
                new PostCodeError(
                    PostCodeErrorCodes.CANNOT_DELETE_ALREADY_DELETED,
                    @$"Id: '{Id}' is already deleted."));
        }

        return Result.Ok();
    }

    private static bool IsInitialized(Guid currentId)
    {
        return currentId != Guid.Empty;
    }

    private static bool IsValidInitialId(Guid id)
    {
        return id != Guid.Empty;
    }

    private static bool IsValidName(string name)
    {
        return !String.IsNullOrWhiteSpace(name);
    }

    private static bool IsValidPostalNumber(string number)
    {
        return !String.IsNullOrWhiteSpace(number);
    }

    private static bool HasChanges(string newName, PostCodeAR postCode)
    {
        return IsNameChanged(postCode.Name, newName);
    }

    private static bool IsNameChanged(string oldName, string newName)
    {
        return oldName != newName;
    }
}
