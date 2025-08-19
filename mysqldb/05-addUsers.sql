CREATE USER 'SpotifyServiceAdmin'@'172.18.0.%' IDENTIFIED BY 'bXPYSNt9eakiC$cRnq6eexQ$QEspJDMcFa$86RLY';
GRANT 'SpotifyAdmin' TO 'SpotifyServiceAdmin'@'172.18.0.%';
SET DEFAULT ROLE 'SpotifyAdmin' TO 'SpotifyServiceAdmin'@'172.18.0.%';
