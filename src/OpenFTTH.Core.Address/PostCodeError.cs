using OpenFTTH.Results;

namespace OpenFTTH.Core.Address;

public enum PostCodeErrorCodes
{
    ID_CANNOT_BE_EMPTY_GUID,
    NUMBER_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE,
    NAME_CANNOT_BE_EMPTY_NULL_OR_WHITESPACE,
    CANNOT_UPDATE_DELETED,
    CANNOT_DELETE_ALREADY_DELETED,
    NO_CHANGES,
    NOT_INITIALIZED
}

public class PostCodeError : Error
{
    public PostCodeErrorCodes Code { get; init; }

    public PostCodeError(PostCodeErrorCodes errorCode, string errorMsg)
        : base(errorCode.ToString() + ": " + errorMsg)
    {
        Code = errorCode;
        Metadata.Add("ErrorCode", errorCode.ToString());
    }
}
