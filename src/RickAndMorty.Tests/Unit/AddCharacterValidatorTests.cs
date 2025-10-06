using RickAndMorty.Web.Features.AddCharacters;

namespace RickAndMorty.Tests.Unit;

public sealed class AddCharacterValidatorTests
{
    private readonly AddCharacterValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        var request = new AddCharacterRequest(
            Name: "Test Character",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyName_FailsValidation(string? name)
    {
        var request = new AddCharacterRequest(
            Name: name!,
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
    }


    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        var request = new AddCharacterRequest(
            Name: new string('a', 201),
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name must not exceed 200 characters");
    }

    [Theory]
    [InlineData("Rick Sanchez")]
    [InlineData("C-137")]
    [InlineData("Rick-C-137")]
    public void Validate_NameWithLetters_PassesValidation(string name)
    {
        var request = new AddCharacterRequest(
            Name: name,
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NameWithOnlyNumbers_FailsValidation()
    {
        var request = new AddCharacterRequest(
            Name: "12345",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name must contain at least one letter");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyStatus_FailsValidation(string? status)
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: status!,
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage == "Status is required");
    }

    [Theory]
    [InlineData("Alive")]
    [InlineData("Dead")]
    [InlineData("unknown")]
    public void Validate_ValidStatus_PassesValidation(string status)
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: status,
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("alive")]
    [InlineData("ALIVE")]
    [InlineData("Invalid")]
    [InlineData("Unknown")]
    public void Validate_InvalidStatus_FailsValidation(string status)
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: status,
            Species: "Human",
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage == "Status must be 'Alive', 'Dead', or 'unknown'");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptySpecies_FailsValidation(string? species)
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: "Alive",
            Species: species!,
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Species" && e.ErrorMessage == "Species is required");
    }

    [Fact]
    public void Validate_SpeciesTooLong_FailsValidation()
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: "Alive",
            Species: new string('a', 101),
            OriginName: "Earth"
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Species" && e.ErrorMessage == "Species must not exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOriginName_FailsValidation(string? originName)
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: "Alive",
            Species: "Human",
            OriginName: originName!
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OriginName" && e.ErrorMessage == "Origin is required");
    }

    [Fact]
    public void Validate_OriginNameTooLong_FailsValidation()
    {
        var request = new AddCharacterRequest(
            Name: "Test",
            Status: "Alive",
            Species: "Human",
            OriginName: new string('a', 201)
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OriginName" && e.ErrorMessage == "Origin must not exceed 200 characters");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        var request = new AddCharacterRequest(
            Name: "",
            Status: "InvalidStatus",
            Species: "",
            OriginName: ""
        );

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
    }
}
