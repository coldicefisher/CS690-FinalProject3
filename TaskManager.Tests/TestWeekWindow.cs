using Xunit;
using TaskManager;
using System;

public class WeekWindowTests
{


    [Fact]
    public void Constructor_NormalizesStartDate() {
        var input = new DateTime(2024, 3, 5, 14, 30, 0);

        var window = new WeekWindow(input);

        Assert.Equal(new DateTime(2024, 3, 5), window.Start);
    }


    [Fact]
    public void Constructor_SetsEndToSixDaysAfterStart() {
        var input = new DateTime(2024, 3, 5);

        var window = new WeekWindow(input);

        Assert.Equal(new DateTime(2024, 3, 11), window.End);
    }

    [Fact]
    public void Equals_ReturnsTrue_ForSameStartDate() {
        var w1 = new WeekWindow(new DateTime(2024, 3, 5, 8, 0, 0));
        var w2 = new WeekWindow(new DateTime(2024, 3, 5, 20, 0, 0));

        Assert.True(w1.Equals(w2));
        Assert.True(w1 == w1); // reference equality still valid
    }


    [Fact]
    public void Equals_ReturnsFalse_ForDifferentStartDates() {
        var w1 = new WeekWindow(new DateTime(2024, 3, 5));
        var w2 = new WeekWindow(new DateTime(2024, 3, 12));

        Assert.False(w1.Equals(w2));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenOtherIsNull() {
    
        var w1 = new WeekWindow(DateTime.Today);

        Assert.False(w1.Equals(null));
    }

    [Fact]
    public void GetHashCode_IsSame_ForEqualObjects() {
        var w1 = new WeekWindow(new DateTime(2024, 3, 5));
        var w2 = new WeekWindow(new DateTime(2024, 3, 5));

        Assert.Equal(w1.GetHashCode(), w2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsDifferent_ForDifferentDates() {
        var w1 = new WeekWindow(new DateTime(2024, 3, 5));
        var w2 = new WeekWindow(new DateTime(2024, 3, 12));

        Assert.NotEqual(w1.GetHashCode(), w2.GetHashCode());
    }
}