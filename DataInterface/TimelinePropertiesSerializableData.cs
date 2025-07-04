namespace MediaClientDataInterFace;

public class TimelinePropertiesSerializableData
{
    public TimeSpan EndTime { get; init; }
    public DateTimeOffset LastUpdatedTime { get; init; }
    public TimeSpan MaxSeekTime { get; init; }
    public TimeSpan MinSeekTime { get; init; }
    public TimeSpan Position { get; init; }
    public TimeSpan StartTime { get; init; }
}