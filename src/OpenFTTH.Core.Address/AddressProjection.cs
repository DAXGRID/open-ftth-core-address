using OpenFTTH.Core.Address.Events;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.Core.Address;

public class AddressProjection : ProjectionBase
{
    public Dictionary<string, Guid> AccessAddressOfficialIdToId { get; } = new();
    public HashSet<Guid> AccessAddressIds { get; } = new();

    public Dictionary<string, Guid> RoadOfficialIdIdToId { get; } = new();
    public HashSet<Guid> RoadIds => RoadOfficialIdIdToId.Values.ToHashSet();

    public Dictionary<string, Guid> PostCodeNumberToId { get; } = new();
    public Dictionary<Guid, string> PostCodeIdToNumber { get; } = new();

    public AddressProjection()
    {
        ProjectEvent<AccessAddressCreated>(
            (@event) =>
            {
                var accessAddressCreated = (AccessAddressCreated)@event.Data;
                if (accessAddressCreated.OfficialId is not null)
                {
                    AccessAddressOfficialIdToId.Add(
                        accessAddressCreated.OfficialId, accessAddressCreated.Id);
                }

                // This is a bit special since we allow access addresses to be created
                // without 'officialIds' so we cannot use the values from the
                // official id to project an internal id lookup table,
                // so we have to keep a seperate lookup table in sync.
                AccessAddressIds.Add(accessAddressCreated.Id);
            });

        ProjectEvent<RoadCreated>(
            (@event) =>
            {
                var roadCreated = (RoadCreated)@event.Data;
                RoadOfficialIdIdToId.Add(roadCreated.OfficialId, roadCreated.Id);
            });

        ProjectEvent<PostCodeCreated>(
            (@event) =>
            {
                var postCodeCreated = (PostCodeCreated)@event.Data;
                PostCodeNumberToId.Add(postCodeCreated.Number, postCodeCreated.Id);
                PostCodeIdToNumber.Add(postCodeCreated.Id, postCodeCreated.Number);
            });

        ProjectEvent<PostCodeDeleted>(
            (@event) =>
            {
                var postCodeDeleted = (PostCodeDeleted)@event.Data;

                var postCodeNumber = PostCodeIdToNumber[postCodeDeleted.Id];
                var postCodeId = PostCodeNumberToId[postCodeNumber];

                PostCodeNumberToId.Remove(postCodeNumber);
                PostCodeIdToNumber.Remove(postCodeId);
            }
        );
    }
}
