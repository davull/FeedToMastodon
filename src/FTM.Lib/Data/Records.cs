namespace FTM.Lib.Data;

public record ProcessedPostRecord(
    string FeedId,
    string ItemId,
    long Timestamp,
    string? StatusId,
    string? FeedTitle);