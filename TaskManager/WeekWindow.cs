namespace TaskManager;

public class WeekWindow : IEquatable<WeekWindow> {
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