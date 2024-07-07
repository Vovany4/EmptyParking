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

                //connection.invoke("SendMessage", 999, false);
                connection.on("ReceiveMessage", function (spotId, isEmpty, latitude, longitude) {
                    debugger;
                    if (!isEmpty) {
                        clearMarker(spotId);
                    } else {
                        addMarker(spotId, longitude, latitude);
                    }

                    var li = document.createElement("li");
                    li.textContent = `SpotId: ${spotId}, IsEmpty: ${isEmpty}, Latitude: ${latitude}, Longitude: ${longitude}`;
                    document.getElementById("msgList").appendChild(li);
                });
            }); /*/notificationHub*/

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