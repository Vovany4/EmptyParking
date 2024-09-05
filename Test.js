import ws from 'k6/ws';
import { check } from 'k6';
import Amqp from 'k6/x/amqp';
import Queue from 'k6/x/amqp/queue';
import { eventMessages, expectedResult } from './TestData.js';

export let options = {
    vus: 1,   // Number of virtual users
    duration: '15s', // Duration of the test
    cloud: {
        // Project: Default project
        projectID: 3712534,
        // Test runs with the same name groups test runs together.
        name: 'Test run demo'
    }
};

let eventMessageCounter = 0;
let finalResult = {};
const queueName = 'DemoQueue';

export default function () {
    const signalRUrl = 'ws://localhost:5083/notificationHub';

    amqpConnection();

    const res = ws.connect(signalRUrl, function (socket) {

        socket.on('open', function () {
            console.log('Connected to SignalR');
            sendSignalRInitialHandshake(socket);

            socket.setInterval(function timeout() {
                amqpPublish();
            }, 3000); // Publish message every 3 sec
        });


        socket.on('message', function (msg) {
            switch (msg) {
                case '{}\x1e':
                    // This is the protocol confirmation
                    break;
                case '{"type":6}\x1e':
                    // Received handshake
                    break;
                default:
                    console.log(`Received message: ${msg}`);
                    let msgWithoutSignalRProtocolEnding = msg.substring(0, msg.length - 1);
                    let parsedMessage = JSON.parse(msgWithoutSignalRProtocolEnding);

                    if (parsedMessage.type === 1 && parsedMessage.target === "ReceiveMessage") {

                        let spotId = parsedMessage.arguments[0];
                        let isEmpty = parsedMessage.arguments[1];
                        let latitude = parsedMessage.arguments[2];
                        let longitude = parsedMessage.arguments[3];
                        let timeStamp = parsedMessage.arguments[4];

                        console.log('params ' + parsedMessage.arguments);


                        const millisSendReceiveDiff = Date.now() - timeStamp;

                        let receivedValues = {
                            "IsEmpty": isEmpty,
                            "MillisSendReceiveDiff": millisSendReceiveDiff
                        };

                        finalResult[spotId] = receivedValues;
                    };
            }

            // You can add more checks here to validate the messages received
            /*check(msg, {
                'message contains expected text': (m) => m.indexOf('expected_text') !== -1,
            });*/
        });

        socket.on('close', function () {
            console.log('Disconnected from SignalR');
            console.log(finalResult);
        });

        socket.on('error', function (e) {
            console.log('WebSocket error: ', e.error());
        });

        // Keep the WebSocket open for a while
        socket.setTimeout(function () {
            console.log('Closing WebSocket connection');
            socket.close();
        }, 15000); // Close after 15 seconds


    });

    check(res, { 'status is 101 (switching protocols)': (r) => r && r.status === 101 });
    check(finalResult, {
        'Expected result comparison Spot[2]': (r) => r[2] && r[2].IsEmpty == expectedResult[2],
        'Expected result comparison Spot[3]': (r) => r[3] && r[3].IsEmpty == expectedResult[3],
        'Expected result comparison Spot[4]': (r) => r[4] && r[4].IsEmpty == expectedResult[4],
        'Expected result comparison Spot[5]': (r) => r[5] && r[5].IsEmpty == expectedResult[5],
        'Expected result comparison Spot[5]': (r) => r[6] && r[6].IsEmpty == expectedResult[6],
        'Expected result comparison Spot[6]': (r) => r[7] && r[7].IsEmpty == expectedResult[7],

    });
}

function amqpConnection() {
    const amqpUrl = "amqp://guest:guest@localhost:5672/";

    Amqp.start({
        connection_url: amqpUrl
    });
    console.log("Connection opened: " + amqpUrl);

    Queue.declare({ name: queueName });

    console.log(queueName + " queue is ready");
}

function amqpPublish() {
    Amqp.publish({
        queue_name: queueName,
        body: getAMQPEventMessage(),
        content_type: "text/plain"
    });
}

function sendSignalRInitialHandshake(socket) {
    const handshakeMessage = JSON.stringify({
        protocol: "json",
        version: 1
    });
    socket.send(`${handshakeMessage}\x1e`);
}

function getAMQPEventMessage() {
    eventMessageCounter += 1;
    console.log('Getting message');

    let sendMsg = eventMessages[eventMessageCounter - 1];
    sendMsg.TimeStamp = Date.now();

    return JSON.stringify(sendMsg);
}