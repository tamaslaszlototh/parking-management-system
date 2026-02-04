using FluentAssertions;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;

namespace ParkingManagementSystem.Tests.AddParkingSpot;

public class AddParkingSpotTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateParkingSpot()
    {
        // Arrange
        var name = ParkingSpotName.Create("A-101");
        var description = ParkingSpotDescription.Create("Ground floor, near entrance");
        var id = Guid.NewGuid();
        var state = ParkingSpotState.Dedicated;

        // Act
        var parkingSpot = ParkingSpot.Create(name, description, state, id);

        // Assert
        parkingSpot.Should().NotBeNull();
        parkingSpot.Id.Should().Be(id);
        parkingSpot.Name.Should().Be(name);
        parkingSpot.Description.Should().Be(description);
        parkingSpot.State.Should().Be(state);
    }

    [Fact]
    public void Create_WithDefaultState_ShouldBeActive()
    {
        // Arrange
        var name = ParkingSpotName.Create("A-101");
        var description = ParkingSpotDescription.Create("Ground floor");

        // Act
        var parkingSpot = ParkingSpot.Create(name, description);

        // Assert
        parkingSpot.State.Should().Be(ParkingSpotState.Active);
    }

    [Fact]
    public void Create_WithoutProvidedId_ShouldGenerateNewId()
    {
        // Arrange
        var name = ParkingSpotName.Create("A-101");
        var description = ParkingSpotDescription.Create("Ground floor");

        // Act
        var parkingSpot = ParkingSpot.Create(name, description);

        // Assert
        parkingSpot.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(ParkingSpotState.Active)]
    [InlineData(ParkingSpotState.Deactivated)]
    [InlineData(ParkingSpotState.Dedicated)]
    public void Create_WithValidState_ShouldSetCorrectState(ParkingSpotState state)
    {
        // Arrange
        var name = ParkingSpotName.Create("A-101");
        var description = ParkingSpotDescription.Create("Ground floor");

        // Act
        var parkingSpot = ParkingSpot.Create(name, description, state);

        // Assert
        parkingSpot.State.Should().Be(state);
    }
}