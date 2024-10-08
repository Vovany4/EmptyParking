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

    data.forEach((item) => {
        addMarker(item.Id, item.Longitude, item.Latitude);
    });

};

function connectClientNotification() {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5083/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function start() {
        try {

            await connection.start().then(() => {
                connection.on("BatchReceiveMessage", function (spots) {
                    spots.forEach((spot) => { 
                        if (spot.isEmpty) {
                            addMarker(spot.id, spot.longitude, spot.latitude);
                        } else {
                            clearMarker(spot.id);
                        }

                        var li = document.createElement("li");
                        li.textContent = `Id: ${spot.id}, IsEmpty: ${spot.isEmpty}, Latitude: ${spot.latitude}, Longitude: ${spot.longitude}, TimeStamp: ${spot.timeStamp}`;
                        document.getElementById("msgList").appendChild(li);
                    });
                });

            });

        } catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    };

    connection.onclose(async () => {
        await start();
    });

    // Start the connection.
    start();
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

function addMarker(itemId, longitude, latitude) {
    if (longitude && latitude) {
        var popupContent =
            '<p>Some Information</p></br>' +
            '<p>test</p></br>' +
            '<button onclick="clearMarker(' + itemId + ')">Clear Marker</button>';

        let marker = new L.Marker([longitude, latitude]);
        marker._id = itemId;
        marker.bindPopup(popupContent, {
            closeButton: false
        });
        map.addLayer(marker);
        markers.push(marker);
    }
}