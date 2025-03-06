using OpenFTTH.Results;

namespace OpenFTTH.Core.Address;

public enum UnitAddressErrorCode
{
    ID_CANNOT_BE_EMPTY_GUID,
    ACCESS_ADDRESS_ID_CANNOT_BE_EMPTY_GUID,
    NOT_INITIALIZED,
    ACCESS_ADDRESS_DOES_NOT_EXISTS,
    CANNOT_UPDATE_DELETED,
    CANNOT_DELETE_ALREADY_DELETED,
    NO_CHANGES,
    ALREADY_CREATED
}

public class UnitAddressError : Error
{
    public UnitAddressErrorCode Code { get; init; }

    public UnitAddressError(UnitAddressErrorCode errorCode, string errorMsg)
        : base(errorCode.ToString() + ": " + errorMsg)
    {
        Code = errorCode;
        Metadata.Add("ErrorCode", errorCode.ToString());
    }
}
