CREATE TABLE IF NOT EXISTS ProcessedPosts (
    FeedId TEXT NOT NULL,
    ItemId TEXT NOT NULL,
    Timestamp INTEGER NOT NULL,
    StatusId TEXT NULL,
    PRIMARY KEY (FeedId, ItemId)
);
