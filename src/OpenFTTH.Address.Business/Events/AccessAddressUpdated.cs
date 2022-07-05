namespace OpenFTTH.Address.Business.Events;

public record AccessAddressUpdated
{
    public Guid Id { get; init; }
    public Guid? OfficialId { get; init; }
    public DateTime Updated { get; init; }
    public string MunicipalCode { get; init; }
    public Status Status { get; init; }
    public string RoadCode { get; init; }
    public string HouseNumber { get; init; }
    public string PostDistrictCode { get; init; }
    public double EastCoordinate { get; init; }
    public double NorthCoordinate { get; init; }
    public DateTime? LocationUpdated { get; init; }
    public string? TownName { get; init; }
    public string? PlotId { get; init; }
    public Guid RoadId { get; init; }

    public AccessAddressUpdated(
        Guid id,
        Guid? officialId,
        DateTime updated,
        string municipalCode,
        Status status,
        string roadCode,
        string houseNumber,
        string postDistrictCode,
        double eastCoordinate,
        double northCoordinate,
        DateTime? locationUpdated,
        string? townName,
        string? plotId,
        Guid roadId)
    {
        Id = id;
        OfficialId = officialId;
        Updated = updated;
        MunicipalCode = municipalCode;
        Status = status;
        RoadCode = roadCode;
        HouseNumber = houseNumber;
        PostDistrictCode = postDistrictCode;
        EastCoordinate = eastCoordinate;
        NorthCoordinate = northCoordinate;
        LocationUpdated = locationUpdated;
        TownName = townName;
        PlotId = plotId;
        RoadId = roadId;
    }
}