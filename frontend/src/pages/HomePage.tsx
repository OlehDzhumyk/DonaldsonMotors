import React from 'react';
import { Link } from 'react-router-dom';
import ServiceCard from '../components/ServiceCard/ServiceCard';
import { useServiceTypes } from '../hooks/useServiceTypes';
import './HomePage.css';

const STEPS = [
    { title: 'Pick a service', text: 'Choose what your car needs and see the price up front.' },
    { title: 'Choose a time', text: 'Free slots are shown for the next two weeks.' },
    { title: 'Get updates by email', text: 'Confirmation, mechanic assigned, job done and a paid invoice.' },
];

const HomePage: React.FC = () => {
    const { serviceTypes, isLoading, error } = useServiceTypes();

    return (
        <>
            <section className="hero">
                <div className="container hero-inner">
                    <div className="hero-copy">
                        <span className="hero-eyebrow">Independent garage · Hamilton</span>
                        <h1>Book your car service online in under a minute.</h1>
                        <p>
                            From an oil change to a full annual service. Pick a time that suits you and
                            follow the job from booking to invoice.
                        </p>
                        <div className="row">
                            <Link to="/book" className="btn btn-lg btn-primary">Book a service</Link>
                            <Link to="/services" className="btn btn-lg hero-btn-outline">See prices</Link>
                        </div>
                    </div>

                    <div className="card hero-steps">
                        <div className="card-header"><h2>How booking works</h2></div>
                        <ol className="steps">
                            {STEPS.map((step, index) => (
                                <li key={step.title}>
                                    <span className="step-number">{index + 1}</span>
                                    <div>
                                        <h3>{step.title}</h3>
                                        <p className="muted small">{step.text}</p>
                                    </div>
                                </li>
                            ))}
                        </ol>
                    </div>
                </div>
            </section>

            <section className="container page">
                <div className="page-header">
                    <div>
                        <h2 className="section-title">Services and prices</h2>
                        <p className="subtitle">Prices are for the service itself. Any parts used are added to the final invoice.</p>
                    </div>
                    <Link to="/services">All services →</Link>
                </div>

                {isLoading && <p className="loading">Loading services…</p>}
                {error && <p className="alert alert-error">{error}</p>}
                <div className="grid grid-3">
                    {serviceTypes.slice(0, 3).map(service => <ServiceCard key={service.id} service={service} />)}
                </div>
            </section>
        </>
    );
};

export default HomePage;
