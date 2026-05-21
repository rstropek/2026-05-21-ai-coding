import { expect, test } from '@playwright/test';

test('shows the result from the Add API', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'Add-Funktion' })).toBeVisible();
  await expect(page.getByTestId('add-result')).toHaveText('3');
});
