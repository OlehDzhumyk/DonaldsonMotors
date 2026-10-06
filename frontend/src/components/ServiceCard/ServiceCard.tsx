// src/components/ServiceCard/ServiceCard.tsx
import React from 'react';
import { Link } from 'react-router-dom';
import type {Service} from '../../api/mockData'; // Import the type
import './ServiceCard.css';

interface ServiceCardProps {
    service: Service;
}

const ServiceCard: React.FC<ServiceCardProps> = ({ service }) => {
    return (
        <div className="service-card">
            <div className="service-icon">{service.icon}</div>
            <h3>{service.title}</h3>
            <p>{service.description}</p>
            <Link to={`/services/${service.id}`} className="learn-more-btn">
                Learn More
            </Link>
        </div>
    );
};

export default ServiceCard;