namespace OpenFTTH.Core.Address.Events;

public sealed record AccessAddressCreated
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public DateTime? ExternalCreatedDate { get; init; }
    public DateTime? ExternalUpdatedDate { get; init; }
    public string MunicipalCode { get; init; }
    public AccessAddressStatus Status { get; init; }
    public string RoadCode { get; init; }
    public string HouseNumber { get; init; }
    public Guid PostCodeId { get; init; }
    public double EastCoordinate { get; init; }
    public double NorthCoordinate { get; init; }
    public string? TownName { get; init; }
    public string? PlotId { get; init; }
    public Guid RoadId { get; init; }
    public bool PendingOfficial { get; init; }
    public string? SupplementaryTownName { get; init; }

    public AccessAddressCreated(
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
        string? townName,
        string? plotId,
        Guid roadId,
        bool pendingOfficial,
        string? supplementaryTownName)
    {
        Id = id;
        ExternalId = externalId;
        ExternalCreatedDate = externalCreatedDate;
        ExternalUpdatedDate = externalUpdatedDate;
        MunicipalCode = municipalCode;
        Status = status;
        RoadCode = roadCode;
        HouseNumber = houseNumber;
        PostCodeId = postCodeId;
        EastCoordinate = eastCoordinate;
        NorthCoordinate = northCoordinate;
        TownName = townName;
        PlotId = plotId;
        RoadId = roadId;
        PendingOfficial = pendingOfficial;
        SupplementaryTownName = supplementaryTownName;
    }
}
