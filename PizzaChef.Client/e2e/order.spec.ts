import { expect, test } from '@playwright/test';

test('can place an order when the window is open', async ({ page }) => {
  await page.goto('/');

  await expect(page.locator('.status-open')).toBeVisible();

  await page.getByTestId('add-pizza-margherita').click();
  await page.getByTestId('add-coca-cola-033').click();
  await expect(page.getByTestId('cart-total')).toBeVisible();

  await page.getByTestId('employee-name').fill('Playwright Tester');
  await page.getByTestId('submit-order').click();

  await expect(page.getByTestId('order-success')).toBeVisible();
});
