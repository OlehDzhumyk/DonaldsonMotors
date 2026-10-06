import React from 'react';
import { Link } from 'react-router-dom';
import BrandMark from './BrandMark';
import './Footer.css';

const Footer: React.FC = () => (
    <footer className="site-footer">
        <div className="container site-footer-inner">
            <div className="site-footer-brand">
                <BrandMark />
                <div>
                    <strong>Donaldson Motors</strong>
                    <p className="muted small">123 Garage Lane, Hamilton, Scotland · (012) 345-6789</p>
                </div>
            </div>
            <nav className="site-footer-links">
                <Link to="/services">Services</Link>
                <Link to="/about">About</Link>
                <Link to="/book">Book a service</Link>
            </nav>
            <p className="muted small">&copy; {new Date().getFullYear()} Donaldson Motors</p>
        </div>
    </footer>
);

export default Footer;
