import React from 'react';
import './AboutUsPage.css';

const AboutUsPage: React.FC = () => {
    return (
        <div className="about-us-page">
            <header className="page-header">
                <h1>Quality Car Care You Can Trust</h1>
            </header>

            <div className="main-content-wrapper">
                <section className="info-section">
                    <h2>What We Do</h2>
                    <p>
                        At Donaldson Motors, we are dedicated to providing top-tier, reliable automotive services. From routine maintenance like oil changes and MOT testing to complex engine repairs, our team of certified mechanics uses the latest technology to ensure your vehicle performs at its best. We pride ourselves on transparent communication and honest work.
                    </p>

                    <h2 style={{ marginTop: '40px' }}>Key Information</h2>
                    <ul className="info-list">
                        <li>
                            <strong>Mon - Fri:</strong> 8:00 AM - 6:00 PM
                        </li>
                        <li>
                            <strong>Saturday:</strong> 9:00 AM - 1:00 PM
                        </li>
                        <li>
                            <strong>Sunday:</strong> Closed
                        </li>
                        <li style={{ paddingTop: '10px' }}>
                            <strong>Payments:</strong> Credit/Debit Card, PayPal, Stripe
                        </li>
                    </ul>
                </section>

                <section className="map-section">
                    <h2>Find Us</h2>
                    {/* This is an embedded Google Map.
          */}
                    <iframe
                        src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d71803.51864161988!2d-4.110531526437936!3d55.77353163351608!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x488841bae41341d1%3A0x838a54e994c55217!2sHamilton!5e0!3m2!1sen!2suk!4v1716624734568!5m2!1sen!2suk"
                        allowFullScreen={true}
                        loading="lazy"
                        referrerPolicy="no-referrer-when-downgrade"
                        title="Donaldson Motors Location"
                    ></iframe>
                </section>
            </div>
        </div>
    );
};

export default AboutUsPage;