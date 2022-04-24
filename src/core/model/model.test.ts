import { zero, getAvailableQuantity } from './model';

test('test get available quantity', () => {
    const id = "test"
    const state = zero(id)
    expect(state.catalogItemId).toBe(id)
    expect(getAvailableQuantity(state)).toBe(0)
});