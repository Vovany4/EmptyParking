"use strict"

//const { signalR } = require("../lib/signalr/dist/browser/signalr.js")

var connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5083/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {

        await connection.start().then(() => {

            //connection.invoke("SendMessage", 999, false);
            connection.on("ReceiveMessage", function (spotId, isEmpty, latitude, longitude, timeStamp) {
                debugger;
                var li = document.createElement("li");
                li.textContent = `SpotId: ${spotId}, IsEmpty: ${isEmpty}, Latitude: ${latitude}, Longitude: ${longitude}, TimeStamp: ${timeStamp}`;
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