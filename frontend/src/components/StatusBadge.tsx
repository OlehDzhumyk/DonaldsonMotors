import React from 'react';
import { statusLabel } from '../utils/format';

const StatusBadge: React.FC<{ status: string }> = ({ status }) => (
    <span className={`badge badge-${status.toLowerCase()}`}>{statusLabel(status)}</span>
);

export default StatusBadge;
