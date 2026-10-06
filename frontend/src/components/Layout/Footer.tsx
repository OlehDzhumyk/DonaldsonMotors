import React from 'react';
import './Footer.css';
import { Link } from 'react-router-dom';

const Footer: React.FC = () => {
    const currentYear = new Date().getFullYear();
    return (
        <footer className="app-footer">
            <div className="footer-content">
                <div className="footer-section">
                    <h4>About Donaldson Motors</h4>
                    <p>Your trusted partner in car care. Providing efficient and trustworthy service for years.</p>
                </div>
                <div className="footer-section">
                    <h4>Quick Links</h4>
                    <p><Link to="/">Home</Link></p>
                    <p><Link to="/services">Services</Link></p>
                    <p><Link to="/book">Book Now</Link></p>
                    <p><Link to="/about">About Us</Link></p>
                </div>
                <div className="footer-section">
                    <h4>Contact Us</h4>
                    <p>123 Garage Lane, Hamilton, Scotland</p>
                    <p>Email: contact@donaldsonmotors.com</p>
                    <p>Phone: (012) 345-6789</p>
                </div>
            </div>
            <div className="footer-bottom">
                <p>&copy; {currentYear} Donaldson Motors©. All Rights Reserved.</p>
            </div>
        </footer>
    );
};

export default Footer;