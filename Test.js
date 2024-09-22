import ws from 'k6/ws';
import { check } from 'k6';
import Amqp from 'k6/x/amqp';
import Queue from 'k6/x/amqp/queue';
import { Gauge } from 'k6/metrics';
import { eventMessages } from './TestData.js';

const myGauge = new Gauge('millisSendReceiveDiff', true);
const testDuration = 300000;

export let options = {
    vus: 1,   // Number of virtual users
    duration: `${testDuration}ms`, // Duration of the test
    cloud: {
        // Project: Default project
        projectID: 3712534,
        // Test runs with the same name groups test runs together.
        name: 'Test run demo'
    }
};

let eventMessageCounter = 0;
let receivedMessages = {};
let sendedMessages = {};
const queueName = 'DemoQueue';

export default function () {
    const signalRUrl = 'ws://localhost:5083/notificationHub';

    amqpConnection();

    const res = ws.connect(signalRUrl, function (socket) {

        socket.on('open', function () {
            console.log('Connected to SignalR');
            sendSignalRInitialHandshake(socket);

            var allowPublishMessages = true;
            socket.setInterval(function timeout() {
                if (allowPublishMessages) {
                    amqpPublish();
                }
            }, 500); // Publish message every 0.5 sec
            socket.setTimeout(function () {
                allowPublishMessages = false;
            }, testDuration - 31000); // Stop publish messages before close connection
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

                    if (parsedMessage.type === 1 && parsedMessage.target === "BatchReceiveMessage") {

                        let dateNow = Date.now();

                        for (var key in parsedMessage.arguments[0]) {
                            let receivedMessage = parsedMessage.arguments[0][key];

                            const millisSendReceiveDiff = dateNow - receivedMessage.timeStamp;

                            let receivedValues = {
                                "IsEmpty": receivedMessage.isEmpty,
                                "MillisSendReceiveDiff": millisSendReceiveDiff
                            };

                            receivedMessages[receivedMessage.id] = receivedValues;
                            myGauge.add(millisSendReceiveDiff);
                        }
                    };
            }

        });

        socket.on('close', function () {
            console.log('Disconnected from SignalR');
            console.log(receivedMessages);
        });

        socket.on('error', function (e) {
            console.log('WebSocket error: ', e.error());
        });

        // Keep the WebSocket open for a while
        socket.setTimeout(function () {
            console.log('Closing WebSocket connection');
            socket.close();
        }, testDuration); // Close after test durations


    });

    check(res, { 'Connected successfully': (r) => r && r.status === 101 });

    let dynamicChecks = {};
    Object.entries(sendedMessages).forEach(([key, val]) => {
        dynamicChecks[`Expected result comparison Spot[${key}]`] = (r) => r[key] && r[key].IsEmpty == val;
    });
    check(receivedMessages, dynamicChecks);
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

    writeSendingMessage(sendMsg);

    return JSON.stringify(sendMsg);
}

function writeSendingMessage(msg) {
    sendedMessages[msg.Id] = msg.IsEmpty;
}