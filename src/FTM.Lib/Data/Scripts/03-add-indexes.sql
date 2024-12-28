CREATE INDEX IX_FeedId_ItemId ON ProcessedPosts (FeedId, ItemId);

CREATE INDEX IX_Timestamp_StatusId ON ProcessedPosts (Timestamp, StatusId);
