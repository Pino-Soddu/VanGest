🚐 VanGest - Sistema di Gestione Noleggio Furgoni
📋 Panoramica
VanGest è un'applicazione Blazor Server completa per la gestione del noleggio furgoni, progettata per Partner SAT. Il sistema offre interfacce separate per clienti e staff, con integrazione AI per l'assistenza clienti.

✨ Funzionalità Principali
🌐 Area Pubblica (Clienti)
Homepage moderna con hero section e presentazione servizi

Assistente AI integrato (DeepSeek) per:

Consigli sulla scelta del furgone

Informazioni su disponibilità e prezzi

Supporto prenotazioni

Sistema di login con modale overlay

Chat in tempo reale con interfaccia moderna

👨‍💼 Area Staff (Interna)
Dashboard operativa con layout dedicato

Gestione flotta furgoni con griglia avanzata

Sistema di filtri complessi per ricerca veicoli

Gestione località e sedi

Visualizzazione mappe integrate (Google Maps/OSM)

🛠️ Architettura Tecnica
Frontend
Blazor Server (.NET 8)

Bootstrap 5 con tema personalizzato (colori Lazio)

CSS modulare con componenti isolati

JavaScript interop per mappe e funzionalità avanzate

Backend
.NET 8 con architettura a servizi

Entity Framework Core (se presente database)

Servizi modulari (Auth, Chat, Geo, Export)

API REST per integrazioni esterne

Integrazioni
DeepSeek AI per assistenza conversazionale

Google Maps API per visualizzazione mappe

OpenStreetMap alternativa open-source

Excel Export per reportistica

