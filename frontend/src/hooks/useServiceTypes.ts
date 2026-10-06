import { useEffect, useState } from 'react';
import { getAllServiceTypes } from '../api/serviceTypeService';
import type { ServiceType } from '../types/serviceType';
import { getErrorMessage } from '../utils/format';

/** Loads the garage's service types (public endpoint). */
export const useServiceTypes = () => {
    const [serviceTypes, setServiceTypes] = useState<ServiceType[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        getAllServiceTypes()
            .then(types => { if (!cancelled) setServiceTypes(types); })
            .catch((err: unknown) => { if (!cancelled) setError(getErrorMessage(err, 'Could not load services.')); })
            .finally(() => { if (!cancelled) setIsLoading(false); });
        return () => { cancelled = true; };
    }, []);

    return { serviceTypes, isLoading, error };
};
