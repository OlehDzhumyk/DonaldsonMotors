import React from 'react';
import ServiceCard from '../components/ServiceCard/ServiceCard';
import { useServiceTypes } from '../hooks/useServiceTypes';

const ServicesPage: React.FC = () => {
    const { serviceTypes, isLoading, error } = useServiceTypes();

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>Services and prices</h1>
                    <p className="subtitle">Prices are for the service itself. Any parts used are added to the final invoice.</p>
                </div>
            </div>

            {isLoading && <p className="loading">Loading services…</p>}
            {error && <p className="alert alert-error">{error}</p>}
            <div className="grid grid-3">
                {serviceTypes.map(service => <ServiceCard key={service.id} service={service} />)}
            </div>
        </div>
    );
};

export default ServicesPage;
