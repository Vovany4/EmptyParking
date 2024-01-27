function setUpMap(data) {
    let mapOptions = {
        center: [50.0299, 19.9099],
        zoom: 13
    };

    var map = L.map('map', mapOptions);

    let layer = new L.TileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png')
    map.addLayer(layer);

    /*L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);*/


    data.forEach((item) => {
        if (item.Longitude && item.Latitude) {
            let marker = new L.Marker([item.Longitude, item.Latitude]);
            marker.addTo(map);
        }
    });

};