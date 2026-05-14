import React, { useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { ChevronLeft, Loader2, Activity } from 'lucide-react';
import type { RootState, AppDispatch } from '../../app/store';
import { fetchEventsAsync } from './bettingSlice';
import { addSelection, removeSelection } from '../betslip/betslipSlice';
import { updateMatchState, clearMatchState, setConnectionStatus } from './liveTrackerSlice';
import { startSignalRConnection, subscribeToMatch, unsubscribeFromMatch, onMatchUpdate, offMatchUpdate } from '../../api/signalr';
import OddButton from './OddButton';

const MatchDetailsView: React.FC = () => {
  const { matchId } = useParams<{ matchId: string }>();
  const dispatch = useDispatch<AppDispatch>();
  const { events } = useSelector((state: RootState) => state.betting);
  const { currentMatch } = useSelector((state: RootState) => state.liveTracker);
  const selections = useSelector((state: RootState) => state.betslip.selections);

  const event = events.find(e => e.id === matchId);
  const isSelected = (outcomeId: string) => selections.some(s => s.outcomeId === outcomeId);


  useEffect(() => {
    if (!event) {
      dispatch(fetchEventsAsync());
    }
  }, [dispatch, event]);

  useEffect(() => {
    if (matchId) {
      const setupSignalR = async () => {
        await startSignalRConnection();
        dispatch(setConnectionStatus(true));
        await subscribeToMatch(matchId);
        
        onMatchUpdate((update) => {
          dispatch(updateMatchState(update));
        });
      };

      setupSignalR();

      return () => {
        unsubscribeFromMatch(matchId);
        offMatchUpdate();
        dispatch(clearMatchState());
      };
    }
  }, [matchId, dispatch]);

  if (!event) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4">
        <Loader2 className="w-10 h-10 text-primary-500 animate-spin" />
        <p className="text-gray-400 font-bold">Loading match details...</p>
      </div>
    );
  }

  // Polimorfizm UI na podstawie SportKey
  const renderTracker = () => {
    if (!currentMatch) return <div className="text-center text-gray-500 py-10 flex flex-col items-center justify-center h-full"><Loader2 className="w-8 h-8 text-primary-500 animate-spin mb-2" />Waiting for live data...</div>;

    return (
      <div className="relative w-full h-full min-h-[240px] bg-green-900/20 border-2 border-green-800/50 rounded-2xl overflow-hidden flex flex-col items-center justify-center p-4">
        {/* Placeholder dla "boiska" - w przyszłości grafika tła */}
        <div className="absolute inset-0 flex items-center justify-center opacity-10">
          <Activity className="w-32 h-32 text-green-500" />
        </div>
        
        <div className="z-10 text-center w-full max-w-md mx-auto">
          <div className="flex justify-between items-center bg-dark-900/80 p-3 rounded-xl border border-dark-700 backdrop-blur-sm mb-4">
             <div className="text-xl font-black text-white">{currentMatch.score?.Home ?? 0}</div>
             <div className="text-sm font-bold text-primary-500 uppercase tracking-widest">{currentMatch.currentTime}</div>
             <div className="text-xl font-black text-white">{currentMatch.score?.Away ?? 0}</div>
          </div>
          
          <div className="bg-primary-600 text-white font-bold py-2 px-6 rounded-full shadow-lg shadow-primary-500/20 animate-pulse inline-block">
             {currentMatch.currentAction}
          </div>

          <div className="mt-6 flex justify-center flex-wrap gap-4 text-xs font-medium text-gray-400">
            {Object.entries(currentMatch.statistics || {}).map(([key, val]) => (
               <div key={key} className="bg-dark-800/80 px-3 py-1.5 rounded-lg border border-dark-700">
                 <span className="uppercase text-[10px] text-gray-500 block mb-0.5">{key}</span>
                 <span className="text-white font-bold text-sm">{val}</span>
               </div>
            ))}
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="max-w-6xl mx-auto space-y-6 pb-12">
      <div className="flex items-center gap-4 mb-2">
        <Link to="/" className="text-gray-500 hover:text-white transition-colors">
          <ChevronLeft className="w-5 h-5" />
        </Link>
        <div>
          <h1 className="text-2xl font-black text-white">{event.name}</h1>
          <p className="text-sm text-gray-400 font-bold">{event.league}</p>
        </div>
      </div>

      {/* Grid: Tracker na lewo, Rynki na prawo */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Sekcja Live Trackera */}
        <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6 flex flex-col">
          <div className="flex items-center gap-2 mb-4">
            <div className="w-2 h-2 bg-red-500 rounded-full animate-pulse" />
            <h2 className="text-sm font-bold text-white uppercase tracking-wider">Live Match Tracker</h2>
          </div>
          <div className="flex-1">
            {renderTracker()}
          </div>
        </div>

        {/* Sekcja Kursów */}
        <div className="bg-dark-800 border border-dark-700 rounded-3xl p-6 flex flex-col h-full max-h-[600px]">
          <h2 className="text-sm font-bold text-white uppercase tracking-wider mb-4">Available Markets</h2>
          <div className="space-y-4 overflow-y-auto pr-2 custom-scrollbar flex-1">
            {Array.isArray(event.markets) && event.markets.length > 0 ? (
              event.markets.map((market: any, mIndex: number) => (
                <div key={market.id || mIndex} className="bg-dark-900 p-4 rounded-2xl border border-dark-700">
                  <h3 className="text-xs font-bold text-gray-500 uppercase mb-3">{market.name}</h3>
                  <div className="flex flex-wrap gap-2">
                    {Array.isArray(market.outcomes) && market.outcomes.map((outcome: any, oIndex: number) => (
                      <OddButton
                        key={outcome.id || oIndex}
                        name={outcome.name || '-'}
                        odd={outcome.odd || 0}
                        isSelected={outcome.id ? isSelected(outcome.id) : false}
                        onClick={() => {
                          if (outcome.id) {
                            if (isSelected(outcome.id)) {
                              dispatch(removeSelection(outcome.id));
                            } else {
                              dispatch(addSelection({
                                eventId: event.id,
                                eventName: event.name || 'Unknown Event',
                                marketId: market.id,
                                marketName: market.name || 'Unknown Market',
                                outcomeId: outcome.id,
                                outcomeName: outcome.name === '1' ? (event.name?.split(' vs ')[0] || 'Team 1') : outcome.name === '2' ? (event.name?.split(' vs ')[1] || 'Team 2') : outcome.name,
                                odd: outcome.odd || 0
                              }));
                            }
                          }
                        }}
                      />
                    ))}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center text-gray-500 py-10 border border-dashed border-dark-600 rounded-xl">
                No odds available for this event yet.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default MatchDetailsView;
