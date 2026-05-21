import { defineConfig, devices } from '@playwright/test';

delete process.env['NO_COLOR'];

export default defineConfig({
  testDir: './e2e',
  webServer: [
    {
      command: 'dotnet run --project PizzaChef.Api/PizzaChef.Api.csproj --no-build --urls http://localhost:5252',
      cwd: '..',
      port: 5252,
      reuseExistingServer: true,
      timeout: 120_000,
    },
    {
      command: 'npm start -- --host 127.0.0.1 --port 4200 --watch=false --hmr=false',
      port: 4200,
      reuseExistingServer: true,
      timeout: 120_000,
    },
  ],
  use: {
    baseURL: 'http://127.0.0.1:4200',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
