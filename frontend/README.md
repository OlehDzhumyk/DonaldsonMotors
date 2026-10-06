# Donaldson Motors web app

The React front end of [Donaldson Motors](../README.md). It's built with React 19, TypeScript, Redux Toolkit,
React Router, react-hook-form with yup, axios and Vite.

## Running it

With Docker Compose (from the repository root) the app is built and served by nginx at http://localhost:3000.
For development you need Node 22 and the API running on port 5080 (see the [backend README](../backend/README.md)):

```bash
cd frontend
npm ci
npm run dev          # http://localhost:5173
```

| Script              | What it does                                    |
|---------------------|-------------------------------------------------|
| `npm run dev`       | Vite dev server with hot reload                 |
| `npm run build`     | type check, then a production build in `dist/`  |
| `npm run typecheck` | `tsc -b` only                                   |
| `npm run lint`      | ESLint                                          |
| `npm test`          | Vitest, once                                    |

The app always calls the API on the same origin under `/api`. In development the Vite dev server forwards `/api` to
http://localhost:5080. In Docker, nginx forwards it to the `api` container ([`nginx.conf`](nginx.conf)). So there
are no API URLs to configure and no CORS to deal with in the browser.

## How it's built

```
src/
  pages/        one component per route
  components/   BookingCard, Modal, forms (PartForm, VehicleForm, ...), Layout, ProtectedRoute
  api/          one typed module per API area, all using apiClient (axios)
  app/          Redux store and the auth slice
  hooks/        typed Redux hooks, useServiceTypes
  types/        request and response types that match the API DTOs
  utils/        money and date formatting, error messages, roles, validation
  test/         Vitest setup and a render helper with a store
```

- **Auth.** Logging in returns a JWT and the user's role. Both live in the Redux auth slice and in `localStorage`,
  so a page reload keeps the user logged in. `apiClient` adds the `Authorization` header to every request. If the
  API answers 401 to a saved token (for example after it expires), the user is logged out.
- **Routes by role.** `ProtectedRoute` wraps each group of routes with the roles allowed to see them. Customers get
  `/book`, `/my-bookings` and `/profile`. Managers and accounts clerks get `/dashboard`. Managers also get
  `/manage/staff`, managers and stock controllers get `/manage/stock`, and mechanics get `/my-jobs`. The API checks
  the roles again; hiding a page is only for convenience.
- **Forms** use react-hook-form with yup schemas. The form types are inferred from the schemas, so the validation
  and the TypeScript types can't drift apart.
- **Errors.** The API answers errors with ProblemDetails. `getErrorMessage` in [`utils/format.ts`](src/utils/format.ts)
  turns them into one readable sentence: the `detail`, or the list of validation errors.
- **Types.** TypeScript runs in `strict` mode with `noUncheckedIndexedAccess`. ESLint uses `typescript-eslint`'s
  `strictTypeChecked` rules and reports no errors.

## Tests

`npm test` runs 17 Vitest tests with Testing Library and jsdom. They cover:

- which roles `ProtectedRoute` lets through;
- form validation;
- the booking card;
- money formatting;
- turning API errors into messages.

CI runs the type check, lint, tests and production build on every push.
