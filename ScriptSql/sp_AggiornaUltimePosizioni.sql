USE [DBUnico];
GO

IF OBJECT_ID('sp_AggiornaUltimePosizioni', 'P') IS NOT NULL
DROP PROCEDURE sp_AggiornaUltimePosizioni;
GO

CREATE PROCEDURE sp_AggiornaUltimePosizioni
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Targa, 
        CONVERT(DECIMAL(9,6), REPLACE(Lat, ',', '.')) AS Lat,
        CONVERT(DECIMAL(9,6), REPLACE(Lon, ',', '.')) AS Lon,
        Now, TipoSat, Citta, FullAddress, km, velocita, Nazione,
        ROW_NUMBER() OVER (PARTITION BY Targa ORDER BY Now DESC) AS Ordine
    INTO #UltimePosizioni
    FROM [EasySat1].[dbo].[Track]
    WHERE Now > DATEADD(HOUR, -2, GETDATE())
    AND TRY_CONVERT(DECIMAL(9,6), REPLACE(Lat, ',', '.')) IS NOT NULL
    AND TRY_CONVERT(DECIMAL(9,6), REPLACE(Lon, ',', '.')) IS NOT NULL;
    
    -- MODIFICA SPECIFICA: tronca l'indirizzo a 250 byte
    UPDATE a SET
        a.TipoSatTrack = up.TipoSat,
        a.MatricolaTrack = up.Citta,
        a.EsisteSuTrack = 1,
        a.OnlineEffettivo = 1,
        a.DataUltimaRilevazioneGPS = up.Now,
        a.IndirizzoUltimaRilevazioneGPS = LEFT(up.FullAddress, 250), -- <-- UNICA MODIFICA
        a.ContaKmFine = up.km,
        a.Latitudine = up.Lat,
        a.Longitudine = up.Lon,
        a.velocitaGPS = up.velocita,
        a.EventoGPS = up.Nazione
    FROM [DBUnico].[dbo].[AnagGenerale] a
    JOIN #UltimePosizioni up ON a.Targa = up.Targa
    WHERE up.Ordine = 1;
    
    INSERT INTO [DBUnico].[dbo].[LogTargheNonTrovate] (
        Targa, DataRegistrazione, Lat, Lon, TipoSat, Matricola, Motivo
    )
    SELECT 
        up.Targa, up.Now, up.Lat, up.Lon, up.TipoSat, up.Citta, 
        'Targa non registrata'
    FROM #UltimePosizioni up
    LEFT JOIN [DBUnico].[dbo].[AnagGenerale] a ON up.Targa = a.Targa
    WHERE up.Ordine = 1 AND a.Targa IS NULL;
    
    DROP TABLE #UltimePosizioni;
END;
GO