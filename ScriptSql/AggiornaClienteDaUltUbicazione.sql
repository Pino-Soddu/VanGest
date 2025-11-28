UPDATE A
SET A.ClienteUbicazione = 
    CASE 
        WHEN C.Nome IS NOT NULL THEN C.Cognome + ' ' + C.Nome 
        ELSE C.Cognome 
    END
FROM AnagVeicoli A
CROSS APPLY (
    SELECT TOP 1 C.Cognome, C.Nome
    FROM TabClienti C
    WHERE A.UltUbicazione LIKE '%' + C.Cognome + '%'
    ORDER BY 
        -- Priorità: prima corrispondenze con cognome+nome
        CASE 
            WHEN A.UltUbicazione LIKE '%' + C.Cognome + '%' 
                 AND C.Nome IS NOT NULL 
                 AND A.UltUbicazione LIKE '%' + C.Nome + '%' THEN 1
            ELSE 2
        END
) C
WHERE 
    A.ClienteUbicazione IS NULL  
    AND C.Cognome IS NOT NULL   