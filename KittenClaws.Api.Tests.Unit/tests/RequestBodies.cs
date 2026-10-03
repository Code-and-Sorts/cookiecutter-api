namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

public static class RequestBodies
{
    private static readonly (string Name, string Value)[] SystemFields =
    [
        ("id", "\"6f1c2a3b-4d5e-4f60-8a7b-000000000000\""),
        ("isDeleted", "false"),
        ("createdTimestamp", "\"2026-01-15T10:00:00.000Z\""),
        ("updatedTimestamp", "\"2026-01-15T10:00:00.000Z\""),
        ("createdBy", "\"sample\""),
        ("updatedBy", "\"sample\""),
    ];

    public static string With(string body, string name, string? value)
    {
        var properties = JsonNode.Parse(body)!.AsObject();
        properties.Remove(name);
        if (value != null)
        {
            properties[name] = JsonNode.Parse(value);
        }
        return properties.ToJsonString();
    }

    // Bodies an operation must reject: not an object, server-set, unknown or refused properties, rejected values, missing required ones.
    public static TheoryData<string> Invalid(string valid, string[] required, string[] refused, (string Name, string Value)[] rejected)
    {
        var properties = JsonNode.Parse(valid)!.AsObject();
        IEnumerable<string> bodies =
        [
            "", "[]", "null", "\"text\"", "{not json", With(valid, "notAField", "1"),
            .. SystemFields.Select(field => With(valid, field.Name, field.Value)),
            .. refused.Select(name => With(valid, name, "\"x\"")),
            .. rejected.Where(field => properties.ContainsKey(field.Name)).Select(field => With(valid, field.Name, field.Value)),
            .. required.Select(name => With(valid, name, null)),
        ];
        return new TheoryData<string>(bodies.Distinct());
    }

    // Bodies an operation must accept: valid with one property set to an edge value.
    public static TheoryData<string> WithEdgeValues(string valid, (string Name, string Value)[] boundaries)
    {
        var properties = JsonNode.Parse(valid)!.AsObject();
        return new TheoryData<string>(boundaries.Where(field => properties.ContainsKey(field.Name)).Select(field => With(valid, field.Name, field.Value)).Distinct());
    }
}
