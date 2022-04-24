import { FastifyInstance } from "fastify";
import { Server, IncomingMessage, ServerResponse } from "http";


export default function(server: FastifyInstance<
    Server,
    IncomingMessage,
    ServerResponse
  >) {
    server.get("/ping", (req, resp) => {
        resp.send({ "message": "pong" });
    })
    
    server.get("/health", (req, resp) => {
        resp.code(200)
        resp.send({ "message": "healthy" });
    })
}