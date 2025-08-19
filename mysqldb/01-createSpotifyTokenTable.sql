CREATE TABLE SpotifyToken (
    SpotifyTokenID INT AUTO_INCREMENT PRIMARY KEY,
    RefreshToken VARCHAR(512),
    AccessToken VARCHAR(512),
    Code VARCHAR(1024) NOT NULL,
    Status VARCHAR(255),
    ExpirationTime DATETIME
);
