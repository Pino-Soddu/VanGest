-- Elimina la view se esiste già
DROP VIEW IF EXISTS VansView;
GO

-- Aggiorna il campo Disponibile in AnagVeicoli
UPDATE AnagVeicoli
SET Disponibile = 
    CASE 
        WHEN LEFT(UltUbicazione, 1) = '$' THEN '$'
        WHEN UltUbicazione LIKE '[[]_]%' THEN 
            SUBSTRING(UltUbicazione, CHARINDEX('[', UltUbicazione) + 1, 1) 
        ELSE 
            Disponibile 
    END
WHERE AnagVeicoli.IdVeicolo > 0 
AND (UltUbicazione LIKE '[[]_]%' OR LEFT(UltUbicazione, 1) = '$');
GO

-- Crea la view VansView
CREATE VIEW VansView AS
SELECT 
    ag.IdVeicolo AS IdVeicolo,
    ag.Targa,
    ag.Telaio,
    ag.Località AS Località,
    av.Proprietario AS Proprietario,
    av.Locatario AS Locatario,
    av.Utilizzatore AS Utilizzatore,
    av.SubUtilizzatore AS SubUtilizzatore,
    av.MetodoAcquisizione AS MetodoAcquisizione,
    av.IdGestore AS IdGestore,
    av.IdGestManutenzioni AS IdGestManutenzioni,
    av.UltUbicazione AS UltUbicazione,
    av.Disponibile AS Disponibile,
	av.ClienteUbicazione AS ClienteUbicazione,
    ac.NomeComune AS Comune,
    ac.NomeProvincia AS Provincia,
    ac.NomeRegione AS Regione,
    marca.Descrizione AS Marca,
    modello.Descrizione AS Modello,
    motore.Descrizione AS Alimentazione,
    segmento.Descrizione AS Segmento,

    ag.DataUltimaRilevazioneGPS,
	ag.DataInizio,
	ag.DataFine,
	ag.TipoSosta,
	ag.IndirizzoUltimaRilevazioneGPS,
    ag.ContaKmFine,
    ag.Latitudine,
    ag.Longitudine,
	ag.VelocitaGPS,
	ag.EventoGPS,
    ag.EsisteSuTrack,
    ag.TipoSatTrack,
    ag.ModelloTrack,
    ag.MatricolaTrack,
    ag.OnlineEffettivo,
    ag.NggOffLine,

    ag.FotoAnteprimaURL,
	ass_max.DataScadPremio AS DataScadenzaPremio,
    ab.DataProssRev AS DataProssimaRevisione,
    ab.DataScadATP AS DataScadenzaATP
FROM AnagGenerale ag
    LEFT JOIN AnagVeicoli av ON ag.IdVeicolo = av.IdVeicolo
    LEFT JOIN AnagCitta ac ON ag.IdComune = ac.CodiceComune
    LEFT JOIN AnagCaratteristiche acar ON ag.IdVeicolo = acar.IdVeicolo
    LEFT JOIN TabMarcaVeicoli marca ON acar.IdMarca = marca.IdMarcaVeicoli
    LEFT JOIN TabModelloVeicoli modello ON acar.IdModello = modello.IdModelloVeicoli
    LEFT JOIN TabMotoreVeicoli motore ON acar.IdMotore = motore.IdMotoreVeicoli
    LEFT JOIN TabSegmenti segmento ON acar.Idsegmento = segmento.IdSegmento
    LEFT JOIN AnagDatibase ab ON ag.IdVeicolo = ab.IdVeicolo
	    LEFT JOIN (
        SELECT a1.IdVeicolo, a1.DataScadPremio
        FROM Assicurazioni a1
        INNER JOIN (
            SELECT IdVeicolo, MAX(DataScadPremio) AS MaxDataScad
            FROM Assicurazioni
            WHERE TipoPolizza <> 2
            GROUP BY IdVeicolo
        ) a2 ON a1.IdVeicolo = a2.IdVeicolo AND a1.DataScadPremio = a2.MaxDataScad
    ) ass_max ON ag.IdVeicolo = ass_max.IdVeicolo
WHERE 
    av.StatoAttivoEasy = 1 
    OR av.StatoAttivoTruck = 1 
    OR av.StatoAttivoIVCE = 1 
    OR av.StatoAttivoMeaG = 1 
    OR av.StatoAttivoTAG = 1
GO

-- Verifica dei risultati
SELECT 
    IdVeicolo,
    UltUbicazione,
    Disponibile AS NuovoValoreDisponibile
FROM AnagGenerale
WHERE UltUbicazione LIKE '[[]_]%'
ORDER BY Targa;