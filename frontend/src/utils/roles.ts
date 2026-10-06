// src/utils/roles.ts
export const Roles = {
    Manager: 'Manager',
    Mechanic: 'Mechanic',
    StockController: 'StockController',
    AccountsClerk: 'AccountsClerk',
    Customer: 'Customer',
} as const;

export type Role = typeof Roles[keyof typeof Roles];