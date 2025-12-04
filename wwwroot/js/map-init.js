// map-init.js - Versione ottimizzata per coesistenza Google Maps/OSM

if (!window.vanGest) {
    window.vanGest = {};
}

// map-init.js - Versione ottimizzata per coesistenza Google Maps/OSM

if (!window.vanGest) {
    window.vanGest = {};
}

// Funzione semplificata per lo scroll
// Aggiungi questo all'inizio del file, dopo la dichiarazione di window.vanGest
window.vanGest.utils = {
    // Scroll alla sezione filtri (esistente)
    scrollToFilters: function () {
        window.scrollTo(0, 0);

        // 2. Trova la sezione filtri con priorità
        const filters = document.querySelector(`
            .advanced-filter-container,
            [id*='filter'],
            [class*='filter']
        `);

        if (filters) {
            // 3. Calcolo preciso con offset
            const header = document.querySelector('header');
            const offset = header ? header.offsetHeight + 20 : 20;
            const targetPos = filters.getBoundingClientRect().top + window.scrollY - offset;

            // 4. Scroll animato dopo un breve delay
            setTimeout(() => {
                window.scrollTo({
                    top: targetPos,
                    behavior: 'smooth'
                });
            }, 30);
        }
    },

    // Scroll in alto alla pagina (nuova)
    scrollToTop: function (options = {}) {
        try {
            window.scrollTo({
                top: 0,
                behavior: options.behavior || 'smooth',
                ...options
            });
            return true;
        } catch (err) {
            console.error('Errore scrollToTop:', err);
            // Fallback a scroll immediato
            window.scrollTo(0, 0);
            return false;
        }
    },

    // Scroll a un elemento specifico (utilità generica)
    scrollToElement: function (selector, options = {}) {
        try {
            const element = document.querySelector(selector);
            if (!element) {
                console.warn('Elemento non trovato:', selector);
                return false;
            }
            element.scrollIntoView({
                behavior: options.behavior || 'smooth',
                block: options.block || 'start',
                ...options
            });
            return true;
        } catch (err) {
            console.error('Errore scrollToElement:', err, selector);
            return false;
        }
    }
};

