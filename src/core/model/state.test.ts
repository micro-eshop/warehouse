import { zero, getAvailableQuantity, create } from './state';

test('test get available quantity when state is empty', () => {
    const id = "test"
    const state = zero(id)
    expect(state.catalogItemId).toBe(id)
    expect(getAvailableQuantity(state)).toBe(0)
});

test('test get available quantity when state is not empty', () => {
    const id = "test"
    const state = create(id, 10, 5)
    expect(state.catalogItemId).toBe(id)
    expect(getAvailableQuantity(state)).toBe(5)
});