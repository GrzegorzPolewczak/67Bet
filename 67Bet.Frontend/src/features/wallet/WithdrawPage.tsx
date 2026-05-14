import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../app/store';
import { withdrawAsync } from './walletSlice';
import { ChevronLeft, Banknote } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';

const WithdrawPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { balance } = useSelector((state: RootState) => state.wallet);
  
  const [amount, setAmount] = useState<number>(50);
  const [loading, setLoading] = useState(false);

  const handleWithdraw = async () => {
    if (amount < 10) {
      toast.error('Minimum withdrawal is 10 PLN');
      return;
    }

    if (amount > balance) {
      toast.error('Insufficient funds in your wallet');
      return;
    }

    setLoading(true);
    try {
      const resultAction = await dispatch(withdrawAsync(amount));
      if (withdrawAsync.fulfilled.match(resultAction)) {
        toast.success(`Successfully withdrawn ${amount} PLN`);
        navigate('/');
      } else {
        const error = resultAction.payload as string;
        toast.error(error || 'Failed to process withdrawal.');
      }
    } catch (error) {
      toast.error('An error occurred.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto space-y-8 pb-12">
      <Link to="/" className="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors text-sm font-bold">
        <ChevronLeft className="w-4 h-4" /> Back to Betting
      </Link>

      <div>
        <h1 className="text-3xl font-black text-white">Withdraw Funds</h1>
        <p className="text-gray-400 text-sm">Transfer funds from your wallet back to your account.</p>
      </div>

      <section className="bg-dark-800 border border-dark-700 rounded-3xl p-8 space-y-6">
        <div className="flex justify-between items-center bg-dark-900/50 p-4 rounded-2xl border border-dark-600">
          <span className="text-gray-400 text-sm font-bold">Available Balance</span>
          <span className="text-white text-xl font-black">{balance.toFixed(2)} PLN</span>
        </div>

        <div>
          <label className="text-xs font-bold text-gray-500 uppercase px-1">Withdrawal Amount (PLN)</label>
          <div className="mt-2 grid grid-cols-4 gap-4">
            {[20, 50, 100, 200].map((val) => (
              <button
                key={val}
                onClick={() => setAmount(val)}
                className={`py-3 rounded-xl font-bold transition-all ${
                  amount === val 
                    ? 'bg-primary-600 text-white border-primary-500' 
                    : 'bg-dark-900 text-gray-400 border-dark-600 hover:border-gray-500'
                } border`}
              >
                {val}
              </button>
            ))}
          </div>
          <div className="mt-4 relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-bold">PLN</span>
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
          onClick={handleWithdraw}
          disabled={loading || amount <= 0 || amount > balance}
          className="w-full bg-primary-600 hover:bg-primary-700 text-white py-4 rounded-xl font-black text-sm flex items-center justify-center gap-2 transition-all active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {loading ? 'Processing...' : (
            <>
              <Banknote className="w-4 h-4" /> Confirm Withdrawal
            </>
          )}
        </button>
        
        {amount > balance && (
          <p className="text-red-400 text-xs text-center font-bold italic">
            Insufficient funds to withdraw this amount.
          </p>
        )}
      </section>

      <div className="bg-dark-800/50 border border-dark-700 rounded-2xl p-6">
        <h3 className="text-white font-bold mb-2">Important Information</h3>
        <ul className="text-gray-400 text-xs space-y-2 list-disc pl-4">
          <li>Withdrawals are processed instantly in test mode.</li>
          <li>The minimum withdrawal amount is 10.00 PLN.</li>
        </ul>
      </div>
    </div>
  );
};

export default WithdrawPage;
