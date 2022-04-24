import { Option } from "fp-ts/lib/Option";
import { CatalogItemWarehouseState } from "../model/state";


export interface CatalogItemWarehouseStateRepository {
    getCatalogWarehouseState(catalogItemId: string): Promise<CatalogItemWarehouseState | null>;
}