import React from 'react';
import { Link } from 'react-router-dom';
import type { ServiceType } from '../../types/serviceType';
import { formatHours, formatMoney } from '../../utils/format';
import './ServiceCard.css';

const ServiceCard: React.FC<{ service: ServiceType }> = ({ service }) => (
    <article className="card service-card">
        <div className="card-body">
            <h3>{service.name}</h3>
            <p className="muted">{service.description}</p>
        </div>
        <div className="service-card-footer">
            <div>
                <span className="service-price">{formatMoney(service.price)}</span>
                <span className="muted small"> · {formatHours(service.durationHours)}</span>
            </div>
            <Link to={`/book?service=${String(service.id)}`} className="btn btn-sm btn-secondary">Book</Link>
        </div>
    </article>
);

export default ServiceCard;
