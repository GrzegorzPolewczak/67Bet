import React, { useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import { Elements } from "@stripe/react-stripe-js";
import { useDispatch } from "react-redux";
import type { AppDispatch } from "../../app/store";
import { createPaymentIntentAsync } from "./walletSlice";
import CheckoutForm from "./CheckoutForm";
import { ChevronLeft, CreditCard } from "lucide-react";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

const DepositPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const [amount, setAmount] = useState<number>(50);
  const [clientSecret, setClientSecret] = useState<string>("");
  const [publishableKey, setPublishableKey] = useState<string>("");
  const [loading, setLoading] = useState(false);

  const handleCreateIntent = async () => {
    if (amount < 10) {
      toast.error("Minimum deposit is 10 PLN");
      return;
    }

    setLoading(true);
    try {
      const resultAction = await dispatch(createPaymentIntentAsync(amount));
      if (createPaymentIntentAsync.fulfilled.match(resultAction)) {
        setClientSecret(resultAction.payload.clientSecret);
        setPublishableKey(resultAction.payload.publishableKey);
      } else {
        toast.error("Failed to initialize payment.");
      }
    } catch (error) {
      toast.error("An error occurred.");
    } finally {
      setLoading(false);
    }
  };

  const stripePromise = publishableKey ? loadStripe(publishableKey) : null;

  return (
    <div className="max-w-2xl mx-auto space-y-8 pb-12">
      <Link
        to="/"
        className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold"
      >
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white">Deposit Funds</h1>
        <p className="text-gray-400 text-sm">
          Top up your wallet to start betting.
        </p>
      </div>

      {!clientSecret ? (
        <section className="bg-dark-800 border border-dark-700 rounded-3xl p-8 space-y-6">
          <div>
            <label className="text-xs font-bold text-gray-500 uppercase px-1">
              Deposit Amount (PLN)
            </label>
            <div className="mt-2 grid grid-cols-4 gap-4">
              {[20, 50, 100, 200].map((val) => (
                <button
                  key={val}
                  onClick={() => setAmount(val)}
                  className={`py-3 rounded-xl font-bold transition-all ${
                    amount === val
                      ? "bg-primary-600 text-white border-primary-500"
                      : "bg-dark-900 text-gray-400 border-dark-600 hover:border-gray-500"
                  } border`}
                >
                  {val}
                </button>
              ))}
            </div>
            <div className="mt-4 relative">
              <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-bold">
                PLN
              </span>
              <input
                type="number"
                value={amount}
                onChange={(e) => setAmount(Number(e.target.value))}
                className="w-full bg-dark-900 border border-dark-600 rounded-xl py-4 pl-14 pr-4 text-white font-bold focus:outline-none focus:border-primary-500"
                placeholder="Custom amount"
              />
            </div>
          </div>

          <button
            onClick={handleCreateIntent}
            disabled={loading}
            className="w-full bg-primary-600 hover:bg-primary-700 text-white py-4 rounded-xl font-black text-sm flex items-center justify-center gap-2 transition-all active:scale-95 disabled:opacity-50"
          >
            {loading ? (
              "Processing..."
            ) : (
              <>
                <CreditCard className="w-4 h-4" /> Continue to Payment
              </>
            )}
          </button>
        </section>
      ) : (
        stripePromise && (
          <section className="bg-dark-800 border border-dark-700 rounded-3xl p-8">
            <Elements
              stripe={stripePromise}
              options={{ clientSecret, appearance: { theme: "night" } }}
            >
              <CheckoutForm amount={amount} />
            </Elements>
            <button
              onClick={() => setClientSecret("")}
              className="w-full mt-4 text-gray-500 hover:text-gray-300 text-xs font-bold transition-colors"
            >
              Cancel and change amount
            </button>
          </section>
        )
      )}
    </div>
  );
};

export default DepositPage;
