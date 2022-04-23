import { fastify } from 'fastify';
import pino from 'pino';
const Port = process.env.PORT || 7000;

const server = fastify({
    logger: pino({ level: 'info' })
});

server.get("/ping", (req, resp) => {
    resp.send({ "message": "pong" });
})

const start = async () => {
    try {
        await server.listen(Port);
        console.log('Server started successfully');
    } catch (err) {
        server.log.error(err);
        process.exit(1);
    }
};
start();