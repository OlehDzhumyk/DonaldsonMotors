import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import NoteForm from './NoteForm';

const setup = (minLength?: number) => {
    const onSubmit = vi.fn(() => Promise.resolve());
    render(<NoteForm label="Reason" minLength={minLength} submitLabel="Cancel booking" onSubmit={onSubmit} onCancel={vi.fn()} error={null} />);
    return { onSubmit, user: userEvent.setup() };
};

describe('NoteForm', () => {
    it('asks for a longer note when one is required', async () => {
        const { onSubmit, user } = setup(10);

        await user.type(screen.getByLabelText('Reason'), 'too short');
        await user.click(screen.getByRole('button', { name: 'Cancel booking' }));

        expect(screen.getByText('Please write at least 10 characters.')).toBeInTheDocument();
        expect(onSubmit).not.toHaveBeenCalled();
    });

    it('submits the trimmed note', async () => {
        const { onSubmit, user } = setup(10);

        await user.type(screen.getByLabelText('Reason'), '  Car was sold last week  ');
        await user.click(screen.getByRole('button', { name: 'Cancel booking' }));

        expect(onSubmit).toHaveBeenCalledWith('Car was sold last week');
    });

    it('sends null when an optional note is left empty', async () => {
        const { onSubmit, user } = setup();

        await user.click(screen.getByRole('button', { name: 'Cancel booking' }));

        expect(onSubmit).toHaveBeenCalledWith(null);
    });
});
