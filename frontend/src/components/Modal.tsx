import React, { useEffect } from 'react';

interface ModalProps {
    title: string;
    onClose: () => void;
    children: React.ReactNode;
}

/** A centred dialog. Children supply their own .modal-body and .modal-footer. */
const Modal: React.FC<ModalProps> = ({ title, onClose, children }) => {
    useEffect(() => {
        const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
        window.addEventListener('keydown', onKey);
        return () => { window.removeEventListener('keydown', onKey); };
    }, [onClose]);

    return (
        <>
            <div className="modal-overlay" onClick={onClose} />
            <div className="modal" role="dialog" aria-modal="true" aria-label={title}>
                <div className="modal-header">
                    <h2>{title}</h2>
                    <button type="button" className="icon-btn" onClick={onClose} aria-label="Close">&times;</button>
                </div>
                {children}
            </div>
        </>
    );
};

export default Modal;
