import { FastifyInstance } from "fastify";
import { Server, IncomingMessage, ServerResponse } from "http";


export default function(server: FastifyInstance<
    Server,
    IncomingMessage,
    ServerResponse
  >) {
    server.get("/warehouse/:catalogid/state", (req, resp) => {
        const p = req.params as any;
        resp.send({ "message": `pong ${p.catalogid}` });
    })
    
}