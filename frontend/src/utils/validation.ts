import * as yup from 'yup';

/** Optional text field: an empty input is sent to the API as null. */
export const optionalText = (max: number) =>
    yup.string().max(max, `At most ${String(max)} characters`).nullable().defined()
        .transform((value: string | null) => (value?.trim() ? value.trim() : null));

/** Number input that reports a friendly message when left empty. */
export const money = (label: string) =>
    yup.number().typeError(`${label} is required`).required(`${label} is required`)
        .min(0.01, `${label} must be more than £0`).max(10000, `${label} must be under £10,000`);
