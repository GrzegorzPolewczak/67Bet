import React, { useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { CheckCircle, ChevronLeft } from "lucide-react";
import { useDispatch } from "react-redux";
import type { AppDispatch } from "../../app/store";
import { fetchBalanceAsync } from "./walletSlice";
import toast from "react-hot-toast";

const DepositSuccessPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const dispatch = useDispatch<AppDispatch>();
  const paymentIntentClientSecret = searchParams.get(
    "payment_intent_client_secret",
  );

  useEffect(() => {
    if (paymentIntentClientSecret) {
      toast.success("Payment processed! Updating your balance...");
      // Refresh balance after successful payment
      const timer = setTimeout(() => {
        dispatch(fetchBalanceAsync());
      }, 2000);
      return () => clearTimeout(timer);
    }
  }, [paymentIntentClientSecret, dispatch]);

  return (
    <div className="max-w-2xl mx-auto text-center space-y-8 py-20">
      <div className="flex justify-center">
        <div className="w-20 h-20 bg-accent-success/20 rounded-full flex items-center justify-center">
          <CheckCircle className="w-12 h-12 text-accent-success" />
        </div>
      </div>

      <div>
        <h1 className="text-4xl font-black text-white">Deposit Successful!</h1>
        <p className="text-gray-400 mt-2 text-lg">
          Your funds are being added to your wallet.
        </p>
      </div>

      <div className="pt-8">
        <Link
          to="/"
          className="inline-flex items-center gap-2 bg-primary-600 hover:bg-primary-700 text-white px-8 py-4 rounded-xl font-black transition-all active:scale-95"
        >
          <ChevronLeft className="w-4 h-4" /> Back to Betting
        </Link>
      </div>
    </div>
  );
};

export default DepositSuccessPage;
