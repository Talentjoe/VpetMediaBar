using MediaClientDataInterFace;

namespace MediaServer;

public class TimelinePropertiesSerializableDataGenerator : TimelinePropertiesSerializableData
{
    public TimelinePropertiesSerializableDataGenerator(
        Windows.Media.Control.GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties)
    {
        EndTime = timelineProperties.EndTime;
        LastUpdatedTime = timelineProperties.LastUpdatedTime;
        MaxSeekTime = timelineProperties.MaxSeekTime;
        MinSeekTime = timelineProperties.MinSeekTime;
        Position = timelineProperties.Position;
        StartTime = timelineProperties.StartTime;
    }
    
}