// Separazione chiara delle mappe
window.vanGest.maps = {
    // OSM
    osm: {
        map: null,
        markers: new Map(),

        init: function (localita, dotNetHelper) {
            // Reset precedente mappa
            if (this.map) {
                this.map.off();
                this.map.remove();
            }
            this.markers.clear();

            // Container
            const mapContainer = document.createElement('div');
            mapContainer.id = 'osm-map-container';
            mapContainer.style.width = '100%';
            mapContainer.style.height = '100%';
            document.querySelector('.map-overlay-body').appendChild(mapContainer);

            // Mappa
            this.map = L.map(mapContainer.id).setView([41.8719, 12.5674], 6);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(this.map);

            // Icona
            const customIcon = L.icon({
                iconUrl: '/icons/parking.svg',
                iconSize: [32, 32],
                popupAnchor: [0, -15]
            });

            // Marker
            localita.forEach(loc => {
                const lat = Number(loc.latitudine || loc.Latitudine);
                const lng = Number(loc.longitudine || loc.Longitudine);
                const id = loc.idLocalita;

                if (!isNaN(lat) && !isNaN(lng)) {
                    const marker = L.marker([lat, lng], {
                        icon: customIcon,
                        draggable: true
                    }).bindTooltip(loc.nomeLocalita || loc.NomeLocalita, {
                        permanent: false,
                        direction: 'top',
                        className: 'leaflet-tooltip-custom'
                    }).addTo(this.map);

                    this.markers.set(id, {
                        instance: marker,
                        originalPos: { lat, lng }
                    });

                    // Eventi (mantenuti identici)
                    marker.on('dragstart', function () {
                        const data = window.vanGest.maps.osm.markers.get(id);
                        data.dragStartPos = this.getLatLng();
                    });

                    marker.on('dragend', async function () {
                        const markerData = window.vanGest.maps.osm.markers.get(id);
                        const confirmMove = await confirm("Confermi lo spostamento?");

                        if (confirmMove) {
                            const newPos = this.getLatLng();
                            await dotNetHelper.invokeMethodAsync('OnMarkerDragEnd', id, newPos.lat, newPos.lng);
                            markerData.originalPos = newPos;
                        } else {
                            this.setLatLng(markerData.dragStartPos || markerData.originalPos);
                        }
                    });
                }
            });
        },

        centerOnMarker: function (id, zoom = 16) {
            const markerData = this.markers.get(id);
            if (markerData && this.map) {
                this.map.flyTo(markerData.instance.getLatLng(), zoom, {
                    animate: true,
                    duration: 0.5
                });
            }
        },

        reset: function () {
            if (this.map) {
                this.map.off();
                this.map.remove();
                this.map = null;
            }
            this.markers.clear();
        }
    },


    // Google Maps
    google: {
        _map: null,
        _markers: [],
        _cluster: null,
        _iconCache: {
            'M': { url: '/icons/vanm.png', size: 42 },
            'F': { url: '/icons/vanf.png', size: 42 },
            'S': { url: '/icons/vans.png', size: 42 },
            'P': { url: '/icons/vanp.png', size: 42 },
            'default': { url: '/icons/van.png', size: 42 }
        },

        // Funzione principale unificata
        renderMap: function (containerId, vans) {
            // Inizializzazione mappa se non esiste
            if (!this._map) {
                this._initMap(containerId);
            }

            // Aggiornamento marker
            this._updateMarkers(vans);
        },

        // Inizializzazione mappa (solo al primo load)
        // 1. INIZIALIZZAZIONE UNICA (nella tua funzione _initMap)
        _initMap: function (containerId) {
            const container = document.getElementById(containerId);
            if (!container) return;

            this._map = new google.maps.Map(container, {
                center: { lat: 41.9028, lng: 12.4964 },
                zoom: 6,
                gestureHandling: 'greedy'
            });

            // INIZIALIZZA IL CLUSTER UNA SOLA VOLTA QUI
            this._cluster = new markerClusterer.MarkerClusterer({
                map: this._map,
                markers: [], // Inizialmente vuoto
                minimumClusterSize: 5,
                maxZoom: 14,
                styles: [{
                    url: '/icons/cluster.png',
                    width: 40,
                    height: 40,
                    textColor: 'white',
                    anchorText: [0, 10]
                }]
            });
        },

        // 2. AGGIORNAMENTO MARKER (usa SOLO addMarkers/removeMarkers)
        _updateMarkers: function (vans) {
            const validVans = vans.filter(van =>
                !isNaN(van.latitudine) && !isNaN(van.longitudine)
            );

            // Rimuovi SOLO i marker obsoleti
            const oldMarkers = this._cluster.getMarkers();
            oldMarkers.forEach(marker => {
                if (!validVans.some(v => v.targa === marker.targa)) {
                    this._cluster.removeMarker(marker);
                    marker.setMap(null);
                }
            });

            // Aggiungi/aggiorna i nuovi marker
            validVans.forEach(van => {
                const existing = oldMarkers.find(m => m.targa === van.targa);
                if (existing) {
                    existing.setPosition({
                        lat: parseFloat(van.latitudine),
                        lng: parseFloat(van.longitudine)
                    });
                } else {
                    const marker = new google.maps.Marker({
                        position: {
                            lat: parseFloat(van.latitudine),
                            lng: parseFloat(van.longitudine)
                        },
                        targa: van.targa, // Aggiungi riferimento
                        map: this._map,
                        icon: this._getIconConfig(van.tipososta || van.tipoSosta)
                    });
                    this._cluster.addMarker(marker);
                }
            });
        },

        // Gestione intelligente dei marker
        _updateMarkers: function (vans) {
            // 1. Filtra coordinate valide
            const validVans = vans.filter(van =>
                !isNaN(van.latitudine) && !isNaN(van.longitudine)
            );

            // 2. Rimuovi TUTTI i vecchi marker dal cluster
            if (this._cluster) {
                this._cluster.clearMarkers();
            }

            // 3. Pulisci mappa dai marker esistenti
            this._markers.forEach(m => m.marker.setMap(null));
            this._markers = [];

            // 4. Crea nuovi marker con tooltip
            this._markers = validVans.map(van => {
                const marker = new google.maps.Marker({
                    position: {
                        lat: parseFloat(van.latitudine),
                        lng: parseFloat(van.longitudine)
                    },
                    map: this._map,
                    icon: this._getIconConfig(van.tipososta || van.tipoSosta),
                });

                // Contenuto HTML elegante per il tooltip
                const tooltipContent = `
                    <div class="custom-tooltip">
                        <div class="tooltip-header">
                            <span class="targa">${van.targa}</span>
                            ${van.tipososta ? `<span class="status-badge ${van.tipososta}">${van.tipososta}</span>` : ''}
                        </div>
                        <div class="tooltip-body">
                            ${van.clienteubicazione ? `<p><strong>Cliente:</strong> ${van.clienteubicazione}</p>` : ''}
                            ${van.disponibile ? `<p><strong>Dispo:</strong> ${van.disponibile}</p>` : ''}
                            ${van.marca ? `<p><strong>Marca:</strong> ${van.marca}</p>` : ''}
                        </div>
                        ${van.datafine ? `
                        <div class="tooltip-footer">
                            Ultimo Aggiornamento: <span class="datafine">${
                            new Date(van.datafine).toLocaleString('it-IT', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit'
                                }).replace(',', ' -')
                            }
                        </div>` : ''}
                    </div>
                    `;

                // InfoWindow (tooltip avanzato)
                const infoWindow = new google.maps.InfoWindow({
                    content: tooltipContent,
                    maxWidth: 250
                });

                // Mostra/nascondi al passaggio del mouse
                marker.addListener('mouseover', () => infoWindow.open(this._map, marker));
                marker.addListener('mouseout', () => infoWindow.close());

                return { marker, vanData: van };

            });

            // 5. RI-INIZIALIZZA il cluster con le nuove impostazioni
            if (this._markers.length > 0) {
                this._cluster = new markerClusterer.MarkerClusterer({
                    map: this._map,
                    markers: this._markers.map(m => m.marker),
                    minimumClusterSize: 5,
                    maxZoom: 14,
                    styles: [{
                        url: '/icons/cluster.png',
                        width: 40,
                        height: 40,
                        textColor: 'white'
                    }]
                });
            }
        },

        // Helper: Assegna icona in base a TipoSosta
        _updateMarkerIcon: function (marker, tipososta) {
            marker.setIcon(this._getIconConfig(tipososta));
        },

        // Helper: Configurazione icona con fallback
        _getIconConfig: function (tipososta) {
            const tipo = (tipososta || '').toUpperCase();
            const iconType = this._iconCache[tipo] || this._iconCache.default;

            return {
                url: iconType.url,
                scaledSize: new google.maps.Size(iconType.size, iconType.size),
                anchor: new google.maps.Point(iconType.size / 2, iconType.size / 2)
            };
        },

        // Helper: Costruzione tooltip
        _buildTooltip: function (van) {
            return [
                `Targa: ${van.targa}`,
                van.tipososta && `Stato: ${van.tipososta}`,
                van.clienteubicazione && `Cliente: ${van.clienteubicazione}`
            ].filter(Boolean).join(' | ');
        },

        // Reset completo
        reset: function () {
            this._markers.forEach(m => m.marker.setMap(null));
            this._markers = [];
            if (this._cluster) this._cluster.clearMarkers();
        }
    }
};

