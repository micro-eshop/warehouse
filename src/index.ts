import { fastify, FastifyInstance } from 'fastify';
import { Server, IncomingMessage, ServerResponse } from 'http';
import pino from 'pino';
import health from './handlers/health';
const Port = process.env.PORT || 7000;

const server = fastify({
    logger: pino({ level: 'info' })
});

function setupEndpoints(server: FastifyInstance<
    Server,
    IncomingMessage,
    ServerResponse
>) {
    health(server);
}

const start = async () => {
    try {
        setupEndpoints(server);
        await server.listen(Port);
        console.log('Server started successfully');
    } catch (err) {
        server.log.error(err);
        process.exit(1);
    }
};
start();