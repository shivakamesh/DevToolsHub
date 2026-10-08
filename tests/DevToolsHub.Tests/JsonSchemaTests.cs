using System.Text.Json.Nodes;
using DevToolsHub.Services;

namespace DevToolsHub.Tests;

public class SchemaTests
{
    private static List<SchemaError> Run(string schema, string data, params string[] extra) =>
        new JsonSchemaChecker(JsonNode.Parse(schema), extra.Select(e => JsonNode.Parse(e))).Validate(JsonNode.Parse(data));

    [Fact]
    public void ValidObject_HasNoErrors() =>
        Assert.Empty(Run("""{"type":"object","required":["a"],"properties":{"a":{"type":"integer","minimum":1}}}""", """{"a":5}"""));

    [Fact]
    public void MissingRequired_AndWrongType_AreReported()
    {
        var errors = Run("""{"type":"object","required":["a","b"],"properties":{"a":{"type":"string"}}}""", """{"a":1}""");
        Assert.Contains(errors, e => e.Message.Contains("'b'"));
        Assert.Contains(errors, e => e.Path == "$.a");
    }

    [Fact]
    public void AdditionalPropertiesFalse_RejectsExtra() =>
        Assert.Single(Run("""{"properties":{"a":{}},"additionalProperties":false}""", """{"a":1,"b":2}"""));

    [Fact]
    public void LocalRef_IsResolved() =>
        Assert.Single(Run("""{"$defs":{"n":{"type":"number"}},"properties":{"x":{"$ref":"#/$defs/n"}}}""", """{"x":"no"}"""));

    [Fact]
    public void ExternalRef_ResolvedById()
    {
        const string ext = """{"$id":"https://example.com/address.json","definitions":{"zip":{"pattern":"^\\d{5}$"}}}""";
        const string schema = """{"properties":{"zip":{"$ref":"address.json#/definitions/zip"}}}""";
        Assert.Empty(Run(schema, """{"zip":"12345"}""", ext));
        Assert.Single(Run(schema, """{"zip":"12"}""", ext));
    }

    [Fact]
    public void UnresolvableRef_Throws() =>
        Assert.Throws<InvalidOperationException>(() => Run("""{"$ref":"other.json"}""", "1"));

    [Fact]
    public void OneOf_RequiresExactlyOne() =>
        Assert.Single(Run("""{"oneOf":[{"type":"integer"},{"minimum":0}]}""", "5"));

    [Fact]
    public void IfThenElse_AppliesBranch()
    {
        const string schema = """{"if":{"properties":{"t":{"const":"a"}}},"then":{"required":["x"]},"else":{"required":["y"]}}""";
        Assert.Single(Run(schema, """{"t":"a"}"""));
        Assert.Empty(Run(schema, """{"t":"b","y":1}"""));
    }

    [Fact]
    public void DependentRequired_IsEnforced() =>
        Assert.Single(Run("""{"dependentRequired":{"card":["cvv"]}}""", """{"card":"1"}"""));

    [Fact]
    public void UnevaluatedProperties_ConsidersAllOf()
    {
        const string schema = """{"allOf":[{"properties":{"a":{}}}],"unevaluatedProperties":false}""";
        Assert.Empty(Run(schema, """{"a":1}"""));
        Assert.Single(Run(schema, """{"a":1,"b":2}"""));
    }

    [Fact]
    public void Contains_WithMinContains() =>
        Assert.Single(Run("""{"contains":{"type":"string"},"minContains":2}""", """["a",1]"""));

    [Fact]
    public void UniqueItems_DetectsDuplicates() =>
        Assert.Single(Run("""{"uniqueItems":true}""", """[1,{"a":1},{"a":1}]"""));

    [Theory]
    [InlineData("email", "a@b.co", true)]
    [InlineData("email", "nope", false)]
    [InlineData("date", "2024-02-30", false)]
    [InlineData("date-time", "2024-01-01T10:00:00Z", true)]
    [InlineData("ipv4", "10.0.0.1", true)]
    [InlineData("ipv4", "10.0.1", false)]
    [InlineData("uuid", "3f2504e0-4f89-11d3-9a0c-0305e82c3301", true)]
    public void Formats(string format, string value, bool ok) =>
        Assert.Equal(ok, JsonSchemaChecker.FormatOk(format, value));

    [Fact]
    public void IntegerKind_ForWholeNumbers()
    {
        Assert.Equal("integer", JsonSchemaChecker.Kind(JsonNode.Parse("2.0")));
        Assert.Equal("number", JsonSchemaChecker.Kind(JsonNode.Parse("2.5")));
    }
}
