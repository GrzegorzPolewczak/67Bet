import React, { useState } from "react";
import { useParams } from "react-router-dom";
import { identityApi } from "../../api/axios";

const MobileVerification: React.FC = () => {
  const { sessionId } = useParams<{ sessionId: string }>();
  const [idCard, setIdCard] = useState<File | null>(null);
  const [selfie, setSelfie] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isSuccess, setIsSuccess] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const handleVerify = async () => {
    if (!idCard || !selfie) {
      setError("Please upload both an ID card and a selfie.");
      return;
    }

    setIsLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append("idCard", idCard);
    formData.append("selfie", selfie);

    try {
      await identityApi.post(`/kyc/verify/${sessionId}`, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      setIsSuccess(true);
    } catch (err) {
      console.error("Verification failed:", err);
      setError("Verification failed. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  if (isSuccess) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50 p-4">
        <div className="text-center">
          <div className="text-green-500 text-6xl mb-4">✓</div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2">Done!</h1>
          <p className="text-gray-600">Look back at your computer screen.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col min-h-screen bg-gray-50 p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-6 text-center">
        Mobile Verification
      </h1>

      {error && (
        <div className="bg-red-100 text-red-700 p-3 rounded mb-4">{error}</div>
      )}

      <div className="mb-6">
        <label className="block text-gray-700 font-bold mb-2">1. ID Card</label>
        <div className="relative border-2 border-dashed border-gray-300 rounded-lg p-6 flex flex-col items-center justify-center bg-white">
          <input
            type="file"
            accept="image/*"
            capture="environment"
            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            onChange={(e) =>
              setIdCard(e.target.files ? e.target.files[0] : null)
            }
          />
          <div className="text-gray-500 text-center">
            {idCard ? (
              <span className="text-green-600 font-semibold">
                {idCard.name}
              </span>
            ) : (
              <span>Tap to take a photo of your ID</span>
            )}
          </div>
        </div>
      </div>

      <div className="mb-8">
        <label className="block text-gray-700 font-bold mb-2">2. Selfie</label>
        <div className="relative border-2 border-dashed border-gray-300 rounded-lg p-6 flex flex-col items-center justify-center bg-white">
          <input
            type="file"
            accept="image/*"
            capture="user"
            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            onChange={(e) =>
              setSelfie(e.target.files ? e.target.files[0] : null)
            }
          />
          <div className="text-gray-500 text-center">
            {selfie ? (
              <span className="text-green-600 font-semibold">
                {selfie.name}
              </span>
            ) : (
              <span>Tap to take a selfie</span>
            )}
          </div>
        </div>
      </div>

      <button
        onClick={handleVerify}
        disabled={isLoading}
        className={`w-full py-3 px-4 text-white font-bold rounded-lg shadow-md focus:outline-none focus:ring-2 focus:ring-blue-400 focus:ring-opacity-75 ${
          isLoading
            ? "bg-blue-300 cursor-not-allowed"
            : "bg-blue-600 hover:bg-blue-700"
        }`}
      >
        {isLoading ? "Verifying..." : "Verify"}
      </button>
    </div>
  );
};

export default MobileVerification;
