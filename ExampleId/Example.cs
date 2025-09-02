using System;
using LeanCode.DomainModels.Ids;

namespace Example;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct AggregateId;

public class Service
{
    public AggregateId Parse(Guid guidId)
    {
        AggregateId.TryParse(guidId, out var id);
        return id;
    }
}
