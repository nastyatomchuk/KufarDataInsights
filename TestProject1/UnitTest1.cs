using KufarDataInsights;
using System;
using System.Collections.Generic;
using Xunit;

public class ApartmentFinderTests
{
    [Fact]
    public void FindApartmentsInPolygon_ShouldReturnApartmentsInsidePolygon()
    {
        // Arrange
        var apartments = new List<Apartment>
        {
            new Apartment(1, 1),
            new Apartment(2, 2),
            // new Apartment(3, 3) // Должен быть вне полигона
        };

        var polygon = new List<(double, double)>
        {
            (0, 0),
            (0, 4),
            (4, 4),
            (4, 0)
        };

        var finder = new ApartmentFinder();

        // Act
        var result = finder.FindApartmentsInPolygon(apartments, polygon);

        // Assert
        Assert.Contains(result, a => a.Latitude == 1 && a.Longitude == 1);
        Assert.Contains(result, a => a.Latitude == 2 && a.Longitude == 2);
        Assert.DoesNotContain(result, a => a.Latitude == 3 && a.Longitude == 3);
    }

    [Fact]
    public void FindApartmentsInPolygon_ShouldThrowException_WhenLessThanThreePoints()
    {
        // Arrange
        var finder = new ApartmentFinder();
        var polygon = new List<(double, double)>
        {
            (0, 0),
            (0, 1)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => finder.FindApartmentsInPolygon(new List<Apartment>(), polygon));
    }
}