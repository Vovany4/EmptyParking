import ws from 'k6/ws';
import { check } from 'k6';
import Amqp from 'k6/x/amqp';
import Queue from 'k6/x/amqp/queue';
import { Trend, Counter } from 'k6/metrics';
import { eventMessagesSet } from './TestData.js';

const latencyTrend = new Trend('latency', true);
const sendedCallsCounter = new Counter('sended_calls');
const receivedCallsCounter = new Counter('received_calls');

const sendMessageDelayMs = 50; 

const batchTimeOutMs = 10000;

const delayForLatencyMs = 2000; 
const sendingPeriodAtMs = 120000;
const userConnectionDurationAtMs = sendingPeriodAtMs + batchTimeOutMs + delayForLatencyMs;


const queueName = 'DemoQueue';

export let options = {
    vus: 1,   // Number of virtual users
    duration: `${userConnectionDurationAtMs}ms`, // Duration of the test
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
            }, sendMessageDelayMs); // Publish message every X milis
            socket.setTimeout(function() {
                allowPublishMessages = false;
            }, sendingPeriodAtMs); // Stop publish messages before close connection
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
                    let signalRRecordSeparator = msg.substring(msg.length - 1, msg.length);
                    let msgsWithoutSignalRProtocolEnding = msg.split(signalRRecordSeparator);
                    msgsWithoutSignalRProtocolEnding.pop(); // Remove end of message

                    msgsWithoutSignalRProtocolEnding.forEach((msgWithoutSignalRProtocolEnding) => {
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
                                latencyTrend.add(millisSendReceiveDiff)
                            }

                            receivedCallsCounter.add(1);
                        };
                    });
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
        }, userConnectionDurationAtMs); // Close after test durations
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

    let sendMsg = eventMessagesSet[(eventMessageCounter - 1) % 600];
    sendMsg.TimeStamp = Date.now();

    writeSendingMessage(sendMsg);

    return JSON.stringify(sendMsg);
}

function writeSendingMessage(msg) {
    sendedMessages[msg.Id] = msg.IsEmpty;
    sendedCallsCounter.add(1);
}