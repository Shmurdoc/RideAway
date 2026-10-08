using System.Text.Json;
using FluentAssertions;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;

namespace RideAway.Tests.Domain.Entities;

/// <summary>
/// Guards against credentials leaking through API responses. These are regression
/// tests: the entity graph used to be returned straight from controllers, which
/// serialised PasswordHash and the whole User navigation graph.
/// </summary>
public class SerializationSafetyTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void User_ShouldNeverSerializePasswordHash()
    {
        var user = new User
        {
            Name = "Rider One",
            Email = "rider@example.com",
            PasswordHash = "c2FsdA==:aGFzaA==",
            Role = UserRole.Rider
        };

        var json = JsonSerializer.Serialize(user, Options);

        json.Should().NotContain("PasswordHash");
        json.Should().NotContain("passwordHash");
        json.Should().NotContain("c2FsdA==");
        json.Should().Contain("rider@example.com");
    }

    [Fact]
    public void User_ShouldNotSerializeNestedObjects()
    {
        var user = new User
        {
            Email = "driver@example.com",
            Role = UserRole.Driver,
            Vehicle = new Vehicle { PlateNumber = "ABC123", Model = "Model S" }
        };

        var json = JsonSerializer.Serialize(user, Options);

        // Without this, a lazily-loaded Vehicle (and anything hanging off it) would
        // end up in the response.
        json.Should().NotContain("Vehicle");
    }

    [Fact]
    public void Ride_ShouldNeverSerializeItsNavigationProperties()
    {
        var rider = new User { Email = "rider@example.com", PasswordHash = "c2FsdA==:aGFzaA==" };
        var driver = new User { Email = "driver@example.com", PasswordHash = "c2FsdA==:aGFzaA==" };

        var ride = new Ride("1 Main St", "2 Main St", 120m)
        {
            RiderId = rider.Id,
            DriverId = driver.Id,
            Rider = rider,
            Driver = driver,
            Status = RideStatus.Completed
        };

        var json = JsonSerializer.Serialize(ride, Options);

        json.Should().NotContain("PasswordHash");
        json.Should().NotContain("rider@example.com");
        json.Should().NotContain("driver@example.com");
        json.Should().NotContain("\"Rider\":");
        json.Should().NotContain("\"Driver\":");

        // The ride's own data is still present.
        json.Should().Contain("1 Main St");
        json.Should().Contain("120");
    }
}
