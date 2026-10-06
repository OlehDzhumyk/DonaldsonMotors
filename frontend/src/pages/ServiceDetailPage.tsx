// src/pages/ServiceDetailPage.tsx
import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { servicesList } from '../api/mockData';
import './ServiceDetailPage.css';

const ServiceDetailPage: React.FC = () => {
    // The useParams hook reads the dynamic 'id' from the URL
    const { id } = useParams<{ id: string }>();

    // Find the service in our mock data that matches the id from the URL
    // We use parseInt because the id from URL is a string
    const service = servicesList.find(s => s.id === parseInt(id || ''));

    // What to show if a service with this ID is not found
    if (!service) {
        return (
            <div className="not-found">
                <h1>404 - Service Not Found</h1>
                <p>We couldn't find the service you're looking for.</p>
                <Link to="/services">Back to all services</Link>
            </div>
        );
    }

    return (
        <div className="service-detail-page">
            <header className="service-detail-header">
                <div className="service-detail-icon">{service.icon}</div>
                <div className="service-detail-title">
                    <h1>{service.title}</h1>
                    <div className="service-meta">
                        <span><strong>Price:</strong> {service.price}</span>
                        <span><strong>Duration:</strong> {service.duration}</span>
                    </div>
                </div>
            </header>
            <section className="service-detail-body">
                <h2>What's Included?</h2>
                <ul>
                    {service.details.map((detail, index) => (
                        <li key={index}>{detail}</li>
                    ))}
                </ul>
            </section>
        </div>
    );
};

export default ServiceDetailPage;