CREATE OR ALTER TRIGGER TR_Localita_Update_Furgoni
ON Localita
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Aggiorna solo se IdComune è stato modificato (evita esecuzioni inutili)
    IF UPDATE(IdComune) OR UPDATE(NomeLocalita) OR UPDATE(Latitudine) OR UPDATE(Longitudine)
    BEGIN
        UPDATE f
        SET 
            f.IdComune = i.IdComune,
            f.IdProvincia = c.CodiceProvincia,
            f.IdRegione = c.CodiceRegione,
            f.Latitudine = ISNULL(i.Latitudine, f.Latitudine),  -- Mantiene il valore esistente se NULL
            f.Longitudine = ISNULL(i.Longitudine, f.Longitudine),
            f.Località = i.NomeLocalita  -- Aggiorna anche il nome se cambiato
        FROM 
            AnagGenerale f
            INNER JOIN inserted i ON f.Località = i.NomeLocalita  -- Match solo sul nome
            INNER JOIN AnagCitta c ON i.IdComune = c.CodiceComune
        WHERE 
            i.IdComune IS NOT NULL
            AND c.CodiceComune IS NOT NULL;  -- Doppio check per sicurezza
    END
END