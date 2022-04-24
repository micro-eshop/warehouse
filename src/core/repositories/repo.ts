import { CatalogItemId, CatalogItemWarehouseState } from "../model/state";


export interface CatalogItemWarehouseStateRepository {
    getCatalogWarehouseState(catalogItemId: CatalogItemId): Promise<CatalogItemWarehouseState | null>;
    save(state: CatalogItemWarehouseState): Promise<void>
}