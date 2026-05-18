import React, { useState } from 'react';
import { useStripe, useElements, PaymentElement } from '@stripe/react-stripe-js';
import toast from 'react-hot-toast';

interface CheckoutFormProps {
  amount: number;
}

const CheckoutForm: React.FC<CheckoutFormProps> = ({ amount }) => {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: `${window.location.origin}/deposit-success`,
      },
    });

    if (error) {
      toast.error(error.message || 'An unexpected error occurred.');
    }

    setIsProcessing(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement />
      <button
        disabled={isProcessing || !stripe || !elements}
        className="w-full bg-primary-600 hover:bg-primary-700 text-white py-4 rounded-xl font-black text-sm transition-all active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {isProcessing ? 'Processing...' : `Pay ${amount.toFixed(2)} PLN`}
      </button>
    </form>
  );
};

export default CheckoutForm;
