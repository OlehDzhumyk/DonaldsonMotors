// src/pages/ServicesPage.tsx
import React from 'react';
import { servicesList } from '../api/mockData'; // Import our mock data
import ServiceCard from '../components/ServiceCard/ServiceCard';
import './ServicesPage.css';

const ServicesPage: React.FC = () => {
    // In a real app, you would use useEffect to fetch this data from an API
    // For now, we just use the imported mock data.
    const services = servicesList;

    return (
        <div className="services-page">
            <h1>Our Services</h1>
            <div className="services-grid">
                {services.map(service => (
                    <ServiceCard key={service.id} service={service} />
                ))}
            </div>
        </div>
    );
};

export default ServicesPage;