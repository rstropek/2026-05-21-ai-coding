import { expect, test } from '@playwright/test';

test('shows the menu and search filters items', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'Pizza Gabriel' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Speisekarte' })).toBeVisible();
  await expect(page.getByText('Pizza Margherita', { exact: true })).toBeVisible();

  await page.getByTestId('menu-search').fill('cola');
  await expect(page.getByText('Coca-Cola 0,33 l')).toBeVisible();
  await expect(page.getByText('Pizza Margherita', { exact: true })).toHaveCount(0);

  await page.getByTestId('menu-search').fill('');
  await expect(page.getByText('Pizza Margherita', { exact: true })).toBeVisible();
});
