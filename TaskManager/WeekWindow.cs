namespace TaskManager;

public class WeekWindow : IEquatable<WeekWindow> {
    /*
    Represents a 7-day window starting from a given date. The start date is normalized to the beginning of the day, and the end date is set to 6 days after the start.

     Equality is based on the Start date only, meaning two WeekWindow instances are considered equal if they have the same Start date, regardless of their End dates.
    */
    
    public DateTime Start { get; }
    public DateTime End { get; }

    public WeekWindow(DateTime start) {
        Start = start.Date;
        End = Start.AddDays(6);
    }

    public bool Equals(WeekWindow? other) {
        if (other is null) return false;
        return Start == other.Start;
    }


    public override bool Equals(object? obj) => Equals(obj as WeekWindow);

    public override int GetHashCode() => Start.GetHashCode();


}