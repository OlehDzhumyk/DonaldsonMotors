
export interface Service {
    id: number;
    title: string;
    description: string;
    icon: string;
    price: string;
    duration: string;
    details: string[];
}


export interface ServiceType {
    id: number;
    name: string;
    estimatedDurationMinutes: number; // Example: 60 for 1 hour
    description?: string;
}

export const serviceTypesList: ServiceType[] = [
    { id: 1, name: 'Oil & Filter Change', estimatedDurationMinutes: 60, description: 'Standard oil and filter replacement.' },
    { id: 2, name: 'Brake Inspection', estimatedDurationMinutes: 45, description: 'Visual inspection of brake system.' },
    { id: 3, name: 'Engine Diagnostics', estimatedDurationMinutes: 90, description: 'Full computer diagnostics.' },
    { id: 4, name: 'Tyre Rotation', estimatedDurationMinutes: 30, description: 'Rotation of tyres for even wear.' },
    { id: 5, name: 'MOT Pre-Check', estimatedDurationMinutes: 120, description: 'Preliminary check before official MOT.' },
    // Add more service types as needed by your application
];


export const servicesList: Service[] = [
    {
        id: 1,
        title: 'Engine Diagnostics',
        description: 'State-of-the-art equipment to diagnose and fix engine issues, ensuring peak performance.',
        icon: '⚙️',
        price: '£75',
        duration: '1-2 hours',
        details: [
            'Full computerised diagnostics scan.',
            'Check engine warning light analysis.',
            'Report of fault codes and recommended repairs.',
            'Pressure tests for fuel, oil, and compression.'
        ]
    },
    {
        id: 2,
        title: 'Oil & Filter Change',
        description: 'Essential for engine longevity. We use premium oils and filters to keep your engine running smoothly.',
        icon: '🛢️',
        price: 'Starting from £90',
        duration: '45 minutes',
        details: [
            'Drain old engine oil and replace with premium grade oil.',
            'Replacement of the oil filter with a high-quality part.',
            'Check and top-up of all essential fluids.',
            'Reset service light.'
        ]
    },
    {
        id: 3,
        title: 'Brake Services',
        description: 'Comprehensive brake inspection, repair, and replacement to guarantee your safety on the road.',
        icon: '🛑',
        price: 'Quote upon inspection',
        duration: '2-3 hours',
        details: [
            'Inspection of brake pads, discs, calipers, and hoses.',
            'Measurement of brake fluid levels and quality.',
            'Replacement of worn components with OEM-spec parts.',
            'Brake fluid bleeding and replacement.'
        ]
    },
    {
        id: 4,
        title: 'Tyre Services',
        description: 'Including tyre fitting, balancing, alignment, and puncture repairs for all major brands.',
        icon: '🚗',
        price: 'Starting from £20 per tyre',
        duration: '30-60 minutes',
        details: [
            'Professional fitting of new tyres.',
            'Digital wheel balancing for a smooth ride.',
            '4-wheel laser alignment.',
            'Safe and reliable puncture repairs.'
        ]
    },
    {
        id: 5,
        title: 'Air Conditioning',
        description: 'Full AC system check, re-gassing, and repairs to keep you cool during the summer.',
        icon: '❄️',
        price: '£60',
        duration: '1 hour',
        details: [
            'AC refrigerant level check and recharge (re-gas).',
            'System leak detection and pressure testing.',
            'Antibacterial cleaning to remove odours.',
            'Full performance test.'
        ]
    },
    {
        id: 6,
        title: 'MOT Testing',
        description: 'Certified MOT testing to ensure your vehicle meets all legal safety and environmental standards.',
        icon: '📋',
        price: '£54.85 (Standard Rate)',
        duration: '1 hour',
        details: [
            'Comprehensive inspection covering all required MOT checks.',
            'Includes lighting, suspension, brakes, and emissions.',
            'Free re-test within 10 working days if your vehicle fails.',
            'Official VT20 certificate issued upon passing.'
        ]
    },
];