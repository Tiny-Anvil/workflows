import { expect, test } from 'vitest'
import { greeting } from './greeting'

test('greets', () => {
  expect(greeting('x')).toBe('Hello, x')
})
