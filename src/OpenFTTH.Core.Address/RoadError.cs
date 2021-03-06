using FluentResults;

namespace OpenFTTH.Core.Address;

public enum RoadErrorCode
{
    ID_CANNOT_BE_EMPTY_GUID,
    OFFICIAL_ID_CANNOT_BE_WHITE_SPACE_OR_NULL,
    CANNOT_UPDATE_DELETED,
    CANNOT_DELETE_ALREADY_DELETED,
    NO_CHANGES
}

public class RoadError : Error
{
    public RoadErrorCode Code { get; init; }

    public RoadError(RoadErrorCode errorCode, string errorMsg)
        : base(errorCode.ToString() + ": " + errorMsg)
    {
        Code = errorCode;
        Metadata.Add("ErrorCode", errorCode.ToString());
    }
}
