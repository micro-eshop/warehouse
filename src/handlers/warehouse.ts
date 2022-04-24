import { FastifyInstance } from "fastify";
import { Server, IncomingMessage, ServerResponse } from "http";


export default function(server: FastifyInstance<
    Server,
    IncomingMessage,
    ServerResponse
  >) {
    server.get("/warehouse/{catalog_id}/state", (req, resp) => {
        resp.send({ "message": "pong" });
    })
    
}