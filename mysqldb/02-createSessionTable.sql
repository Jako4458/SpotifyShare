CREATE TABLE Session (
    SessionID INT AUTO_INCREMENT PRIMARY KEY,
    SessionName VARCHAR(255) NOT NULL,
    SharingStatus VARCHAR(50) NOT NULL DEFAULT "Private" CHECK (SharingStatus IN ('Private', 'Public', 'LoginRequired', 'PasswordRequired')),
    Password VARCHAR(255),
    ExpirationTime DATETIME
);
