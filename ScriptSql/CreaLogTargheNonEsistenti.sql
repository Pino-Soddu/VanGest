CREATE TABLE dbo.LogTargheNonTrovate (
    IDLog INT IDENTITY(1,1) PRIMARY KEY,
    Targa NVARCHAR(20) NOT NULL,
    DataRegistrazione DATETIME NOT NULL,
    Lat FLOAT NOT NULL,
    Lon FLOAT NOT NULL,
    TipoSat NVARCHAR(50) NULL,
    Matricola NVARCHAR(100) NULL,
    Motivo NVARCHAR(255) NOT NULL
);

-- Aggiungo indice per migliorare le ricerche per targa
CREATE INDEX IX_LogTargheNonTrovate_Targa ON dbo.LogTargheNonTrovate (Targa);

-- Aggiungo indice per ricerche temporali
CREATE INDEX IX_LogTargheNonTrovate_DataRegistrazione ON dbo.LogTargheNonTrovate (DataRegistrazione);