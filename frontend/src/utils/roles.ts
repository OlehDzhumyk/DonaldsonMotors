export const Roles = {
    Manager: 'Manager',
    Mechanic: 'Mechanic',
    StockController: 'StockController',
    AccountsClerk: 'AccountsClerk',
    Customer: 'Customer',
} as const;

export type Role = typeof Roles[keyof typeof Roles];

/** The page each role lands on after logging in. */
export const HOME_BY_ROLE: Record<Role, string> = {
    Customer: '/my-bookings',
    Manager: '/dashboard',
    AccountsClerk: '/dashboard',
    Mechanic: '/my-jobs',
    StockController: '/manage/stock',
};
