import React from 'react';
import Header from './Header'; // Перевірте імпорт
import Footer from './Footer'; // Перевірте імпорт
import './Layout.css';

const Layout = ({ children }: { children: React.ReactNode }) => {
    return (
        <div className="layout-container">
            <Header />
            <main className="main-content">
                {children}
            </main>
            <Footer />
        </div>
    );
};

export default Layout;