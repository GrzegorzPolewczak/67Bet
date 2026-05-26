import React, { useEffect, useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import * as signalR from '@microsoft/signalr';
import { identityApi } from '../../api/axios';
import { useDispatch } from 'react-redux';
import { setKycVerified } from './authSlice';

const DesktopVerification: React.FC = () => {
    const [sessionId, setSessionId] = useState<string | null>(null);
    const [isVerified, setIsVerified] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const dispatch = useDispatch();

    useEffect(() => {
        // 1. Fetch a new session ID from the backend
        const fetchSession = async () => {
            try {
                // Use identityApi which automatically attaches the JWT token
                const response = await identityApi.get('/kyc/session');
                const id = response.data.sessionId;
                setSessionId(id);

                // 2. Connect to SignalR Hub
                const identityUrl = import.meta.env.VITE_API_IDENTITY || 'http://localhost:5010/api';
                const hubUrl = identityUrl.replace('/api', '/hubs/verification');
                
                const connection = new signalR.HubConnectionBuilder()
                    .withUrl(hubUrl)
                    .withAutomaticReconnect()
                    .build();

                connection.on('VerificationCompleted', () => {
                    setIsVerified(true);
                    dispatch(setKycVerified()); // Sync the global state!
                });

                await connection.start();
                // 3. Join the session group
                await connection.invoke('JoinGroup', id);

            } catch (error: any) {
                console.error("Error setting up KYC session:", error);
                setError(error.response?.status === 401 
                    ? "You must be logged in to verify your identity." 
                    : "Failed to initialize verification session.");
            }
        };

        fetchSession();
    }, [dispatch]);

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
                <div className="p-8 bg-white rounded-lg shadow-lg text-center">
                    <h1 className="text-2xl font-bold text-red-600 mb-4">Error</h1>
                    <p className="text-gray-600">{error}</p>
                </div>
            </div>
        );
    }

    if (isVerified) {
        return (
            <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
                <div className="p-8 bg-white rounded-lg shadow-lg text-center">
                    <h1 className="text-4xl font-bold text-green-600 mb-4">Verification Successful</h1>
                    <p className="text-gray-600">Your identity has been verified successfully. You can now use all features.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
            <div className="p-8 bg-white rounded-lg shadow-lg text-center max-w-md">
                <h1 className="text-2xl font-bold text-gray-800 mb-4">Identity Verification</h1>
                <p className="text-gray-600 mb-6">
                    Please scan the QR code below using your mobile device to complete the verification process.
                </p>
                {sessionId ? (
                    <div className="flex justify-center p-4 bg-gray-100 rounded-lg">
                        <QRCodeSVG 
                            value={`${window.location.origin}/mobile/${sessionId}`} 
                            size={256} 
                        />
                    </div>
                ) : (
                    <div className="flex justify-center items-center h-64">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default DesktopVerification;
