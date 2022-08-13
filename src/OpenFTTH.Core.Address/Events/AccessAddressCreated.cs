namespace OpenFTTH.Core.Address.Events;

public sealed record AccessAddressCreated
{
    public Guid Id { get; init; }
    public string? OfficialId { get; init; }
    public DateTime Created { get; init; }
    public DateTime Updated { get; init; }
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

    public AccessAddressCreated(
        Guid id,
        string? officialId,
        DateTime created,
        DateTime updated,
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
        bool pendingOfficial)
    {
        Id = id;
        OfficialId = officialId;
        Created = created;
        Updated = updated;
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
    }
}
