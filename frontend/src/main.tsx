import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { Provider } from 'react-redux';
import App from './App';
import { store } from './app/store';
import { logout } from './app/authSlice';
import { setupApiClient } from './api/apiClient';
import './index.css';

setupApiClient({
    getToken: () => store.getState().auth.user?.token,
    onUnauthorized: () => store.dispatch(logout()),
});

const root = document.getElementById('root');
if (!root) throw new Error('Missing #root element');

ReactDOM.createRoot(root).render(
    <React.StrictMode>
        <Provider store={store}>
            <BrowserRouter>
                <App />
            </BrowserRouter>
        </Provider>
    </React.StrictMode>
);