// Funzioni di drag (rimaste identiche)
function initMapDrag(container) {
    const header = container.querySelector('.map-overlay-header');
    let startX, startY, startRight, startTop;

    header.addEventListener('mousedown', (e) => {
        e.preventDefault();
        startX = e.clientX;
        startY = e.clientY;
        startRight = parseInt(window.getComputedStyle(container).right) || 20;
        startTop = parseInt(window.getComputedStyle(container).top) || 100;

        const moveHandler = (e) => {
            const deltaX = startX - e.clientX;
            const deltaY = e.clientY - startY;
            container.style.right = `${Math.max(10, Math.min(startRight + deltaX, window.innerWidth - container.offsetWidth - 10))}px`;
            container.style.top = `${Math.max(10, Math.min(startTop + deltaY, window.innerHeight - container.offsetHeight - 10))}px`;
        };

        const upHandler = () => {
            document.removeEventListener('mousemove', moveHandler);
            document.removeEventListener('mouseup', upHandler);
        };

        document.addEventListener('mousemove', moveHandler);
        document.addEventListener('mouseup', upHandler);
    });
}

// Esposizione globale (mantenuta per retrocompatibilità)
window.initGoogleMap = (containerId, vans) => window.vanGest.maps.google.renderMap(containerId, vans);
window.initMap = (dotNetRef, containerElement, vans) => window.vanGest.maps.google.init(dotNetRef, containerElement, vans);
window.initOSMMap = (localita, dotNetHelper) => window.vanGest.maps.osm.init(localita, dotNetHelper);
window.initMapDrag = initMapDrag;
window.centerMapOnMarker = (id, zoom) => window.vanGest.maps.osm.centerOnMarker(id, zoom);

