import React from 'react';
import { Link } from 'react-router-dom';

const OPENING_HOURS = [
    { days: 'Monday – Friday', hours: '09:00 – 17:00' },
    { days: 'Lunch break', hours: '13:00 – 14:00' },
    { days: 'Saturday – Sunday', hours: 'Closed' },
];

const AboutUsPage: React.FC = () => (
    <div className="container page">
        <div className="page-header">
            <div>
                <h1>Quality car care you can trust</h1>
                <p className="subtitle">An independent garage in Hamilton, Scotland.</p>
            </div>
            <Link to="/book" className="btn btn-primary">Book a service</Link>
        </div>

        <div className="grid grid-2" style={{ alignItems: 'start' }}>
            <section className="card card-body stack">
                <h2>What we do</h2>
                <p className="muted">
                    At Donaldson Motors we provide reliable servicing and repairs, from routine maintenance
                    like oil changes to complex engine work. Our mechanics use up-to-date equipment, and we
                    keep you informed by email at every step: booking, mechanic assigned, job finished and invoice.
                </p>
                <p className="muted">We pride ourselves on clear communication and honest work.</p>
            </section>

            <section className="card">
                <div className="card-header"><h2>Opening hours</h2></div>
                <table className="table">
                    <tbody>
                        {OPENING_HOURS.map(row => (
                            <tr key={row.days}>
                                <td>{row.days}</td>
                                <td className="num">{row.hours}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <div className="card-footer" style={{ justifyContent: 'flex-start' }}>
                    <span className="muted small">123 Garage Lane, Hamilton, Scotland · (012) 345-6789</span>
                </div>
            </section>
        </div>
    </div>
);

export default AboutUsPage;