🎨 Design System
Colori aziendali: Blu Lazio (#1e3c72 → #2a5298)

Layout responsive mobile-first

Componenti riutilizzabili (Chat, Grid, Filter, Modal)

UI/UX ottimizzata per usabilità

📁 Struttura Progetto
text
VanGest.Server/
├── Components/          # Componenti riutilizzabili
│   ├── Chat/           # Sistema chat AI
│   ├── Staff/          # Componenti area staff
│   └── Map/            # Integrazioni mappe
├── Pages/              # Pagine principali
├── Services/           # Logica business
├── Models/             # Modelli dati
└── wwwroot/            # Asset statici
🚀 Deployment
L'applicazione supporta deployment su:

IIS (Windows Server)

Linux con Kestrel

Docker (configurazione inclusa)

Azure App Service

🔧 Prerequisiti
.NET 8 SDK

Database (SQL Server / PostgreSQL / SQLite)

API Keys (Google Maps, DeepSeek - opzionali)

⚡ Quick Start
bash
# Clone repository
git clone https://github.com/Pino-Soddu/VanGest.git

# Configura appsettings.json
cp appsettings.example.json appsettings.json

# Avvia applicazione
dotnet run
📊 Stato Progetto
✅ Completato - Sistema base funzionante
✅ Completato - Login e autenticazione
✅ Completato - Area staff operativa
✅ Completato - Chat AI integrata
🔄 In sviluppo - Ottimizzazioni performance
📅 Pianificato - App mobile companion

👥 Ruoli Utente
Cliente occasionale - Consultazione e prenotazione

Cliente registrato - Gestione noleggi attivi

Staff operativo - Gestione flotta e clienti

Amministratore - Configurazione sistema

🔒 Sicurezza
Autenticazione basata su token JWT

Autorizzazione per ruoli

Validazione input lato server

Protezione contro attacchi comuni

📞 Supporto
Per supporto o contributi, consultare le Issue o contattare il maintainer.

Licenza: Proprietario - Partner SAT
Versione: 1.0.0
Ultimo aggiornamento: 2024

"La tua libertà su quattro ruote" 🚚💨

me lo dai formattato md?
markdown
# 🚐 VanGest - Sistema di Gestione Noleggio Furgoni

## 📋 Panoramica
**VanGest** è un'applicazione Blazor Server completa per la gestione del noleggio furgoni, progettata per **Partner SAT**. Il sistema offre interfacce separate per clienti e staff, con integrazione AI per l'assistenza clienti.

## ✨ Funzionalità Principali

### 🌐 **Area Pubblica (Clienti)**
- **Homepage moderna** con hero section e presentazione servizi
- **Assistente AI integrato** (DeepSeek) per:
  - Consigli sulla scelta del furgone
  - Informazioni su disponibilità e prezzi
  - Supporto prenotazioni
- **Sistema di login** con modale overlay
- **Chat in tempo reale** con interfaccia moderna

### 👨‍💼 **Area Staff (Interna)**
- **Dashboard operativa** con layout dedicato
- **Gestione flotta furgoni** con griglia avanzata
- **Sistema di filtri** complessi per ricerca veicoli
- **Gestione località** e sedi
- **Visualizzazione mappe** integrate (Google Maps/OSM)

## 🛠️ Architettura Tecnica

### **Frontend**
- **Blazor Server** (.NET 8)
- **Bootstrap 5** con tema personalizzato (colori Lazio)
- **CSS modulare** con componenti isolati
- **JavaScript interop** per mappe e funzionalità avanzate

### **Backend**
- **.NET 8** con architettura a servizi
- **Entity Framework Core** (se presente database)
- **Servizi modulari** (Auth, Chat, Geo, Export)
- **API REST** per integrazioni esterne

### **Integrazioni**
- **DeepSeek AI** per assistenza conversazionale
- **Google Maps API** per visualizzazione mappe
- **OpenStreetMap** alternativa open-source
- **Excel Export** per reportistica

## 🎨 Design System
- **Colori aziendali**: Blu Lazio (#1e3c72 → #2a5298)
- **Layout responsive** mobile-first
- **Componenti riutilizzabili** (Chat, Grid, Filter, Modal)
- **UI/UX ottimizzata** per usabilità

## 📁 Struttura Progetto
VanGest.Server/
├── Components/ # Componenti riutilizzabili
│ ├── Chat/ # Sistema chat AI
│ ├── Staff/ # Componenti area staff
│ └── Map/ # Integrazioni mappe
├── Pages/ # Pagine principali
├── Services/ # Logica business
├── Models/ # Modelli dati
└── wwwroot/ # Asset statici

text

## 🚀 Deployment
L'applicazione supporta deployment su:
- **IIS** (Windows Server)
- **Linux** con Kestrel
- **Docker** (configurazione inclusa)
- **Azure App Service**

## 🔧 Prerequisiti
- **.NET 8 SDK**
- **Database** (SQL Server / PostgreSQL / SQLite)
- **API Keys** (Google Maps, DeepSeek - opzionali)

## ⚡ Quick Start
```bash
# Clone repository
git clone https://github.com/Pino-Soddu/VanGest.git

# Configura appsettings.json
cp appsettings.example.json appsettings.json

# Avvia applicazione
dotnet run
📊 Stato Progetto
✅ Completato - Sistema base funzionante
✅ Completato - Login e autenticazione
✅ Completato - Area staff operativa
✅ Completato - Chat AI integrata
🔄 In sviluppo - Ottimizzazioni performance
📅 Pianificato - App mobile companion

👥 Ruoli Utente
Cliente occasionale - Consultazione e prenotazione

Cliente registrato - Gestione noleggi attivi

Staff operativo - Gestione flotta e clienti

Amministratore - Configurazione sistema

🔒 Sicurezza
Autenticazione basata su token JWT

Autorizzazione per ruoli

Validazione input lato server

Protezione contro attacchi comuni

📞 Supporto
Per supporto o contributi, consultare le Issue o contattare il maintainer.

Licenza: Proprietario - Partner SAT
Versione: 1.0.0
Ultimo aggiornamento: 2024

"La tua libertà su quattro ruote" 🚚💨
