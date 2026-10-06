// src/pages/HomePage.tsx
import React from 'react';
import { Link } from 'react-router-dom';

// Simple inline styles. Consider moving to a separate CSS file for larger projects.
const styles: { [key: string]: React.CSSProperties } = {
    // Hero section with blue background
    hero: {
        textAlign: 'center',
        padding: '100px 20px',
        backgroundColor: '#005A9C', // Royal Blue
        color: 'white',
    },
    title: {
        fontSize: '3rem',
        fontWeight: 'bold',
        marginBottom: '1rem',
    },
    subtitle: {
        fontSize: '1.5rem',
        marginBottom: '2rem',
        maxWidth: '600px',
        margin: '0 auto 3rem auto',
        opacity: 0.9,
    },
    ctaButton: {
        backgroundColor: 'white',
        color: '#005A9C',
        padding: '15px 35px',
        border: 'none',
        borderRadius: '50px', // Pill-shaped button
        textDecoration: 'none',
        fontWeight: 'bold',
        fontSize: '1.1rem',
        cursor: 'pointer',
        transition: 'transform 0.2s ease',
    },
    // Services section with white background
    servicesSection: {
        padding: '80px 20px',
        textAlign: 'center',
        backgroundColor: '#ffffff', // White background
        color: '#333', // Dark text
    },
    sectionTitle: {
        fontSize: '2.5rem',
        marginBottom: '4rem',
    },
    servicesGrid: {
        display: 'flex',
        justifyContent: 'center',
        gap: '30px',
        flexWrap: 'wrap',
        maxWidth: '1200px',
        margin: '0 auto',
    },
    serviceCard: {
        backgroundColor: '#f9f9f9',
        padding: '30px',
        borderRadius: '10px',
        boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
        width: '300px',
        textAlign: 'left',
    },
    serviceCardTitle: {
        fontSize: '1.5rem',
        marginBottom: '1rem',
        color: '#005A9C',
    },
};

const HomePage: React.FC = () => {
    // Example services data
    const services = [
        { title: "Engine Diagnostics", description: "State-of-the-art equipment to diagnose and fix engine issues." },
        { title: "Oil & Filter Change", description: "Keep your engine running smoothly with regular oil changes." },
        { title: "Tyres & Brakes", description: "Full tyre and brake inspection, repair, and replacement services." },
    ];

    return (
        <div>
            {/* Hero Section */}
            <section style={styles.hero}>
                <h1 style={styles.title}>Your Trusted Partner in Car Care</h1>
                <p style={styles.subtitle}>Efficient and trustworthy service, from an oil change to an engine repair.</p>
                <Link to="/book" style={styles.ctaButton} onMouseOver={e => e.currentTarget.style.transform = 'scale(1.05)'} onMouseOut={e => e.currentTarget.style.transform = 'scale(1)'}>
                    Book a Service
                </Link>
            </section>

            {/* Services Section */}
            <section style={styles.servicesSection}>
                <h2 style={styles.sectionTitle}>Our Core Services</h2>
                <div style={styles.servicesGrid}>
                    {services.map((service, index) => (
                        <div key={index} style={styles.serviceCard}>
                            <h3 style={styles.serviceCardTitle}>{service.title}</h3>
                            <p>{service.description}</p>
                        </div>
                    ))}
                </div>
            </section>
        </div>
    );
};

export default HomePage;