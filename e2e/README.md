# e2e (Playwright) — reserved

Phase 1b stub: **no specs yet**.

Workflow: `.github/workflows/playwright.yml` stays **green** with an explicit stub when zero `e2e/**/*.spec.*` files exist.

When UI automation is scheduled (**Stagecraft QA** via **Gatekeeper Nova**):

1. Add Playwright config + specs under `e2e/`
2. Extend the workflow to `npm ci`, install browsers, and run `npx playwright test`
3. Prefer any future web admin/gallery surfaces; pure WPF desktop may need a different harness (WinAppDriver / FlaUI) owned under QA planning

Related: work_id **WRQ-WIN-001**, stage board in Agency Desks vault.
