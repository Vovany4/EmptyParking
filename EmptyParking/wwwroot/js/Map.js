var map;
var markers = [];
function setUpMap(data) {
    let mapOptions = {
        center: [50.0299, 19.9099],
        zoom: 13
    };

    map = L.map('map', mapOptions);

    let layer = new L.TileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png')
    map.addLayer(layer);

    /*L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);*/


    data.forEach((item) => {
        if (item.Longitude && item.Latitude) {
            var popupContent =
                '<p>Some Information</p></br>' +
                '<p>test</p></br>' +
                '<button onclick="clearMarker(' + item.Id + ')">Clear Marker</button>';

            let marker = new L.Marker([item.Longitude, item.Latitude]);
            marker._id = item.Id;
            marker.bindPopup(popupContent, {
                closeButton: false
            });
            map.addLayer(marker);
            markers.push(marker);
        }
    });

};

function clearMarker(id) {
    var index = -1;
    markers.forEach(function (marker) {
        if (marker._id == id) {
            map.removeLayer(marker);
            index = markers.indexOf(marker);
        }
    });

    if (index > -1) { 
        markers.splice(index, 1);
    }
}