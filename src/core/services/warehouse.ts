import { CatalogItemId, getAvailableQuantity } from "../model/state";
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