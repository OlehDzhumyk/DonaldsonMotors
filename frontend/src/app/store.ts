import { configureStore } from '@reduxjs/toolkit';
import authReducerFromSlice from './authSlice';

export const store = configureStore({
    reducer: {
        auth: authReducerFromSlice,
        // posts: postsReducer,
        // comments: commentsReducer,
    },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;