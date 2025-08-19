CREATE TABLE User (
    UserID VARCHAR(255) PRIMARY KEY,
    SpotifyTokenID INT,
    SessionID INT,

    FOREIGN KEY (SpotifyTokenID)
    REFERENCES SpotifyToken(SpotifyTokenID)
    ON DELETE CASCADE,

    FOREIGN KEY (SessionID)
    REFERENCES Session(SessionID)
    ON DELETE CASCADE
);
