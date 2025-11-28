CREATE TABLE Localita (
    IdLocalita INT IDENTITY(1,1) PRIMARY KEY,
    NomeLocalita NVARCHAR(150) NOT NULL,
    Comune NVARCHAR(50),
    IdComune INT,
	Indirizzo	nvarchar(250),
    Latitudine DECIMAL(9,6),
    Longitudine DECIMAL(9,6),
    NomeResponsabile NVARCHAR(100),
    TelefonoResponsabile VARCHAR(20),
    EmailResponsabile NVARCHAR(100),
    Note TEXT,
    
    -- Vincolo di integrità referenziale con tabella ISTAT Comuni
    CONSTRAINT FK_Localita_Comune FOREIGN KEY (IdComune) 
    REFERENCES AnagCitta (CodiceComune)
);