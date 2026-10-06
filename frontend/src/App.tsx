// src/App.tsx
import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import { Roles } from './utils/roles';

// Import Pages
import HomePage from './pages/HomePage';
import ServicesPage from './pages/ServicesPage';
import ServiceDetailPage from './pages/ServiceDetailPage';
import AboutUsPage from './pages/AboutUsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import UnauthorizedPage from './pages/UnauthorizedPage';
import MyProfilePage from './pages/MyProfilePage';
import BookingPage from './pages/BookingPage'; // Customer's booking creation page

// Role-specific pages
import ManagerDashboardPage from './pages/ManagerDashboardPage';
import MyBookingsPage from './pages/MyBookingsPage'; // Customer's bookings list
import MyJobsPage from './pages/MyJobsPage';
import RegisterStaffPage from "./pages/RegisterStaffPage.tsx";
import StockManagementPage from "./pages/StockManagementPage.tsx";       // Mechanic's jobs list

function App() {
    return (
        <Layout>
            <Routes>
                {/* === Public Routes === */}
                <Route path="/" element={<HomePage />} />
                <Route path="/services" element={<ServicesPage />} />
                <Route path="/services/:id" element={<ServiceDetailPage />} />
                <Route path="/about" element={<AboutUsPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/unauthorized" element={<UnauthorizedPage />} />

                {/* ... (Common Protected Routes like /profile for Customer) ... */}
                <Route element={<ProtectedRoute allowedRoles={[Roles.Customer]} />}>
                    <Route path="/my-bookings" element={<MyBookingsPage />} />
                    <Route path="/book" element={<BookingPage />} />
                    <Route path="/profile" element={<MyProfilePage />} />
                </Route>

                {/* === Manager Specific Routes === */}
                <Route element={<ProtectedRoute allowedRoles={[Roles.Manager]} />}>
                    <Route path="/dashboard" element={<ManagerDashboardPage />} />
                    <Route path="/manage/staff" element={<RegisterStaffPage />} />
                    {/* Manager also gets access to stock if not covered below */}
                </Route>

                {/* === Routes for Manager AND StockController === */}
                <Route element={<ProtectedRoute allowedRoles={[Roles.Manager, Roles.StockController]} />}>
                    <Route path="/manage/stock" element={<StockManagementPage />} />
                </Route>

                {/* === Mechanic Specific Routes === */}
                <Route element={<ProtectedRoute allowedRoles={[Roles.Mechanic]} />}>
                    <Route path="/my-jobs" element={<MyJobsPage />} />
                </Route>

                {/* Catch-all for any other route - redirects to home */}
                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Layout>
    );
}

export default App;