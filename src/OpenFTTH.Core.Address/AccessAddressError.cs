using OpenFTTH.Results;

namespace OpenFTTH.Core.Address;

public enum AccessAddressErrorCode
{
    ID_CANNOT_BE_EMPTY_GUID,
    ROAD_DOES_NOT_EXIST,
    POST_CODE_DOES_NOT_EXIST,
    CANNOT_UPDATE_DELETED,
    CANNOT_DELETE_ALREADY_DELETED,
    NO_CHANGES,
    ALREADY_CREATED,
    NOT_INITIALIZED
}

public class AccessAddressError : Error
{
    public AccessAddressErrorCode Code { get; init; }

    public AccessAddressError(AccessAddressErrorCode errorCode, string errorMsg)
        : base(errorCode.ToString() + ": " + errorMsg)
    {
        Code = errorCode;
        Metadata.Add("ErrorCode", errorCode.ToString());
    }
}
