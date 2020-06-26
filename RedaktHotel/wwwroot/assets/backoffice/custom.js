var RedaktHotel = (function () {
    
    function RedaktHotel() {
    }

    RedaktHotel.prototype.initializeMap = function (containerId, latitude, longitude) {
        let location = [4.899432, 52.379752];
        if (latitude && longitude) location = [longitude, latitude];

        const map = new mapboxgl.Map({
            container: containerId,
            style: {
                'version': 8,
                'sources': {
                    'raster-tiles': {
                        'type': 'raster',
                        'tiles': [
                            'https://b.tile.openstreetmap.org/{z}/{x}/{y}.png'
                        ],
                        'tileSize': 256,
                    }
                },
                'layers': [{
                    'id': 'simple-tiles',
                    'type': 'raster',
                    'source': 'raster-tiles',
                    'minzoom': 0,
                    'maxzoom': 22
                }]
            },
            center: location,
            zoom: 10
        });

        if (latitude && longitude) {
            marker.setLngLat([longitude, latitude]).addTo(map);
        }
    };

    return RedaktHotel;
}());

var redaktHotel = new RedaktHotel();
