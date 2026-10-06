import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import { Roles } from './utils/roles';
import HomePage from './pages/HomePage';
import ServicesPage from './pages/ServicesPage';
import AboutUsPage from './pages/AboutUsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import UnauthorizedPage from './pages/UnauthorizedPage';
import MyBookingsPage from './pages/MyBookingsPage';
import BookingPage from './pages/BookingPage';
import MyProfilePage from './pages/MyProfilePage';
import ManagerDashboardPage from './pages/ManagerDashboardPage';
import RegisterStaffPage from './pages/RegisterStaffPage';
import StockManagementPage from './pages/StockManagementPage';
import MyJobsPage from './pages/MyJobsPage';

function App() {
    return (
        <Layout>
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/services" element={<ServicesPage />} />
                <Route path="/about" element={<AboutUsPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/unauthorized" element={<UnauthorizedPage />} />

                <Route element={<ProtectedRoute allowedRoles={[Roles.Customer]} />}>
                    <Route path="/my-bookings" element={<MyBookingsPage />} />
                    <Route path="/book" element={<BookingPage />} />
                    <Route path="/profile" element={<MyProfilePage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={[Roles.Manager, Roles.AccountsClerk]} />}>
                    <Route path="/dashboard" element={<ManagerDashboardPage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={[Roles.Manager]} />}>
                    <Route path="/manage/staff" element={<RegisterStaffPage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={[Roles.Manager, Roles.StockController]} />}>
                    <Route path="/manage/stock" element={<StockManagementPage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={[Roles.Mechanic]} />}>
                    <Route path="/my-jobs" element={<MyJobsPage />} />
                </Route>

                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Layout>
    );
}

export default App;
