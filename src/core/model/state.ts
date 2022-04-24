
export type CatalogItemId = string

export interface CatalogItemWarehouseState {
    readonly catalogItemId: CatalogItemId;
    readonly availableQuantity: number;
    readonly reservedQuantity: number;
}

export function zero(id: CatalogItemId) : CatalogItemWarehouseState {
    return {
        catalogItemId: id,
        availableQuantity: 0,
        reservedQuantity: 0
    };
}

export function create(id: CatalogItemId, availableQuantity: number, reservedQuantity: number) : CatalogItemWarehouseState {
    return {
        catalogItemId: id,
        availableQuantity: availableQuantity,
        reservedQuantity: reservedQuantity
    };
}

export function getAvailableQuantity(state: CatalogItemWarehouseState) : number {
    const possibleQuantity = state.availableQuantity - state.reservedQuantity;
    if(possibleQuantity < 0) {
        return 0
    }
    return possibleQuantity;
}