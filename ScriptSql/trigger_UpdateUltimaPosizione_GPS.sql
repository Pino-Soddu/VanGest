CREATE TRIGGER tr_UpdateUltimaPosizione_EasySat1_Track
ON [EasySat1].[dbo].[Track]  
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Aggiorna i veicoli esistenti e logga le targhe non trovate
    MERGE INTO dbo.DBUnico.AnagGenerale AS target
    USING (
        SELECT 
            i.Targa,
            i.TipoSat,
            i.Citta AS Matricola,
            i.Now AS DataRilevazione,
            i.FullAddress,
            i.km,
            i.Lat,
            i.Lon,
            i.velocita,
            i.Nazione AS Evento
        FROM inserted i
        WHERE i.Lat <> 0 AND i.Lon <> 0
    ) AS source
    ON (target.Targa = source.Targa)
    
    WHEN MATCHED THEN
        UPDATE SET 
            target.TipoSatTrack = source.TipoSat,
            target.MatricolaTrack = source.Matricola,
            target.EsisteSuTrack = 1,
            target.OnlineEffettivo = 1,
            target.DataUltimaRilevazioneGPS = source.DataRilevazione,
            target.IndirizzoUltimaRilevazioneGPS = source.FullAddress,
            target.ContaKmFine = source.km,
            target.Latitudine = source.Lat,
            target.Longitudine = source.Lon,
            target.velocitaGPS = source.velocita,
            target.EventoGPS = source.Evento;
    
    -- 2. Logga separatamente le targhe non trovate
    INSERT INTO dbo.LogTargheNonTrovate (
        Targa,
        DataRegistrazione,
        Lat,
        Lon,
        TipoSat,
        Matricola,
        Motivo
    )
    SELECT 
        i.Targa,
        i.Now,
        i.Lat,
        i.Lon,
        i.TipoSat,
        i.Citta,
        'Targa non presente in AnagGenerale'
    FROM inserted i
    LEFT JOIN dbo.DBUnico.AnagGenerale a ON i.Targa = a.Targa
    WHERE a.Targa IS NULL
    AND i.Lat <> 0 AND i.Lon <> 0;
END;