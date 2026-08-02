using FluentAssertions;
using RideAway.Application.Services;

namespace RideAway.Tests.Application.Services;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ShouldProduceSaltAndHashSeparatedByColon()
    {
        var result = PasswordHasher.Hash("correct horse battery staple");

        result.Should().Contain(":");
        result.Split(':').Length.Should().Be(2);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_ForCorrectPassword()
    {
        var hash = PasswordHasher.Hash("P@ssw0rd!");

        PasswordHasher.Verify("P@ssw0rd!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForWrongPassword()
    {
        var hash = PasswordHasher.Hash("P@ssw0rd!");

        PasswordHasher.Verify("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForTamperedHash()
    {
        var hash = PasswordHasher.Hash("P@ssw0rd!");
        var parts = hash.Split(':');
        var tamperedHash = $"{parts[0]}:{(parts[1][0] == 'A' ? 'B' : 'A') + parts[1][1..]}";

        PasswordHasher.Verify("P@ssw0rd!", tamperedHash).Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForMalformedStoredHash()
    {
        PasswordHasher.Verify("P@ssw0rd!", "not-a-valid-hash").Should().BeFalse();
    }

    [Fact]
    public void Hash_ShouldProduceUniqueSalt_PerCall()
    {
        PasswordHasher.Hash("same-password").Should().NotBe(PasswordHasher.Hash("same-password"));
    }
}
