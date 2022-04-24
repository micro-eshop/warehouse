import { string } from "fp-ts";
import Redis from "ioredis";
import { CatalogItemId, CatalogItemWarehouseState } from "../../core/model/state";
import { CatalogItemWarehouseStateRepository } from "../../core/repositories/repo";

function getKey(id: CatalogItemId): string {
    return `warehouse:${id}`;
}

export default class RedisCatalogItemWarehouseStateRepository implements CatalogItemWarehouseStateRepository {
    #redis: Redis

    constructor(redis: Redis) {
        this.#redis = redis;
    }
    
    async save(state: CatalogItemWarehouseState): Promise<void> {
        const json = JSON.stringify(state)
        const key = getKey(state.catalogItemId)
        await this.#redis.set(key, json)
    }

    async getCatalogWarehouseState(catalogItemId: CatalogItemId): Promise<CatalogItemWarehouseState | null> {
        const key = getKey(catalogItemId)
        const result = await this.#redis.get(key);
        if(result === null) {
            return null
        }
        return JSON.parse(result)
    }
}