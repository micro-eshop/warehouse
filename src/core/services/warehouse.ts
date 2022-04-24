import { CatalogItemId, CatalogItemWarehouseState, getAvailableQuantity } from "../model/state";
import { CatalogItemWarehouseStateRepository } from "../repositories/repo";

export function getCatalogWarehouseItemAvailableQuantity(repo: CatalogItemWarehouseStateRepository) {
    return async (id: CatalogItemId) => {
        const itemOpt = await repo.getCatalogWarehouseState(id);
        if(itemOpt === null) {
            return 0;
        }
        return getAvailableQuantity(itemOpt);
    }
}

export function saveCatalogWarehouseState(repo: CatalogItemWarehouseStateRepository) {
    return async (state: CatalogItemWarehouseState) => {
        await repo.save(state);
    }
}

